$ErrorActionPreference = 'Stop'

$prBody = $env:PR_BODY
if ([string]::IsNullOrWhiteSpace($prBody)) {
    throw "PR body is empty. Fill the PR template and check required evidence boxes."
}

$requiredChecks = @(
    @{ Name = 'Gate A evidence updated'; Pattern = '(?im)^\s*-\s*\[[xX]\]\s*Gate A evidence updated' },
    @{ Name = 'Performance evidence updated or linked'; Pattern = '(?im)^\s*-\s*\[[xX]\]\s*Performance evidence updated or linked' }
)

$missingChecks = @()
foreach ($check in $requiredChecks) {
    if ($prBody -notmatch $check.Pattern) {
        $missingChecks += $check.Name
    }
}

if ($missingChecks.Count -gt 0) {
    throw ("Missing required PR checklist checks: " + ($missingChecks -join ', '))
}

$changedFiles = @()
$changedFilesInput = $env:CHANGED_FILES

if (-not [string]::IsNullOrWhiteSpace($changedFilesInput)) {
    $changedFiles = @(
        $changedFilesInput -split "(`r`n|`n|;|,)" |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}
else {
    $baseRef = $env:GITHUB_BASE_REF
    if ([string]::IsNullOrWhiteSpace($baseRef)) {
        Write-Output 'GITHUB_BASE_REF is not set; checklist check passed, changed-file enforcement skipped.'
        exit 0
    }

    $baseTarget = "origin/$baseRef"
    $mergeBase = (git merge-base $baseTarget HEAD).Trim()
    if ([string]::IsNullOrWhiteSpace($mergeBase)) {
        throw "Unable to determine merge base against $baseTarget."
    }

    $changedFiles = @(git diff --name-only "$mergeBase...HEAD")
}

if ($changedFiles.Count -eq 0) {
    Write-Output 'No changed files detected; PR evidence check passed.'
    exit 0
}

$touchesSrc = $false
$touchesGateA = $false
$touchesPerf = $false

foreach ($file in $changedFiles) {
    if ($file -match '^src/') {
        $touchesSrc = $true
    }

    if ($file -match '^artifacts/gate-a/r\d{2}\.md$') {
        $touchesGateA = $true
    }

    if ($file -match '^artifacts/perf/.+\.md$') {
        $touchesPerf = $true
    }
}

if ($touchesSrc -and -not $touchesGateA) {
    throw "Source files changed but no Gate A evidence file was updated under artifacts/gate-a/rXX.md."
}

if ($touchesSrc -and -not $touchesPerf) {
    throw "Source files changed but no performance evidence file was updated under artifacts/perf/*.md."
}

Write-Output 'PR evidence check passed.'
