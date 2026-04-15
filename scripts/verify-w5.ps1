param(
    [string]$TraceabilityPath = "TraceabilityMatrix.md",
    [string]$ReleasePlanPath = "ReleasePlan.md",
    [string]$W3RulesReportPath = "artifacts/compliance/2026-04-10-w3-rules32-review.md",
    [string]$TraceabilityReportPath = "artifacts/traceability/2026-04-10-dashboard-validation.md",
    [string]$GateCReportPath = "artifacts/gate-c/2026-04-10-gate-c-validation.md",
    [string]$OutputPath = "artifacts/w5/2026-04-10-w5-validation.md",
    [string]$ValidationReportPath = "ValidationReport.md",
    [switch]$SkipDotnetTest
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot/.."
Set-Location $repoRoot

foreach ($requiredPath in @($TraceabilityPath, $ReleasePlanPath)) {
    if (-not (Test-Path $requiredPath)) {
        throw "Missing required file: $requiredPath"
    }
}

$lines = Get-Content $TraceabilityPath
$rows = @()
foreach ($line in $lines) {
    if ($line -notmatch '^\|\s*F\d+\s*\|') {
        continue
    }

    $parts = $line.Split('|')
    if ($parts.Count -lt 13) {
        continue
    }

    $rows += [PSCustomObject]@{
        Id = $parts[1].Trim()
        FunctionName = $parts[2].Trim()
        ImplementationPath = $parts[5].Trim()
        TestPath = $parts[6].Trim()
        PerformancePath = $parts[7].Trim()
        Status = $parts[8].Trim()
    }
}

if ($rows.Count -eq 0) {
    throw "No function rows found in traceability matrix: $TraceabilityPath"
}

function Split-ReferencePaths {
    param([string]$RawValue)

    if ([string]::IsNullOrWhiteSpace($RawValue) -or $RawValue -eq '-') {
        return @()
    }

    return @($RawValue.Split(';') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
}

$missingImplementationRefs = @()
$missingTestRefs = @()
$missingPerformanceRefs = @()
$statusNotPassed = @()

foreach ($row in $rows) {
    if ($row.Status -ne 'Passed') {
        $statusNotPassed += $row
    }

    $implementationRefs = Split-ReferencePaths -RawValue $row.ImplementationPath
    if ($implementationRefs.Count -eq 0) {
        $missingImplementationRefs += [PSCustomObject]@{ Id = $row.Id; Path = $row.ImplementationPath }
    }
    else {
        $missingImplRefs = @()
        foreach ($ref in $implementationRefs) {
            $implPath = Join-Path $repoRoot ($ref -replace '/', [IO.Path]::DirectorySeparatorChar)
            if (-not (Test-Path $implPath)) {
                $missingImplRefs += $ref
            }
        }

        if ($missingImplRefs.Count -gt 0) {
            $missingImplementationRefs += [PSCustomObject]@{ Id = $row.Id; Path = ($missingImplRefs -join '; ') }
        }
    }

    $testRefs = Split-ReferencePaths -RawValue $row.TestPath
    if ($testRefs.Count -eq 0) {
        $missingTestRefs += [PSCustomObject]@{ Id = $row.Id; Path = $row.TestPath }
    }
    else {
        $missingRowTestRefs = @()
        foreach ($ref in $testRefs) {
            $testPath = Join-Path $repoRoot ($ref -replace '/', [IO.Path]::DirectorySeparatorChar)
            if (-not (Test-Path $testPath)) {
                $missingRowTestRefs += $ref
            }
        }

        if ($missingRowTestRefs.Count -gt 0) {
            $missingTestRefs += [PSCustomObject]@{ Id = $row.Id; Path = ($missingRowTestRefs -join '; ') }
        }
    }

    $performanceRefs = Split-ReferencePaths -RawValue $row.PerformancePath
    if ($performanceRefs.Count -gt 0) {
        $missingRowPerfRefs = @()
        foreach ($ref in $performanceRefs) {
            $perfPath = Join-Path $repoRoot ($ref -replace '/', [IO.Path]::DirectorySeparatorChar)
            if (-not (Test-Path $perfPath)) {
                $missingRowPerfRefs += $ref
            }
        }

        if ($missingRowPerfRefs.Count -gt 0) {
            $missingPerformanceRefs += [PSCustomObject]@{ Id = $row.Id; Path = ($missingRowPerfRefs -join '; ') }
        }
    }
}

$testFiles = @(Get-ChildItem -Path (Join-Path $repoRoot 'tests') -Recurse -Filter *.cs)
if ($testFiles.Count -eq 0) {
    throw "No test files found under tests/."
}

$moduleNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($file in $testFiles) {
    $relative = $file.FullName.Substring((Join-Path $repoRoot 'tests').Length + 1)
    $topFolder = $relative.Split([IO.Path]::DirectorySeparatorChar)[0]
    [void]$moduleNames.Add($topFolder)
}

$requiredModules = @('TensorCore', 'LinalgCore', 'OdeCore', 'NumericalCore')
$missingModules = @()
foreach ($m in $requiredModules) {
    if (-not $moduleNames.Contains($m)) {
        $missingModules += $m
    }
}

$propertyStyleFiles = @($testFiles | Where-Object { $_.Name -match 'Property|Invariant' })
$complexEvidenceCount = @($testFiles | Select-String -Pattern 'Complex' -SimpleMatch).Count
$edgeEvidenceCount = @($testFiles | Select-String -Pattern 'Empty|Singular|Stiff|NaN|Infinity|Bounded|Tolerance').Count

$knownRegressionPaths = @(
    'tests/NumericalCore/RootFinding/BrentTests.cs',
    'tests/OdeCore/RadauTests.cs',
    'tests/LinalgCore/LuTests.cs'
)

$missingKnownRegression = @()
foreach ($path in $knownRegressionPaths) {
    $full = Join-Path $repoRoot ($path -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path $full)) {
        $missingKnownRegression += $path
    }
}

$testStatus = 'Skipped'
$testPassedCount = 'N/A'
if (-not $SkipDotnetTest) {
    $resultsDir = Join-Path $repoRoot 'artifacts/w5/test-results'
    New-Item -Path $resultsDir -ItemType Directory -Force | Out-Null

    $testOutput = (& dotnet test LAL.sln --configuration Release --results-directory $resultsDir --logger 'trx;LogFileName=w5-tests.trx' 2>&1)
    $testOutputText = $testOutput -join "`n"

    if ($LASTEXITCODE -ne 0) {
        $testStatus = 'Failed'
    }
    else {
        $testStatus = 'Passed'
        $passedMatches = [regex]::Matches($testOutputText, '(?i)Passed\s*[:=]\s*(\d+)')
        if ($passedMatches.Count -gt 0) {
            $maxPassed = ($passedMatches | ForEach-Object { [int]$_.Groups[1].Value } | Measure-Object -Maximum).Maximum
            $testPassedCount = $maxPassed
        }
        else {
            $trxFile = Join-Path $resultsDir 'w5-tests.trx'
            if (-not (Test-Path $trxFile)) {
                $trxFile = (Get-ChildItem -Path $resultsDir -Recurse -Filter *.trx | Select-Object -First 1).FullName
            }

            if (-not [string]::IsNullOrWhiteSpace($trxFile) -and (Test-Path $trxFile)) {
                [xml]$trxXml = Get-Content $trxFile
                $passedCounter = $trxXml.TestRun.ResultSummary.Counters.passed
                if (-not [string]::IsNullOrWhiteSpace($passedCounter)) {
                    $testPassedCount = [int]$passedCounter
                }
            }
        }
    }
}

$reportChecks = @(
    [PSCustomObject]@{ Name = 'W3+Rules32 compliance report'; Path = $W3RulesReportPath; Pattern = 'Overall status:\s*PASSED' },
    [PSCustomObject]@{ Name = 'Traceability dashboard report'; Path = $TraceabilityReportPath; Pattern = 'Validation status:\s*PASSED' },
    [PSCustomObject]@{ Name = 'Gate C report'; Path = $GateCReportPath; Pattern = 'Status:\s*PASSED' }
)

$reportFailures = @()
foreach ($check in $reportChecks) {
    if (-not (Test-Path $check.Path)) {
        $reportFailures += "$($check.Name): missing file ($($check.Path))"
        continue
    }

    $text = Get-Content $check.Path -Raw
    if ($text -notmatch $check.Pattern) {
        $reportFailures += "$($check.Name): expected pattern not found ($($check.Pattern))"
    }
}

$triLinkOk = ($missingImplementationRefs.Count -eq 0) -and ($missingTestRefs.Count -eq 0) -and ($missingPerformanceRefs.Count -eq 0) -and ($statusNotPassed.Count -eq 0)
$layeredTestsOk = ($missingModules.Count -eq 0) -and ($propertyStyleFiles.Count -gt 0) -and ($complexEvidenceCount -gt 0) -and ($edgeEvidenceCount -gt 0) -and ($missingKnownRegression.Count -eq 0)
$reportsOk = ($reportFailures.Count -eq 0)
$testRunOk = (($SkipDotnetTest) -or ($testStatus -eq 'Passed'))
$releaseDocsOk = (Test-Path $ReleasePlanPath)

$overall = if ($triLinkOk -and $layeredTestsOk -and $reportsOk -and $testRunOk) { 'PASSED' } else { 'FAILED' }

$outDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outDir)) {
    New-Item -Path $outDir -ItemType Directory -Force | Out-Null
}

