param(
    [Parameter(Mandatory = $true)]
    [string]$CurrentReportPath,

    [string]$BaselineReportPath,

    [double]$MaxLatencyRegressionPercent = 10,

    [double]$MaxAllocationRegressionPercent = 25,

    [long]$MaxAllocationIncreaseBytes = 128,

    [string[]]$RequiredFunctions = @(),

    [string]$SummaryOutputPath
)

$ErrorActionPreference = 'Stop'

if ($MaxLatencyRegressionPercent -lt 0) {
    throw 'MaxLatencyRegressionPercent must be non-negative.'
}

if ($MaxAllocationRegressionPercent -lt 0) {
    throw 'MaxAllocationRegressionPercent must be non-negative.'
}

if ($MaxAllocationIncreaseBytes -lt 0) {
    throw 'MaxAllocationIncreaseBytes must be non-negative.'
}

if (-not (Test-Path $CurrentReportPath)) {
    throw "Current report not found: $CurrentReportPath"
}

function Get-Metrics {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $metrics = @{}
    $lines = Get-Content $Path

    foreach ($line in $lines) {
        if ($line -notmatch '^\|\s*F\d+\s') {
            continue
        }

        $parts = $line.Split('|')
        if ($parts.Count -lt 5) {
            continue
        }

        $functionName = $parts[1].Trim()
        $latencyText = $parts[3].Trim()
        $allocationText = $parts[4].Trim()

        $latency = 0.0
        $latencyOk = [double]::TryParse(
            $latencyText,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$latency)

        if (-not $latencyOk) {
            continue
        }

        $allocation = 0L
        $allocationOk = [long]::TryParse(
            $allocationText,
            [System.Globalization.NumberStyles]::Integer,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$allocation)

        if (-not $allocationOk) {
            continue
        }

        $metrics[$functionName] = [PSCustomObject]@{
            LatencyMs = $latency
            AllocationBytes = $allocation
        }
    }

    return $metrics
}

function Write-SummarySection {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [System.Collections.IEnumerable]$Rows
    )

    $lines = @(
        "### $Title",
        "",
        "| Function | Baseline Latency (ms) | Current Latency (ms) | Delta Latency (%) | Baseline Alloc (bytes) | Current Alloc (bytes) | Delta Alloc (bytes) | Status |",
        "|---|---:|---:|---:|---:|---:|---:|---|"
    )

    $rowCount = 0
    foreach ($row in $Rows) {
        $rowCount++
        $lines += "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} |" -f $row.Function, $row.BaselineLatency, $row.CurrentLatency, $row.DeltaLatency, $row.BaselineAlloc, $row.CurrentAlloc, $row.DeltaAlloc, $row.Status
    }

    if ($rowCount -eq 0) {
        $lines += "| (no parsed rows) | - | - | - | - | - | - | skipped |"
    }

    $lines += ""
    Add-Content -Path $Path -Value $lines
}

$currentMetrics = Get-Metrics -Path $CurrentReportPath
if ($currentMetrics.Count -eq 0) {
    throw "No benchmark rows parsed from current report: $CurrentReportPath"
}

$missingRequired = @()
foreach ($required in $RequiredFunctions) {
    if (-not $currentMetrics.ContainsKey($required)) {
        $missingRequired += $required
    }
}

if ($missingRequired.Count -gt 0) {
    throw ("Missing required benchmark functions in current report: " + ($missingRequired -join ', '))
}

$summaryRows = @()
$reportName = Split-Path -Path $CurrentReportPath -Leaf
$hasBaseline = -not [string]::IsNullOrWhiteSpace($BaselineReportPath) -and (Test-Path $BaselineReportPath)

