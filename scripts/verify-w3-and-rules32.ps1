param(
    [string]$RuleChecklistPath = "RuleComplianceChecklist.md",
    [string]$W3ReportPath = "artifacts/w3/2026-04-10-w3-validation.md",
    [string]$OutputPath = "artifacts/compliance/2026-04-10-w3-rules32-review.md"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $RuleChecklistPath)) {
    throw "Rule checklist file not found: $RuleChecklistPath"
}

if (-not (Test-Path $W3ReportPath)) {
    throw "W3 report not found: $W3ReportPath"
}

$ruleLines = Get-Content $RuleChecklistPath | Where-Object { $_ -match '^\|\s*\d+\s*\|' }
if ($ruleLines.Count -eq 0) {
    throw "No rule rows found in checklist: $RuleChecklistPath"
}

$rows = @()
foreach ($line in $ruleLines) {
    $parts = $line.Split('|')
    if ($parts.Count -lt 13) {
        continue
    }

    $ruleNumber = 0
    if (-not [int]::TryParse($parts[1].Trim(), [ref]$ruleNumber)) {
        continue
    }

    $rows += [PSCustomObject]@{
        Rule = $ruleNumber
        Summary = $parts[2].Trim()
        EvidencePath = $parts[6].Trim()
        Status = $parts[8].Trim()
        ArchSignoff = $parts[10].Trim()
        QaSignoff = $parts[11].Trim()
        SignoffDate = $parts[12].Trim()
    }
}

$passed = @($rows | Where-Object { $_.Status -eq 'Passed' })
$inReview = @($rows | Where-Object { $_.Status -eq 'In Review' })
$pending = @($rows | Where-Object { $_.Status -eq 'Pending' })
$blocked = @($rows | Where-Object { $_.Status -eq 'Blocked' })

$missingEvidence = @()
foreach ($row in $rows) {
    if ([string]::IsNullOrWhiteSpace($row.EvidencePath) -or $row.EvidencePath -eq '-') {
        $missingEvidence += $row
        continue
    }

    if (-not (Test-Path $row.EvidencePath)) {
        $missingEvidence += $row
    }
}

$w3Text = Get-Content $W3ReportPath -Raw
$w3Passed = ($w3Text -match 'Namespace mapping gaps:\s*0') -and
            ($w3Text -match 'Contains Tensor sample:\s*Yes') -and
            ($w3Text -match 'Contains Linalg sample:\s*Yes') -and
            ($w3Text -match 'Contains ODE sample:\s*Yes') -and
            ($w3Text -match 'Contains Numerical sample:\s*Yes')

$rule32Passed = ($rows.Count -eq 32) -and ($pending.Count -eq 0) -and ($inReview.Count -eq 0) -and ($blocked.Count -eq 0) -and ($missingEvidence.Count -eq 0)

$overall = if ($w3Passed -and $rule32Passed) { 'PASSED' } else { 'PARTIAL' }
$w3StatusText = if ($w3Passed) { 'PASSED' } else { 'FAILED' }

$outDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outDir)) {
    New-Item -Path $outDir -ItemType Directory -Force | Out-Null
}

$lines = @(
    '# W3 and Rules-32 Compliance Review',
    '',
    '- Date: 2026-04-10',
    "- Rule checklist: $RuleChecklistPath",
    "- W3 report: $W3ReportPath",
    "- Overall status: $overall",
    '',
    '## W3 Result',
    '',
    "- W3 compliance: $w3StatusText",
    '',
    '## Rules-32 Result',
    '',
    "- Parsed rule rows: $($rows.Count)",
    "- Passed: $($passed.Count)",
    "- In Review: $($inReview.Count)",
    "- Pending: $($pending.Count)",
    "- Blocked: $($blocked.Count)",
    "- Missing evidence paths: $($missingEvidence.Count)",
    ''
)

if ($inReview.Count -gt 0) {
    $lines += '### In Review Rules'
    foreach ($r in ($inReview | Sort-Object Rule)) {
        $lines += "- Rule $($r.Rule): $($r.Summary)"
    }
    $lines += ''
}

if ($pending.Count -gt 0) {
    $lines += '### Pending Rules'
    foreach ($r in ($pending | Sort-Object Rule)) {
        $lines += "- Rule $($r.Rule): $($r.Summary)"
    }
    $lines += ''
}

if ($blocked.Count -gt 0) {
    $lines += '### Blocked Rules'
    foreach ($r in ($blocked | Sort-Object Rule)) {
        $lines += "- Rule $($r.Rule): $($r.Summary)"
    }
    $lines += ''
}

if ($missingEvidence.Count -gt 0) {
    $lines += '### Missing Evidence Paths'
    foreach ($r in ($missingEvidence | Sort-Object Rule)) {
        $lines += "- Rule $($r.Rule): $($r.EvidencePath)"
    }
    $lines += ''
}

Set-Content -Path $OutputPath -Encoding UTF8 -Value $lines
Write-Output "Compliance review report generated: $OutputPath"

if (-not $w3Passed) {
    throw "W3 compliance check failed. See report: $OutputPath"
}

if ($blocked.Count -gt 0) {
    throw "Rules-32 compliance contains blocked rules. See report: $OutputPath"
}