$validationLines = @(
    '# W5 Validation Report',
    '',
    '- Date: 2026-04-10',
    "- Overall status: $overall",
    "- Traceability rows parsed: $($rows.Count)",
    "- Dotnet test status: $testStatus",
    "- Dotnet test passed count: $testPassedCount",
    '',
    '## Test Layering',
    '',
    "- Test files found: $($testFiles.Count)",
    "- Property-style test files: $($propertyStyleFiles.Count)",
    "- Complex-path evidence hits: $complexEvidenceCount",
    "- Edge-condition evidence hits: $edgeEvidenceCount",
    "- Missing required test modules: $($missingModules.Count)",
    "- Missing known-regression files: $($missingKnownRegression.Count)",
    '',
    '## Numeric and Gate Evidence',
    '',
    "- Report validation failures: $($reportFailures.Count)",
    '',
    '## Traceability -> Test -> Perf Tri-link',
    '',
    "- Missing implementation refs: $($missingImplementationRefs.Count)",
    "- Missing test refs: $($missingTestRefs.Count)",
    "- Missing performance refs: $($missingPerformanceRefs.Count)",
    "- Rows not Passed: $($statusNotPassed.Count)",
    ''
)

if ($missingModules.Count -gt 0) {
    $validationLines += '### Missing Test Modules'
    foreach ($m in $missingModules) {
        $validationLines += "- $m"
    }
    $validationLines += ''
}

