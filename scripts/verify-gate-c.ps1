[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', 'Parse-PerfRows', Justification='Legacy analyzer cache false positive after helper refactor.')]
param()

$ErrorActionPreference = 'Stop'

$requiredFiles = @(
    'PerfGate.md',
    'UnsafeRationale.md',
    'artifacts/perf/2026-04-10-batch1.md',
    'artifacts/perf/2026-04-10-batch2.md',
    'artifacts/perf/2026-04-10-batch3.md',
    'Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj',
    'scripts/verify-perf-regression.ps1'
)

$missing = @()
foreach ($path in $requiredFiles) {
    if (-not (Test-Path $path)) {
        $missing += $path
    }
}

if ($missing.Count -gt 0) {
    throw ("Missing required W4/Gate C files: " + ($missing -join ', '))
}

$rules = @(2, 3, 7, 10, 21, 23, 25, 30, 31)
$ruleText = Get-Content 'RuleComplianceChecklist.md' -Raw
$missingRulePass = @()
foreach ($rule in $rules) {
    if ($ruleText -notmatch ("\| {0} \| .*\| Passed \|" -f $rule)) {
        $missingRulePass += $rule
    }
}

if ($missingRulePass.Count -gt 0) {
    throw ("Gate C blocking rules not passed: " + ($missingRulePass -join ', '))
}

function Get-PerfRows {
    param([string]$Path)

    $rows = @{}
    foreach ($line in (Get-Content $Path)) {
        if ($line -notmatch '^\|\s*F\d+\s') {
            continue
        }

        $parts = $line.Split('|')
        if ($parts.Count -lt 5) {
            continue
        }

        $name = $parts[1].Trim()
        $latencyText = $parts[3].Trim()
        $allocText = $parts[4].Trim()

        $latency = 0.0
        $latencyOk = [double]::TryParse(
            $latencyText,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$latency)

        $alloc = 0L
        $allocOk = [long]::TryParse(
            $allocText,
            [System.Globalization.NumberStyles]::Integer,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$alloc)

        if ($latencyOk -and $allocOk) {
            $rows[$name] = [PSCustomObject]@{
                LatencyMs = $latency
                AllocationBytes = $alloc
            }
        }
    }

    return $rows
}

$thresholds = @{
    'F100 Axpy' = @{ Latency = 3.00; Alloc = 64 }
    'F101 Dotu' = @{ Latency = 3.50; Alloc = 64 }
    'F103 Gemv' = @{ Latency = 2.50; Alloc = 128 }
    'F104 Gemm' = @{ Latency = 35.00; Alloc = 128 }
    'F011 Reductions.Sum' = @{ Latency = 6.00; Alloc = 128 }
    'F202 RK45.Step' = @{ Latency = 0.03; Alloc = 640 }
    'F302 Brent.Solve' = @{ Latency = 0.01; Alloc = 64 }
    'F306 GradientDescent.SolveScalar' = @{ Latency = 0.01; Alloc = 64 }
    'F102 Norms.L2' = @{ Latency = 2.00; Alloc = 64 }
    'F105 Transpose.Matrix' = @{ Latency = 1.50; Alloc = 64 }
    'F203 JacobianEstimator.EstimateForwardDifference' = @{ Latency = 0.02; Alloc = 512 }
}

$allRows = @{}
foreach ($report in @('artifacts/perf/2026-04-10-batch1.md', 'artifacts/perf/2026-04-10-batch2.md', 'artifacts/perf/2026-04-10-batch3.md')) {
    $rows = Get-PerfRows -Path $report
    foreach ($key in $rows.Keys) {
        $allRows[$key] = $rows[$key]
    }
}

$perfViolations = @()
$tableRows = @()
foreach ($fn in ($thresholds.Keys | Sort-Object)) {
    if (-not $allRows.ContainsKey($fn)) {
        $perfViolations += "Missing benchmark row: $fn"
        $tableRows += "| $fn | - | - | - | - | missing |"
        continue
    }

    $metric = $allRows[$fn]
    $limit = $thresholds[$fn]

    $status = 'pass'
    if ($metric.LatencyMs -gt $limit.Latency -or $metric.AllocationBytes -gt $limit.Alloc) {
        $status = 'fail'
        $perfViolations += "$fn exceeds threshold (latency=$($metric.LatencyMs) ms limit=$($limit.Latency), allocation=$($metric.AllocationBytes) bytes limit=$($limit.Alloc))"
    }

    $tableRows += "| $fn | $([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, '{0:F4}', $metric.LatencyMs)) | $($limit.Latency) | $($metric.AllocationBytes) | $($limit.Alloc) | $status |"
}

$unsafeCount = (Get-ChildItem -Path src -Recurse -Filter *.cs | ForEach-Object { Select-String -Path $_.FullName -Pattern '\bunsafe\b' -SimpleMatch:$false } | Measure-Object).Count
$unsafeStatus = if ($unsafeCount -eq 0) { 'pass' } else { 'review-required' }

$gateStatus = if ($perfViolations.Count -eq 0 -and $missingRulePass.Count -eq 0) { 'PASSED' } else { 'FAILED' }

New-Item -ItemType Directory -Path 'artifacts/gate-c' -Force | Out-Null
$reportPath = 'artifacts/gate-c/2026-04-10-gate-c-validation.md'

$lines = @(
    '# Gate C Validation Report',
    '',
    '- Date: 2026-04-10',
    '- Scope: W4 Performance Engineering and Unsafe Enablement Gate',
    "- Status: $gateStatus",
    '',
    '## Rule Gate Checks',
    '',
    "- Required passed rules: $($rules -join ', ')",
    "- Rule check result: " + ($(if ($missingRulePass.Count -eq 0) { 'pass' } else { 'fail' })),
    '',
    '## Hot Path Threshold Checks',
    '',
    '| Function | Latency (ms) | Latency Limit (ms) | Allocation (bytes) | Allocation Limit (bytes) | Status |',
    '|---|---:|---:|---:|---:|---|'
)

$lines += $tableRows
$lines += ''
$lines += '## Unsafe Policy Snapshot'
$lines += ''
$lines += "- unsafe keyword occurrences in src/**: $unsafeCount"
$lines += "- Status: $unsafeStatus"
$lines += ''

if ($perfViolations.Count -gt 0) {
    $lines += '## Perf Violations'
    $lines += ''
    foreach ($item in $perfViolations) {
        $lines += "- $item"
    }
    $lines += ''
}

$lines += '## Evidence Links'
$lines += ''
$lines += '- PerfGate.md'
$lines += '- UnsafeRationale.md'
$lines += '- artifacts/perf/2026-04-10-batch1.md'
$lines += '- artifacts/perf/2026-04-10-batch2.md'
$lines += '- artifacts/perf/2026-04-10-batch3.md'
$lines += '- scripts/verify-perf-regression.ps1'
$lines += '- Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj'

Set-Content -Path $reportPath -Value $lines
Write-Output "Gate C report generated: $reportPath"

if ($gateStatus -ne 'PASSED') {
    throw 'Gate C validation failed. See report for details.'
}
