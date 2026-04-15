param(
    [string]$MatrixPath = "TraceabilityMatrix.md",
    [string]$GateCReportPath = "artifacts/gate-c/2026-04-10-gate-c-validation.md",
    [string]$OutputPath = "artifacts/traceability/2026-04-10-dashboard-validation.md"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $MatrixPath)) {
    throw "Traceability matrix not found: $MatrixPath"
}

$lines = Get-Content $MatrixPath
$dashboard = @{}
$inDashboard = $false
foreach ($line in $lines) {
    if ($line -match '^##\s+1\.\s+Status Dashboard') {
        $inDashboard = $true
        continue
    }

    if ($inDashboard -and $line -match '^##\s+') {
        break
    }

    if (-not $inDashboard) {
        continue
    }

    if ($line -notmatch '^\|') {
        continue
    }

    if ($line -match '^\|---') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 4) {
        continue
    }

    $metric = $parts[1].Trim()
    $value = $parts[2].Trim()

    if ($metric -eq 'Metric' -or [string]::IsNullOrWhiteSpace($metric)) {
        continue
    }

    $dashboard[$metric] = $value
}

if ($dashboard.Count -eq 0) {
    throw "No dashboard rows found in section 'Status Dashboard'."
}

$functionRows = @()
$inFunctionTable = $false
foreach ($line in $lines) {
    if ($line -match '^##\s+3\.\s+Function Traceability Matrix') {
        $inFunctionTable = $true
        continue
    }

    if ($inFunctionTable -and $line -match '^##\s+') {
        break
    }

    if (-not $inFunctionTable) {
        continue
    }

    if ($line -notmatch '^\|\s*F\d+\s*\|') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 13) {
        continue
    }

    $functionRows += [PSCustomObject]@{
        Id = $parts[1].Trim()
        Source = $parts[3].Trim()
        Implementation = $parts[5].Trim()
        Test = $parts[6].Trim()
        Performance = $parts[7].Trim()
        Status = $parts[8].Trim()
        Owner = $parts[9].Trim()
        Priority = $parts[10].Trim()
        Batch = $parts[11].Trim()
    }
}

if ($functionRows.Count -eq 0) {
    throw "No function rows found in 'Function Traceability Matrix'."
}

$duplicateIds = @($functionRows | Group-Object Id | Where-Object { $_.Count -gt 1 })

$totalFunctions = $functionRows.Count
$withImplementation = @($functionRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Implementation) -and $_.Implementation -ne '-' }).Count
$withTest = @($functionRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Test) -and $_.Test -ne '-' }).Count
$withPerformance = @($functionRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Performance) -and $_.Performance -ne '-' }).Count
$unassignedOwner = @($functionRows | Where-Object { [string]::IsNullOrWhiteSpace($_.Owner) -or $_.Owner -eq '-' }).Count
$p0Items = @($functionRows | Where-Object { $_.Priority -eq 'P0' }).Count
$batch1 = @($functionRows | Where-Object { $_.Batch -eq 'Batch-1' }).Count
$batch2 = @($functionRows | Where-Object { $_.Batch -eq 'Batch-2' }).Count
$batch3 = @($functionRows | Where-Object { $_.Batch -eq 'Batch-3' }).Count
$batch4 = @($functionRows | Where-Object { $_.Batch -eq 'Batch-4' }).Count
$openBacklog = @($functionRows | Where-Object { $_.Status -ne 'Passed' }).Count

$gateCStatus = 'Failed'
if (Test-Path $GateCReportPath) {
    $gateCText = Get-Content $GateCReportPath -Raw
    if ($gateCText -match 'Status:\s*PASSED') {
        $gateCStatus = 'Passed'
    }
}

$expected = [ordered]@{
    'Total functions' = [string]$totalFunctions
    'With implementation node' = [string]$withImplementation
    'With test node' = [string]$withTest
    'With performance node' = [string]$withPerformance
    'Unassigned owner' = [string]$unassignedOwner
    'P0 items' = [string]$p0Items
    'Batch-1 tasks' = [string]$batch1
    'Batch-2 tasks' = [string]$batch2
    'Batch-3 tasks' = [string]$batch3
    'Batch-4 tasks' = [string]$batch4
    'Open backlog items' = [string]$openBacklog
    'W4 / Gate C' = $gateCStatus
}

$mismatches = @()
foreach ($metric in $expected.Keys) {
    if (-not $dashboard.ContainsKey($metric)) {
        $mismatches += "Missing dashboard metric: $metric"
        continue
    }

    $actual = $dashboard[$metric]
    $target = $expected[$metric]

    if ($metric -eq 'W4 / Gate C') {
        if ($actual -ne $target) {
            $mismatches += "Metric '$metric' mismatch. Dashboard='$actual', Expected='$target'"
        }
        continue
    }

    $actualNumber = 0
    $targetNumber = 0
    $parsedActual = [int]::TryParse($actual, [ref]$actualNumber)
    $parsedTarget = [int]::TryParse($target, [ref]$targetNumber)

    if (-not ($parsedActual -and $parsedTarget -and $actualNumber -eq $targetNumber)) {
        $mismatches += "Metric '$metric' mismatch. Dashboard='$actual', Expected='$target'"
    }
}

if ($duplicateIds.Count -gt 0) {
    $mismatches += "Duplicate Function IDs found: $($duplicateIds.Name -join ', ')"
}

if ($withImplementation -ne $totalFunctions -or $withTest -ne $totalFunctions) {
    $mismatches += 'Traceability is not 100% for implementation/test coverage.'
}

$outDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outDir)) {
    New-Item -Path $outDir -ItemType Directory -Force | Out-Null
}

$report = @(
    '# Traceability Dashboard Validation Report',
    '',
    '- Date: 2026-04-10',
    "- Matrix path: $MatrixPath",
    "- Gate C report path: $GateCReportPath",
    "- Function rows parsed: $totalFunctions",
    "- Dashboard metrics parsed: $($dashboard.Count)",
    "- Validation status: $(if ($mismatches.Count -eq 0) { 'PASSED' } else { 'FAILED' })",
    '',
    '## Computed Metrics',
    '',
    '| Metric | Dashboard | Computed |',
    '|---|---:|---:|'
)

foreach ($metric in $expected.Keys) {
    $actual = if ($dashboard.ContainsKey($metric)) { $dashboard[$metric] } else { '-' }
    $computed = $expected[$metric]
    $report += "| $metric | $actual | $computed |"
}

$report += ''

if ($duplicateIds.Count -gt 0) {
    $report += '## Duplicate Function IDs'
    $report += ''
    foreach ($g in $duplicateIds) {
        $report += "- $($g.Name): $($g.Count)"
    }
    $report += ''
}

if ($mismatches.Count -gt 0) {
    $report += '## Mismatches'
    $report += ''
    foreach ($m in $mismatches) {
        $report += "- $m"
    }
    $report += ''
}

Set-Content -Path $OutputPath -Encoding UTF8 -Value $report
Write-Output "Traceability dashboard report generated: $OutputPath"

if ($mismatches.Count -gt 0) {
    throw "Traceability dashboard validation failed. See report: $OutputPath"
}

Write-Output 'Traceability dashboard validation passed.'