if (-not $hasBaseline) {
    foreach ($functionName in ($currentMetrics.Keys | Sort-Object)) {
        $current = $currentMetrics[$functionName]

        $summaryRows += [PSCustomObject]@{
            Function = $functionName
            BaselineLatency = "-"
            CurrentLatency = ("{0:F4}" -f $current.LatencyMs)
            DeltaLatency = "-"
            BaselineAlloc = "-"
            CurrentAlloc = $current.AllocationBytes
            DeltaAlloc = "-"
            Status = "baseline-missing"
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($SummaryOutputPath)) {
        Write-SummarySection -Path $SummaryOutputPath -Title "Perf Regression Check: $reportName" -Rows $summaryRows
    }

    Write-Output "Baseline report not available for $CurrentReportPath; threshold check skipped."
    exit 0
}

$baselineMetrics = Get-Metrics -Path $BaselineReportPath

if ($baselineMetrics.Count -eq 0) {
    throw "No benchmark rows parsed from baseline report: $BaselineReportPath"
}

$violations = @()

foreach ($functionName in $currentMetrics.Keys) {
    $current = $currentMetrics[$functionName]

    if (-not $baselineMetrics.ContainsKey($functionName)) {
        $summaryRows += [PSCustomObject]@{
            Function = $functionName
            BaselineLatency = "-"
            CurrentLatency = ("{0:F4}" -f $current.LatencyMs)
            DeltaLatency = "-"
            BaselineAlloc = "-"
            CurrentAlloc = $current.AllocationBytes
            DeltaAlloc = "-"
            Status = "missing-baseline-row"
        }
        continue
    }

    $baseline = $baselineMetrics[$functionName]

    $latencyDeltaPercent = 0.0
    if ($baseline.LatencyMs -gt 0) {
        $latencyDeltaPercent = (($current.LatencyMs - $baseline.LatencyMs) / $baseline.LatencyMs) * 100.0
    }

    $allocationDeltaBytes = $current.AllocationBytes - $baseline.AllocationBytes
    $allocationDeltaPercent = 0.0
    if ($baseline.AllocationBytes -gt 0) {
        $allocationDeltaPercent = (($current.AllocationBytes - $baseline.AllocationBytes) / [double]$baseline.AllocationBytes) * 100.0
    }

    $status = 'ok'

    if ($latencyDeltaPercent -gt $MaxLatencyRegressionPercent) {
        $status = 'latency-regression'
        $violations += "$functionName latency regressed by $([Math]::Round($latencyDeltaPercent, 2))% (baseline=$($baseline.LatencyMs) ms, current=$($current.LatencyMs) ms)"
    }

    if ($allocationDeltaBytes -gt $MaxAllocationIncreaseBytes) {
        $status = 'allocation-regression'
        $violations += "$functionName allocation increased by $allocationDeltaBytes bytes (baseline=$($baseline.AllocationBytes), current=$($current.AllocationBytes), max increase=$MaxAllocationIncreaseBytes)"
    }
    elseif ($baseline.AllocationBytes -gt 0 -and $allocationDeltaPercent -gt $MaxAllocationRegressionPercent) {
        $status = 'allocation-regression'
        $violations += "$functionName allocation regressed by $([Math]::Round($allocationDeltaPercent, 2))% (baseline=$($baseline.AllocationBytes), current=$($current.AllocationBytes))"
    }

    $summaryRows += [PSCustomObject]@{
        Function = $functionName
        BaselineLatency = ("{0:F4}" -f $baseline.LatencyMs)
        CurrentLatency = ("{0:F4}" -f $current.LatencyMs)
        DeltaLatency = ("{0:F2}" -f $latencyDeltaPercent)
        BaselineAlloc = $baseline.AllocationBytes
        CurrentAlloc = $current.AllocationBytes
        DeltaAlloc = $allocationDeltaBytes
        Status = $status
    }
}

if (-not [string]::IsNullOrWhiteSpace($SummaryOutputPath)) {
    Write-SummarySection -Path $SummaryOutputPath -Title "Perf Regression Check: $reportName" -Rows ($summaryRows | Sort-Object Function)
}

if ($violations.Count -gt 0) {
    throw ("Performance regression threshold exceeded:`n" + ($violations -join "`n"))
}

Write-Output "Perf threshold check passed for $CurrentReportPath against baseline $BaselineReportPath (latency <= $MaxLatencyRegressionPercent%, allocation increase <= $MaxAllocationIncreaseBytes bytes)."
