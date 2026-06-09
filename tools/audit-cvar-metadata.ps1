param(
    [string]$RepoRoot = "",
    [string]$ReportPath = "",
    [switch]$FailOnMissingDescriptions
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "cvar-metadata.ps1")

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).ProviderPath
}
if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $RepoRoot "tmp_cvar_metadata_audit.json"
}

$categoryConfig = Initialize-CvarCategoryConfig -Config (Import-CvarCategoriesConfig)
$descriptionContext = New-CvarDescriptionResolver -RepoRoot $RepoRoot
$sourceByName = @{}
foreach ($definition in $descriptionContext.SourceDefinitions) {
    if (-not $sourceByName.ContainsKey($definition.Name)) {
        $sourceByName[$definition.Name] = $definition
    }
}

$categoryCounts = @{}
foreach ($category in @($categoryConfig.Categories)) {
    $categoryCounts[[string]$category.id] = 0
}

$entries = @()
$missingDescriptions = New-Object System.Collections.Generic.List[string]
$heuristicDescriptions = New-Object System.Collections.Generic.List[string]

foreach ($definition in ($sourceByName.Values | Sort-Object Name)) {
    $name = [string]$definition.Name
    $categoryId = Get-CvarCategoryId -Name $name -CategoryConfig $categoryConfig
    $categoryCounts[$categoryId] = [int]$categoryCounts[$categoryId] + 1
    $description = & $descriptionContext.Resolve $name
    $hasInlineDescription = -not [string]::IsNullOrWhiteSpace($definition.Description)
    $requiresDescription = Test-CvarRequiresDescription -Name $name -CategoryConfig $categoryConfig
    $isHeuristic = Test-CvarDescriptionIsHeuristic -Description $description

    if ($requiresDescription -and ($isHeuristic -and -not $hasInlineDescription)) {
        [void]$missingDescriptions.Add($name)
    }
    if ($isHeuristic) {
        [void]$heuristicDescriptions.Add($name)
    }

    $entries += [pscustomobject]@{
        Name                 = $name
        CategoryId           = $categoryId
        CategoryTitle        = [string](Get-CvarCategoryForName -Name $name -CategoryConfig $categoryConfig).title
        Description          = $description
        HasInlineDescription = $hasInlineDescription
        RequiresDescription  = $requiresDescription
        Source               = ("{0}:{1}" -f $definition.Source, $definition.Line)
    }
}

$report = [ordered]@{
    generatedUtc          = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
    categoryConfig        = (Get-CvarCategoriesConfigPath)
    totalSourceCvars      = $sourceByName.Count
    categoryCounts        = $categoryCounts
    missingDescriptions   = @($missingDescriptions | Sort-Object)
    heuristicDescriptions = @($heuristicDescriptions | Sort-Object)
    entries               = $entries
}

$reportDir = Split-Path -Parent $ReportPath
if (-not (Test-Path -LiteralPath $reportDir)) {
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $ReportPath -Encoding UTF8

Write-Output ("Audited {0} source CVARs." -f $sourceByName.Count)
Write-Output ("Category config: {0}" -f (Get-CvarCategoriesConfigPath))
Write-Output ("Report: {0}" -f $ReportPath)
Write-Output ("Missing required descriptions: {0}" -f $missingDescriptions.Count)
Write-Output ("Heuristic-only descriptions: {0}" -f $heuristicDescriptions.Count)

Write-Output ""
Write-Output "Category counts:"
foreach ($category in @($categoryConfig.Categories)) {
    $categoryId = [string]$category.id
    $count = [int]$categoryCounts[$categoryId]
    if ($count -eq 0) {
        continue
    }
    Write-Output ("  {0,-24} {1,4}" -f $category.title, $count)
}

if ($missingDescriptions.Count -gt 0) {
    Write-Output ""
    Write-Output "CVARs missing required descriptions:"
    foreach ($name in ($missingDescriptions | Sort-Object)) {
        Write-Output ("  - {0}" -f $name)
    }
}

if ($FailOnMissingDescriptions -and $missingDescriptions.Count -gt 0) {
    exit 1
}