if ($missingKnownRegression.Count -gt 0) {
    $validationLines += '### Missing Known Regression Files'
    foreach ($p in $missingKnownRegression) {
        $validationLines += "- $p"
    }
    $validationLines += ''
}

if ($reportFailures.Count -gt 0) {
    $validationLines += '### Report Validation Failures'
    foreach ($f in $reportFailures) {
        $validationLines += "- $f"
    }
    $validationLines += ''
}

if ($missingImplementationRefs.Count -gt 0) {
    $validationLines += '### Missing Implementation References'
    foreach ($item in $missingImplementationRefs) {
        $validationLines += "- $($item.Id): $($item.Path)"
    }
    $validationLines += ''
}

if ($missingTestRefs.Count -gt 0) {
    $validationLines += '### Missing Test References'
    foreach ($item in $missingTestRefs) {
        $validationLines += "- $($item.Id): $($item.Path)"
    }
    $validationLines += ''
}

if ($missingPerformanceRefs.Count -gt 0) {
    $validationLines += '### Missing Performance References'
    foreach ($item in $missingPerformanceRefs) {
        $validationLines += "- $($item.Id): $($item.Path)"
    }
    $validationLines += ''
}

if ($statusNotPassed.Count -gt 0) {
    $validationLines += '### Rows Not Passed'
    foreach ($item in $statusNotPassed) {
        $validationLines += "- $($item.Id): $($item.FunctionName) ($($item.Status))"
    }
    $validationLines += ''
}

Set-Content -Path $OutputPath -Encoding UTF8 -Value $validationLines
Write-Output "W5 validation report generated: $OutputPath"

$validationSummary = @(
    '# Validation Report',
    '',
    '- Date: 2026-04-10',
    "- Overall W5 status: $overall",
    '',
    '## W5 Scope Completion',
    '',
    "- 6.1 Test layering: $(if ($layeredTestsOk) { 'Passed' } else { 'Failed' })",
    "- 6.2 Numeric validation evidence: $(if ($reportsOk) { 'Passed' } else { 'Failed' })",
    "- 6.3 Documentation and release assets: $(if ($releaseDocsOk) { 'Passed' } else { 'Failed' })",
    "- 6.5 Traceability -> test -> benchmark tri-link: $(if ($triLinkOk) { 'Passed' } else { 'Failed' })",
    '',
    '## Key Metrics',
    '',
    "- Traceability rows: $($rows.Count)",
    "- Test files: $($testFiles.Count)",
    "- Property-style files: $($propertyStyleFiles.Count)",
    "- Dotnet test status: $testStatus",
    "- Dotnet test passed count: $testPassedCount",
    '',
    '## Evidence',
    '',
    "- W5 detail report: $OutputPath",
    "- W3+Rules32 report: $W3RulesReportPath",
    "- Traceability dashboard report: $TraceabilityReportPath",
    "- Gate C report: $GateCReportPath"
)

Set-Content -Path $ValidationReportPath -Encoding UTF8 -Value $validationSummary
Write-Output "Validation summary generated: $ValidationReportPath"

if ($overall -ne 'PASSED') {
    throw "W5 validation failed. See report: $OutputPath"
}

Write-Output 'W5 validation passed.'