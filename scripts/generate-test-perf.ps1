param(
    [string]$Root = "."
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path $Root
$profilePath = Join-Path $repoRoot "src/Core/DataStructureCompatibility.Performance.cs"
$testsProject = Join-Path $repoRoot "LAL.Tests/LAL.Tests.csproj"
$reportPath = Join-Path $repoRoot "TestPerf.md"
$resultsDir = Join-Path $repoRoot "artifacts/testperf-trx"

if (!(Test-Path $profilePath))
{
    throw "Profile source not found: $profilePath"
}

if (!(Test-Path $testsProject))
{
    throw "Tests project not found: $testsProject"
}

New-Item -Path $resultsDir -ItemType Directory -Force | Out-Null

$profileText = Get-Content -Path $profilePath -Raw
$matches = [regex]::Matches(
    $profileText,
    'new\("(?<core>LinalgCore|NumericalCore|OdeCore|TensorCore)",\s*"(?<module>[^"]+)"',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ($matches.Count -eq 0)
{
    throw "No module definitions found in $profilePath"
}

$modules = @()
foreach ($m in $matches)
{
    $core = $m.Groups["core"].Value
    $module = $m.Groups["module"].Value
    $token = ($module -split '\.')[(-1)]

    $modules += [pscustomobject]@{
        Core = $core
        Module = $module
        Token = $token
    }
}

$rows = @()
$moduleIndex = 0

foreach ($entry in $modules)
{
    $moduleIndex++
    $filter = "FullyQualifiedName~LAL.Tests.$($entry.Core)&FullyQualifiedName~$($entry.Token)"
    $safeModuleName = ($entry.Module -replace '[^A-Za-z0-9._-]', '_')
    $trxFileName = "module-$moduleIndex-$($entry.Core)-$safeModuleName.trx"
    $trxPath = Join-Path $resultsDir $trxFileName

    if (Test-Path $trxPath)
    {
        Remove-Item -Path $trxPath -Force
    }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $null = & dotnet test $testsProject -c Release --no-build --nologo --verbosity quiet --results-directory $resultsDir --logger "trx;LogFileName=$trxFileName" --filter $filter 2>&1 | Out-String
    $exitCode = $LASTEXITCODE
    $sw.Stop()

    $total = 0
    $passed = 0
    $failed = 0
    $skipped = 0
    $result = if ($exitCode -eq 0) { "pass" } else { "fail" }
    $notes = ""

    if (Test-Path $trxPath)
    {
        [xml]$trx = Get-Content -Path $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters

        if ($null -ne $counters)
        {
            if ($null -ne $counters.total) { $total = [int]$counters.total }
            if ($null -ne $counters.passed) { $passed = [int]$counters.passed }
            if ($null -ne $counters.failed) { $failed = [int]$counters.failed }

            if ($null -ne $counters.notExecuted)
            {
                $skipped = [int]$counters.notExecuted
            }
            elseif ($total -ge ($passed + $failed))
            {
                $skipped = $total - ($passed + $failed)
            }
        }
    }
    else
    {
        $notes = "missing-trx"
    }

    if ($total -eq 0 -and [string]::IsNullOrWhiteSpace($notes))
    {
        $notes = "no-matching-tests"
    }

    if ($exitCode -ne 0)
    {
        $notes = if ([string]::IsNullOrWhiteSpace($notes)) { "dotnet-test-failed" } else { "$notes; dotnet-test-failed" }
    }

    $rows += [pscustomobject]@{
        Core = $entry.Core
        Module = $entry.Module
        Token = $entry.Token
        Tests = $total
        Passed = $passed
        Failed = $failed
        Skipped = $skipped
        ElapsedMs = [math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        Result = $result
        Notes = $notes
    }
}

$generated = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Module Test Performance Report")
$lines.Add("")
$lines.Add("- Generated At: $generated")
$lines.Add("- Source Entry: src/Core/DataStructureCompatibility.Performance.cs -> CreateModuleProfiles()")
$lines.Add("- Scope: LinalgCore, NumericalCore, OdeCore, TensorCore")
$lines.Add("- Command: dotnet test LAL.Tests/LAL.Tests.csproj -c Release --no-build --filter <core+module-token>")
$lines.Add("")
$lines.Add("| Core | Module | Token | Tests | Passed | Failed | Skipped | Elapsed (ms) | Result | Notes |")
$lines.Add("|---|---|---|---:|---:|---:|---:|---:|---|---|")

foreach ($r in $rows)
{
    $lines.Add("| $($r.Core) | $($r.Module) | $($r.Token) | $($r.Tests) | $($r.Passed) | $($r.Failed) | $($r.Skipped) | $($r.ElapsedMs) | $($r.Result) | $($r.Notes) |")
}

$lines.Add("")
$lines.Add("## Core Summary")
$lines.Add("")
$lines.Add("| Core | Modules | Total Tests | Passed | Failed | Skipped | Total Elapsed (ms) |")
$lines.Add("|---|---:|---:|---:|---:|---:|---:|")

$coreSummary = $rows | Group-Object Core | Sort-Object Name
foreach ($g in $coreSummary)
{
    $moduleCount = $g.Count
    $totalTests = ($g.Group | Measure-Object -Property Tests -Sum).Sum
    $passed = ($g.Group | Measure-Object -Property Passed -Sum).Sum
    $failed = ($g.Group | Measure-Object -Property Failed -Sum).Sum
    $skipped = ($g.Group | Measure-Object -Property Skipped -Sum).Sum
    $elapsed = [math]::Round((($g.Group | Measure-Object -Property ElapsedMs -Sum).Sum), 2)
    $lines.Add("| $($g.Name) | $moduleCount | $totalTests | $passed | $failed | $skipped | $elapsed |")
}

$lines.Add("")
$lines.Add("## Notes")
$lines.Add("")
$lines.Add("- Timings include test host startup overhead for each module-level run.")
$lines.Add("- Module matching token uses the final segment of module name (for example Sparse.Spmv -> Spmv).")
$lines.Add("- If Notes contains no-matching-tests, no test was discovered for the selected core+token filter.")

Set-Content -Path $reportPath -Value $lines -Encoding UTF8
Write-Host "Generated: $reportPath"