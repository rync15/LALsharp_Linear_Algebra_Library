$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path "$PSScriptRoot/.."
$ruleFile = Join-Path $repoRoot 'RuleComplianceChecklist.md'
$evidenceDir = Join-Path $repoRoot 'artifacts/gate-a'

if (-not (Test-Path $ruleFile)) {
    throw "Missing RuleComplianceChecklist.md"
}

if (-not (Test-Path $evidenceDir)) {
    throw "Missing artifacts/gate-a directory"
}

$content = Get-Content $ruleFile -Raw
if ($content -notmatch 'Arch Sign-off \| QA Sign-off \| Sign-off Date') {
    throw "Rule checklist is missing required sign-off columns"
}

$missing = @()
for ($i = 1; $i -le 32; $i++) {
    $name = ('r{0:d2}.md' -f $i)
    $path = Join-Path $evidenceDir $name
    if (-not (Test-Path $path)) {
        $missing += $name
    }
}

if ($missing.Count -gt 0) {
    throw ("Missing Gate A evidence files: " + ($missing -join ', '))
}

Write-Output 'Gate A verification passed: checklist columns and r01-r32 evidence files exist.'
