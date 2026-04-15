param(
    [string]$MatrixPath = "TraceabilityMatrix.md",
    [string]$UsageSamplesPath = "UsageSamples.md",
    [string]$W3ReportPath = "artifacts/w3/2026-04-10-w3-validation.md",
    [string]$PerfAuditPath = "artifacts/perf/perf-node-audit.md",
    [string]$GateCReportPath = "artifacts/gate-c/2026-04-10-gate-c-validation.md",
    [string]$OutputPath = "artifacts/w3-w4/2026-04-10-module-coverage-rerun.md"
)

$ErrorActionPreference = "Stop"

$requiredFiles = @($MatrixPath, $UsageSamplesPath, $W3ReportPath, $PerfAuditPath, $GateCReportPath)
$missing = @()
foreach ($path in $requiredFiles) {
    if (-not (Test-Path $path)) {
        $missing += $path
    }
}

if ($missing.Count -gt 0) {
    throw "Missing required files: $($missing -join ', ')"
}

$matrixLines = Get-Content $MatrixPath
$moduleSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

foreach ($line in $matrixLines) {
    if ($line -notmatch '^\|\s*F\d+\s*\|') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 5) {
        continue
    }

    [void]$moduleSet.Add($parts[4].Trim())
}

$modules = @($moduleSet) | Sort-Object
if ($modules.Count -eq 0) {
    throw "No modules parsed from matrix rows."
}

$usageText = Get-Content $UsageSamplesPath -Raw
$w3Text = Get-Content $W3ReportPath -Raw
$perfText = Get-Content $PerfAuditPath -Raw
$gateCText = Get-Content $GateCReportPath -Raw

$w3HasNoNamespaceGap = $w3Text -match 'Namespace mapping gaps:\s*0'
$gateCPassed = $gateCText -match '(?m)^- Status:\s*PASSED\s*$'

$perfModuleSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($line in (Get-Content $PerfAuditPath)) {
    if ($line -notmatch '^\|\s*F\d+\s*\|') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 5) {
        continue
    }

    $perfNode = $parts[3].Trim()
    if ($perfNode -match '^benches/([^/]+)/') {
        [void]$perfModuleSet.Add($matches[1])
    }
}

$rows = @()
$missingCoverage = @()

foreach ($module in $modules) {
    $w3UsageCovered = $usageText -match [Regex]::Escape($module)
    $w4PerfCovered = $perfModuleSet.Contains($module)

    if (-not $w3UsageCovered -or -not $w4PerfCovered) {
        $missingCoverage += $module
    }

    $rows += [PSCustomObject]@{
        Module = $module
        W3Usage = if ($w3UsageCovered) { 'Yes' } else { 'No' }
        W4PerfNode = if ($w4PerfCovered) { 'Yes' } else { 'No' }
    }
}

$overallPassed = ($missingCoverage.Count -eq 0) -and $w3HasNoNamespaceGap -and $gateCPassed
$overallStatusText = if ($overallPassed) { 'PASSED' } else { 'FAILED' }
$w3StatusText = if ($w3HasNoNamespaceGap) { 'Yes' } else { 'No' }
$gateStatusText = if ($gateCPassed) { 'Yes' } else { 'No' }

$outputDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

$lines = @(
    '# W3/W4 Full Module Coverage Rerun',
    '',
    '- Date: 2026-04-10',
    "- Matrix: $MatrixPath",
    "- W3 report: $W3ReportPath",
    "- W4 perf audit: $PerfAuditPath",
    "- W4 Gate C report: $GateCReportPath",
    "- Overall status: $overallStatusText",
    '',
    '## Global Checks',
    '',
    "- W3 namespace mapping gaps == 0: $w3StatusText",
    "- Gate C status == PASSED: $gateStatusText",
    '',
    '## Module Coverage (from Traceability Matrix)',
    '',
    '| Module | W3 UsageSamples coverage | W4 performance-node coverage |',
    '|---|---|---|'
)

foreach ($row in $rows) {
    $lines += "| $($row.Module) | $($row.W3Usage) | $($row.W4PerfNode) |"
}

if ($missingCoverage.Count -gt 0) {
    $lines += ''
    $lines += '## Missing Coverage'
    $lines += ''
    foreach ($m in $missingCoverage) {
        $lines += "- $m"
    }
}

Set-Content -Path $OutputPath -Value $lines -Encoding UTF8
Write-Output "Module coverage rerun report generated: $OutputPath"

if (-not $overallPassed) {
    throw "W3/W4 full module coverage rerun failed. See $OutputPath"
}

Write-Output "W3/W4 full module coverage rerun passed."
