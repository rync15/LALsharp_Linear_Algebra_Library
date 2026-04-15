param(
    [string]$ApiDesignPath = "ApiDesign.md",
    [string]$ApiSurfacePath = "ApiSurface.cs",
    [string]$UsageSamplesPath = "UsageSamples.md",
    [string]$SourceRoot = "src",
    [string]$OutputPath = "artifacts/w3/2026-04-10-w3-validation.md"
)

$ErrorActionPreference = "Stop"

$requiredFiles = @($ApiDesignPath, $ApiSurfacePath, $UsageSamplesPath)
$missingFiles = @()
foreach ($f in $requiredFiles) {
    if (-not (Test-Path $f)) {
        $missingFiles += $f
    }
}

if ($missingFiles.Count -gt 0) {
    throw "Missing W3 deliverables: $($missingFiles -join ', ')"
}

$sourceFiles = Get-ChildItem -Path $SourceRoot -Recurse -Filter *.cs
if (-not $sourceFiles) {
    throw "No source files found under $SourceRoot"
}

$namespaceSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($file in $sourceFiles) {
    foreach ($line in (Get-Content $file.FullName)) {
        if ($line -match '^namespace\s+([^;]+);') {
            [void]$namespaceSet.Add($matches[1].Trim())
            break
        }
    }
}

$allNamespaces = @($namespaceSet) | Sort-Object

$apiDesignText = Get-Content $ApiDesignPath -Raw
$apiSurfaceText = Get-Content $ApiSurfacePath -Raw
$usageText = Get-Content $UsageSamplesPath -Raw

$missingNamespaceMappings = @()
foreach ($ns in $allNamespaces) {
    $inDesign = $apiDesignText -match [Regex]::Escape($ns)
    $inSurface = $apiSurfaceText -match [Regex]::Escape($ns)
    if (-not ($inDesign -or $inSurface)) {
        $missingNamespaceMappings += $ns
    }
}

$outputDir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

$lines = @(
    "# W3 Validation Report",
    "",
    "- Date: 2026-04-10",
    "- Deliverables checked: $($requiredFiles -join ', ')",
    "- Source namespaces found: $($allNamespaces.Count)",
    "- Namespace mapping gaps: $($missingNamespaceMappings.Count)",
    "",
    "## Namespace Coverage",
    "",
    "| Namespace | Mapped |",
    "|---|---|"
)

foreach ($ns in $allNamespaces) {
    $mapped = if ($missingNamespaceMappings -contains $ns) { "No" } else { "Yes" }
    $lines += "| $ns | $mapped |"
}

$lines += ""
$lines += "## Usage Samples Presence"
$lines += ""
$lines += "- Contains Tensor sample: " + ($(if ($usageText -match 'TensorCore') { 'Yes' } else { 'No' }))
$lines += "- Contains Linalg sample: " + ($(if ($usageText -match 'LinalgCore') { 'Yes' } else { 'No' }))
$lines += "- Contains ODE sample: " + ($(if ($usageText -match 'OdeCore') { 'Yes' } else { 'No' }))
$lines += "- Contains Numerical sample: " + ($(if ($usageText -match 'NumericalCore') { 'Yes' } else { 'No' }))

Set-Content -Path $OutputPath -Value $lines -Encoding UTF8
Write-Output "W3 report generated: $OutputPath"

if ($missingNamespaceMappings.Count -gt 0) {
    throw "W3 namespace mapping gaps found: $($missingNamespaceMappings -join ', ')"
}

Write-Output "W3 validation passed."
