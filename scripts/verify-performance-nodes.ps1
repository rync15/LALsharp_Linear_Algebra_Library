param(
    [string]$MatrixPath = "TraceabilityMatrix.md",
    [string]$OutputPath = "artifacts/perf/perf-node-audit.md"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $MatrixPath)) {
    throw "Matrix file not found: $MatrixPath"
}

$matrixLines = Get-Content $MatrixPath
$rows = @()

foreach ($line in $matrixLines) {
    if ($line -notmatch '^\|\s*F\d+\s*\|') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 8) {
        continue
    }

    $functionId = $parts[1].Trim()
    $functionName = $parts[2].Trim()
    $perfNode = $parts[7].Trim()

    if ([string]::IsNullOrWhiteSpace($perfNode) -or $perfNode -eq '-') {
        continue
    }

    $exists = Test-Path $perfNode
    $rows += [PSCustomObject]@{
        FunctionId = $functionId
        FunctionName = $functionName
        PerfNode = $perfNode
        Exists = $exists
    }
}

if ($rows.Count -eq 0) {
    throw "No performance-node rows were parsed from matrix: $MatrixPath"
}

$missing = @($rows | Where-Object { -not $_.Exists })

$outputDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

$generatedOn = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$out = @()
$out += "# Performance Node Audit"
$out += ""
$out += "- Matrix: $MatrixPath"
$out += "- Generated: $generatedOn"
$out += "- Total performance nodes: $($rows.Count)"
$out += "- Missing performance nodes: $($missing.Count)"
$out += ""
$out += "| Function ID | Function Name | Performance Node | Exists |"
$out += "|---|---|---|---|"

foreach ($row in $rows) {
    $status = if ($row.Exists) { "Yes" } else { "No" }
    $out += "| $($row.FunctionId) | $($row.FunctionName) | $($row.PerfNode) | $status |"
}

Set-Content -Path $OutputPath -Value $out -Encoding UTF8
Write-Output "Performance node audit written: $OutputPath"

if ($missing.Count -gt 0) {
    $missingList = ($missing | ForEach-Object { "$($_.FunctionId):$($_.PerfNode)" }) -join ", "
    throw "Missing performance node files detected: $missingList"
}

Write-Output "Performance node audit passed ($($rows.Count) checked)."
