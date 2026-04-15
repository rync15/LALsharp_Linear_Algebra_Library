param(
    [string]$W3ReportPath = "artifacts/w3/2026-04-10-w3-validation.md",
    [string]$RulesReportPath = "artifacts/compliance/2026-04-10-w3-rules32-review.md",
    [string]$OutputPath = "artifacts/gate-b/2026-04-10-gate-b-validation.md"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot/.."
Set-Location $repoRoot

$reportDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($reportDir)) {
    New-Item -Path $reportDir -ItemType Directory -Force | Out-Null
}

$testResultsDir = Join-Path $repoRoot "artifacts/gate-b/test-results"
New-Item -Path $testResultsDir -ItemType Directory -Force | Out-Null

$testOutput = (& dotnet test LAL.sln --configuration Release --results-directory $testResultsDir --logger "trx;LogFileName=gate-b-tests.trx" 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed during Gate B verification."
}

$testOutputText = $testOutput -join "`n"
$passedCount = 'N/A'
$passedMatches = [regex]::Matches($testOutputText, '(?i)Passed\s*[:=]\s*(\d+)')
if ($passedMatches.Count -gt 0) {
    $passedCount = ($passedMatches | ForEach-Object { [int]$_.Groups[1].Value } | Measure-Object -Maximum).Maximum
}
else {
    $trxFile = Join-Path $testResultsDir "gate-b-tests.trx"
    if (-not (Test-Path $trxFile)) {
        $trxFile = (Get-ChildItem -Path $testResultsDir -Recurse -Filter *.trx | Select-Object -First 1).FullName
    }

    if (-not [string]::IsNullOrWhiteSpace($trxFile) -and (Test-Path $trxFile)) {
        [xml]$trxXml = Get-Content $trxFile
        $trxPassed = $trxXml.TestRun.ResultSummary.Counters.passed
        if (-not [string]::IsNullOrWhiteSpace($trxPassed)) {
            $passedCount = [int]$trxPassed
        }
    }
}

./scripts/verify-w3.ps1
./scripts/verify-w3-and-rules32.ps1

if (-not (Test-Path $W3ReportPath)) {
    throw "Missing W3 report: $W3ReportPath"
}

if (-not (Test-Path $RulesReportPath)) {
    throw "Missing rules report: $RulesReportPath"
}

$w3Text = Get-Content $W3ReportPath -Raw
$rulesText = Get-Content $RulesReportPath -Raw

$w3Passed = ($w3Text -match 'Namespace mapping gaps:\s*0') -and
            ($w3Text -match 'Contains Tensor sample:\s*Yes') -and
            ($w3Text -match 'Contains Linalg sample:\s*Yes') -and
            ($w3Text -match 'Contains ODE sample:\s*Yes') -and
            ($w3Text -match 'Contains Numerical sample:\s*Yes')

$rulesPassed = ($rulesText -match 'Overall status:\s*PASSED') -and
               ($rulesText -match 'Passed:\s*32') -and
               ($rulesText -match 'In Review:\s*0') -and
               ($rulesText -match 'Pending:\s*0') -and
               ($rulesText -match 'Blocked:\s*0')

$overall = if ($w3Passed -and $rulesPassed) { 'PASSED' } else { 'FAILED' }

$lines = @(
    '# Gate B Validation Report',
    '',
    '- Date: 2026-04-10',
    "- Overall status: $overall",
    "- Test passed count: $passedCount",
    "- W3 report: $W3ReportPath",
    "- Rules report: $RulesReportPath",
    '',
    '## Checks',
    '',
    "- W2 runtime and regression tests: Passed ($passedCount)",
    "- W3 API mapping and samples: $(if ($w3Passed) { 'Passed' } else { 'Failed' })",
    "- Rules-32 compliance: $(if ($rulesPassed) { 'Passed' } else { 'Failed' })"
)

Set-Content -Path $OutputPath -Encoding UTF8 -Value $lines
Write-Output "Gate B report generated: $OutputPath"

if ($overall -ne 'PASSED') {
    throw "Gate B validation failed. See report: $OutputPath"
}

Write-Output 'Gate B validation passed.'
