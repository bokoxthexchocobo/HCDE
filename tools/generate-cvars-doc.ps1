param(
    [string]$BaselineCsv = "",
    [string]$SetpassReport = "",
    [string]$OutputPath = "",
    [string]$RepoRoot = "",
    [string]$CategoryCatalogPath = "",
    [switch]$SourceOnly
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "cvar-metadata.ps1")

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $PSScriptRoot "..\wiki\CVAR-Reference.md"
}
if ([string]::IsNullOrWhiteSpace($CategoryCatalogPath)) {
    $CategoryCatalogPath = Join-Path $PSScriptRoot "..\wiki\CVAR-Categories.md"
}
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).ProviderPath
}

$categoryConfig = Initialize-CvarCategoryConfig -Config (Import-CvarCategoriesConfig)
$descriptionContext = New-CvarDescriptionResolver -RepoRoot $RepoRoot
$manualDescriptions = $descriptionContext.ManualDescriptions
$sourceDescriptions = $descriptionContext.SourceDescriptions
$sourceDefinitions = $descriptionContext.SourceDefinitions
$invasionRanges = Get-ManualCvarRanges

$sourceByName = @{}
foreach ($definition in $sourceDefinitions) {
    if (-not $sourceByName.ContainsKey($definition.Name)) {
        $sourceByName[$definition.Name] = $definition
    }
}

function Get-DescriptionForCvar {
    param([string]$Name)
    return & $descriptionContext.Resolve $Name
}

$cvars = @()
$report = $null
$runtimeNameSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$runtimeByName = @{}
$latchedSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$missingGetSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$writeProtectedSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

if (-not $SourceOnly) {
    if ([string]::IsNullOrWhiteSpace($BaselineCsv)) {
        $BaselineCsv = Join-Path $RepoRoot "tmp_cvar_baseline.csv"
    }
    if ([string]::IsNullOrWhiteSpace($SetpassReport)) {
        $SetpassReport = Join-Path $RepoRoot "tmp_cvar_setpass_report.json"
    }
    if (-not (Test-Path -LiteralPath $BaselineCsv)) {
        throw "Runtime baseline CSV not found: $BaselineCsv"
    }
    if (-not (Test-Path -LiteralPath $SetpassReport)) {
        throw "Set/get report JSON not found: $SetpassReport"
    }

    $BaselineCsv = (Resolve-Path -LiteralPath $BaselineCsv).ProviderPath
    $SetpassReport = (Resolve-Path -LiteralPath $SetpassReport).ProviderPath
    $cvars = Import-Csv -LiteralPath $BaselineCsv | Sort-Object Name
    $report = Get-Content -LiteralPath $SetpassReport -Raw | ConvertFrom-Json

    foreach ($n in @($report.latchedMessages)) {
        [void]$latchedSet.Add([string]$n)
    }
    foreach ($n in @($report.missingGetResponses)) {
        [void]$missingGetSet.Add([string]$n)
    }
    foreach ($n in @($report.writeProtectedResponses)) {
        [void]$writeProtectedSet.Add([string]$n)
    }
    foreach ($c in $cvars) {
        $runtimeName = [string]$c.Name
        [void]$runtimeNameSet.Add($runtimeName)
        $runtimeByName[$runtimeName] = $c
    }
}

function Decode-Flags {
    param([string]$Flags)

    if ($null -eq $Flags) {
        $Flags = ""
    }

    $chars = $Flags.PadRight(5).ToCharArray()

    $archive = if ($chars[0] -eq 'A') { 'Yes' } else { 'No' }
    $scope = switch ($chars[1]) {
        'U' { 'UserInfo (replicated per user)' }
        'S' { 'ServerInfo (server-advertised)' }
        'C' { 'Auto/Custom (runtime-created)' }
        default { 'Local/General' }
    }
    $mutability = switch ($chars[2]) {
        '-' { 'Write-protected (NOSET)' }
        'L' { 'Latched (applies next game/level)' }
        '*' { 'Unsettable auto cvar' }
        default { 'Writable now' }
    }
    $isMod = if ($chars[3] -eq 'M') { 'Yes' } else { 'No' }
    $isIgnored = if ($chars[4] -eq 'X') { 'Yes' } else { 'No' }

    return [pscustomobject]@{
        Archive = $archive
        Scope = $scope
        Mutability = $mutability
        Mod = $isMod
        Ignored = $isIgnored
    }
}

$invasionDefaultFallback = @{
    "sv_invasioncountdowntime"    = "30"
    "sv_invasionspawntime"        = "8"
    "sv_invasioncleanuptime"      = "4"
    "sv_invasionintermissiontime" = "6"
    "sv_invasionresulttime"       = "8"
    "sv_invasionwaves"            = "8"
    "sv_invasionbasebudget"       = "24"
    "sv_invasionbudgetstep"       = "8"
    "sv_invasionperplayer"        = "6"
    "sv_invasionspawninterval"    = "0.35"
    "sv_invasionspawnburst"       = "3"
    "sv_invasionmaxactive"        = "0"
    "sv_invasionbosswaveevery"    = "5"
    "sv_invasionbossbonus"        = "20"
    "sv_invasionspotusemaptags"   = "0"
    "sv_invasionspotfallback"     = "1"
    "sv_invasionsimlod"           = "1"
    "sv_invasionsimlodfullrange"  = "2048"
    "sv_invasionsimlodreducedrange" = "4096"
    "sv_invasionsimlodreducedinterval" = "5"
    "sv_invasionsimloddormantinterval" = "TICRATE * 3"
    "sv_usemapsettingswavelimit"  = "1"
    "wavelimit"                   = "0"
    "duellimit"                   = "0"
    "sv_corpsequeuesize"          = "64"
    "sv_corpsefilter"             = "1"
}

$stampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
$sourceNameSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($definition in $sourceDefinitions) {
    [void]$sourceNameSet.Add([string]$definition.Name)
}

$sourceOnlyCount = 0
foreach ($definition in $sourceByName.Values) {
    if (-not $runtimeNameSet.Contains([string]$definition.Name)) {
        $sourceOnlyCount++
    }
}

$runtimeOnlyCount = 0
foreach ($c in $cvars) {
    if (-not $sourceNameSet.Contains([string]$c.Name)) {
        $runtimeOnlyCount++
    }
}

$hcdeFocusSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($name in $manualDescriptions.Keys) {
    [void]$hcdeFocusSet.Add([string]$name)
}
foreach ($definition in $sourceDefinitions) {
    $name = [string]$definition.Name
    if ($name.StartsWith("sv_invasion", [System.StringComparison]::OrdinalIgnoreCase) -or
        $name.StartsWith("net_predict_", [System.StringComparison]::OrdinalIgnoreCase)) {
        [void]$hcdeFocusSet.Add($name)
    }
}

$categoryGroups = Group-CvarsByCategory -Definitions $sourceByName.Values -CategoryConfig $categoryConfig

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# HCDE CVAR Reference")
$lines.Add("")
$lines.Add(("Generated: {0} UTC" -f $stampUtc))
$lines.Add("")
if ($SourceOnly) {
    $lines.Add('This reference is generated from source-defined CVAR macros and the category taxonomy in `tools/cvar-categories.json`.')
} else {
    $lines.Add('This reference combines source-defined CVAR inventory with the imported runtime audit snapshot.')
}
$lines.Add("")
$lines.Add("## Coverage")
$lines.Add("")
$lines.Add(('- Source CVAR definitions discovered: **{0}** unique / **{1}** total macro definitions' -f $sourceByName.Count, $sourceDefinitions.Count))
if (-not $SourceOnly) {
    $lines.Add(('- Source-defined CVARs absent from imported runtime snapshot: **{0}**' -f $sourceOnlyCount))
    $lines.Add(('- Runtime-only CVARs from imported snapshot: **{0}**' -f $runtimeOnlyCount))
    $lines.Add(('- Total runtime CVARs in imported snapshot: **{0}**' -f $report.parsedCvars))
    $lines.Add(('- Set/get tested runtime CVARs: **{0}**' -f $report.setTargets))
    $lines.Add(('- Successful get responses: **{0}**' -f $report.getResponses))
    $lines.Add(('- Missing get responses: **{0}**' -f @($report.missingGetResponses).Count))
    $lines.Add(('- Unexpected parser/runtime lines during sweep: **{0}**' -f @($report.unexpectedLines).Count))
    $lines.Add(('- Runtime baseline CSV: `{0}`' -f $BaselineCsv))
    $lines.Add(('- Set/get report: `{0}`' -f $SetpassReport))
    $lines.Add("")
    $lines.Add("> Note: the source catalog is regenerated from the current checkout. The runtime snapshot is imported from the audit files above, so entries marked absent may still be valid source CVARs that were not visible in that older runtime capture.")
}
$lines.Add("")
$lines.Add("## Category Index")
$lines.Add("")
$lines.Add('CVARs are grouped using prefix rules and explicit overrides in [`tools/cvar-categories.json`](../tools/cvar-categories.json).')
$lines.Add("")
$lines.Add("| Category | CVARs | Description |")
$lines.Add("| --- | ---: | --- |")
foreach ($category in @($categoryConfig.Categories)) {
    $categoryId = [string]$category.id
    $count = if ($categoryGroups.ContainsKey($categoryId)) { $categoryGroups[$categoryId].Count } else { 0 }
    if ($count -eq 0 -and $categoryId -eq "misc") {
        continue
    }
    $anchor = $categoryId.ToLowerInvariant()
    $lines.Add((Format-MarkdownTableRow @(
        ("[{0}](#category-{1})" -f [string]$category.title, $anchor),
        [string]$count,
        [string]$category.description
    )))
}
$lines.Add("")
$lines.Add('See also the compact category catalog: [`wiki/CVAR-Categories.md`](CVAR-Categories.md).')
$lines.Add("")
$lines.Add("## Flag Legend")
$lines.Add("")
$lines.Add('- Position 1: `A` = archived, space = not archived')
$lines.Add('- Position 2: `U` = userinfo, `S` = serverinfo, `C` = auto/custom, space = local/general')
$lines.Add('- Position 3: `-` = write-protected, `L` = latched, `*` = unsettable auto cvar, space = writable')
$lines.Add('- Position 4: `M` = modified/session-marked')
$lines.Add('- Position 5: `X` = ignored/hidden from normal flow')
$lines.Add("")
$lines.Add("## Source Catalog by Category")
$lines.Add("")
$lines.Add("Compact index of source-defined CVARs grouped by category.")
$lines.Add("")

foreach ($category in @($categoryConfig.Categories)) {
    $categoryId = [string]$category.id
    if (-not $categoryGroups.ContainsKey($categoryId) -or $categoryGroups[$categoryId].Count -eq 0) {
        continue
    }

    $anchor = $categoryId.ToLowerInvariant()
    $lines.Add(("### Category: {0} {{#category-{1}}}" -f [string]$category.title, $anchor))
    $lines.Add("")
    $lines.Add([string]$category.description)
    $lines.Add("")
    $lines.Add("| CVAR | Type | Default | Description | Source |")
    $lines.Add("| --- | --- | --- | --- | --- |")

    foreach ($definition in ($categoryGroups[$categoryId] | Sort-Object Name)) {
        $name = [string]$definition.Name
        $description = Get-DescriptionForCvar -Name $name
        $lines.Add((Format-MarkdownTableRow @(
            ('`{0}`' -f $name),
            [string]$definition.Type,
            [string]$definition.Default,
            $description,
            ('`{0}:{1}`' -f $definition.Source, $definition.Line)
        )))
    }
    $lines.Add("")
}

$lines.Add("## HCDE Server, Invasion, and Netcode CVARs")
$lines.Add("")
$lines.Add("These are the high-value controls for invasion, net diagnostics, compatibility, and heavy-load cleanup.")
$lines.Add("")
foreach ($name in (@($hcdeFocusSet) | Sort-Object)) {
    $present = if ($runtimeNameSet.Contains($name)) { "Yes" } else { if ($SourceOnly) { "n/a (source-only generation)" } else { "No (not in this runtime snapshot)" } }
    $sourceDefinition = if ($sourceByName.ContainsKey($name)) { $sourceByName[$name] } else { $null }
    $defaultValue = if ($null -ne $sourceDefinition) {
        [string]$sourceDefinition.Default
    } elseif ($invasionDefaultFallback.ContainsKey($name)) {
        [string]$invasionDefaultFallback[$name]
    } elseif ($runtimeNameSet.Contains($name)) {
        [string]$runtimeByName[$name].Value
    } else {
        "n/a"
    }
    $range = if ($invasionRanges.ContainsKey($name)) { [string]$invasionRanges[$name] } else { "n/a" }
    $sourceText = if ($null -ne $sourceDefinition) { ("{0}:{1}" -f $sourceDefinition.Source, $sourceDefinition.Line) } else { "Not found in source scan" }
    $runtimeValue = if ($runtimeNameSet.Contains($name)) { [string]$runtimeByName[$name].Value } else { "n/a" }
    $category = Get-CvarCategoryForName -Name $name -CategoryConfig $categoryConfig
    $lines.Add(('### `{0}`' -f $name))
    $lines.Add("")
    $lines.Add(('- Category: [{0}](#category-{1})' -f $category.title, ([string]$category.id).ToLowerInvariant()))
    $lines.Add(('- Description: {0}' -f (Get-DescriptionForCvar -Name $name)))
    $lines.Add(('- Source default: `{0}`' -f $defaultValue))
    $lines.Add(('- Valid range/shape: `{0}`' -f $range))
    $lines.Add(('- Source: `{0}`' -f $sourceText))
    $lines.Add(('- Present in runtime snapshot: {0}' -f $present))
    $lines.Add(('- Runtime snapshot value: `{0}`' -f $runtimeValue))
    $lines.Add("")
}

$lines.Add("## Source-Defined CVAR Catalog")
$lines.Add("")
$lines.Add("This section is generated from `CVAR`, `CUSTOM_CVAR`, `CVARD`, `CUSTOM_CVARD`, and named CVAR macros in `src/`.")
$lines.Add("")
foreach ($definition in ($sourceByName.Values | Sort-Object Name)) {
    $name = [string]$definition.Name
    $runtimePresent = if ($runtimeNameSet.Contains($name)) { "Yes" } else { if ($SourceOnly) { "n/a" } else { "No" } }
    $runtimeValue = if ($runtimeNameSet.Contains($name)) { [string]$runtimeByName[$name].Value } else { "n/a" }
    $refText = if ($definition.RefName -ne $definition.Name) { [string]$definition.RefName } else { "same as cvar name" }
    $category = Get-CvarCategoryForName -Name $name -CategoryConfig $categoryConfig

    $lines.Add(('### `{0}`' -f $name))
    $lines.Add("")
    $lines.Add(('- Category: [{0}](#category-{1})' -f $category.title, ([string]$category.id).ToLowerInvariant()))
    $lines.Add(('- Description: {0}' -f (Get-DescriptionForCvar -Name $name)))
    $lines.Add(('- Type: `{0}`' -f $definition.Type))
    $lines.Add(('- Source default: `{0}`' -f $definition.Default))
    $lines.Add(('- Source flags: `{0}`' -f $definition.Flags))
    $lines.Add(('- Macro: `{0}`' -f $definition.Macro))
    $lines.Add(('- Ref symbol: `{0}`' -f $refText))
    $lines.Add(('- Source: `{0}:{1}`' -f $definition.Source, $definition.Line))
    $lines.Add(('- Present in runtime snapshot: {0}' -f $runtimePresent))
    $lines.Add(('- Runtime snapshot value: `{0}`' -f $runtimeValue))
    $lines.Add("")
}

if (-not $SourceOnly) {
    $lines.Add("## Full Runtime CVAR Catalog")
    $lines.Add("")
    foreach ($c in $cvars) {
        $name = [string]$c.Name
        $value = [string]$c.Value
        $prefix = [string]$c.Prefix
        $flagsText = if ([string]::IsNullOrWhiteSpace($prefix)) { "(none)" } else { ('"{0}"' -f $prefix) }
        $decoded = Decode-Flags -Flags $prefix
        $category = Get-CvarCategoryForName -Name $name -CategoryConfig $categoryConfig

        $tested = if ($c.IsNoSet -eq 'True') { 'No (write-protected in flag map)' } else { 'Yes (set/get sweep)' }
        $getResult = if ($c.IsNoSet -eq 'True') {
            'Not applicable'
        } elseif ($missingGetSet.Contains($name)) {
            'Missing response (investigate)'
        } else {
            'OK'
        }

        $latchObserved = if ($latchedSet.Contains($name)) { 'Yes (latched message observed during set pass)' } else { 'No' }
        $writeProtectedObserved = if ($writeProtectedSet.Contains($name)) { 'Yes' } else { 'No' }

        $lines.Add(('### `{0}`' -f $name))
        $lines.Add("")
        $lines.Add(('- Category: [{0}](#category-{1})' -f $category.title, ([string]$category.id).ToLowerInvariant()))
        $lines.Add(('- Description: {0}' -f (Get-DescriptionForCvar -Name $name)))
        $lines.Add(('- Current value: `{0}`' -f $value))
        $lines.Add(('- Raw flag field: {0}' -f $flagsText))
        $lines.Add(('- Archive: {0}' -f $decoded.Archive))
        $lines.Add(('- Scope/type: {0}' -f $decoded.Scope))
        $lines.Add(('- Mutability: {0}' -f $decoded.Mutability))
        $lines.Add(('- Modified flag (`M`): {0}' -f $decoded.Mod))
        $lines.Add(('- Ignored flag (`X`): {0}' -f $decoded.Ignored))
        $lines.Add(('- Set/get tested: {0}' -f $tested))
        $lines.Add(('- Set/get result: {0}' -f $getResult))
        $lines.Add(('- Latched behavior observed: {0}' -f $latchObserved))
        $lines.Add(('- Write-protected message observed in sweep: {0}' -f $writeProtectedObserved))
        $lines.Add("")
    }
}

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllLines($OutputPath, $lines, $utf8NoBom)

$categoryLines = New-Object System.Collections.Generic.List[string]
$categoryLines.Add("# HCDE CVAR Categories")
$categoryLines.Add("")
$categoryLines.Add(("Generated: {0} UTC" -f $stampUtc))
$categoryLines.Add("")
$categoryLines.Add("Compact, category-first index of source-defined CVARs.")
$categoryLines.Add("")
$categoryLines.Add('Category rules live in [`tools/cvar-categories.json`](../tools/cvar-categories.json).')
$categoryLines.Add('The full reference with runtime audit data is in [`wiki/CVAR-Reference.md`](CVAR-Reference.md).')
$categoryLines.Add("")
$categoryLines.Add("## Category Summary")
$categoryLines.Add("")
$categoryLines.Add("| Category | CVARs |")
$categoryLines.Add("| --- | ---: |")
foreach ($category in @($categoryConfig.Categories)) {
    $categoryId = [string]$category.id
    $count = if ($categoryGroups.ContainsKey($categoryId)) { $categoryGroups[$categoryId].Count } else { 0 }
    if ($count -eq 0) {
        continue
    }
    $categoryLines.Add((Format-MarkdownTableRow @([string]$category.title, [string]$count)))
}
$categoryLines.Add("")

foreach ($category in @($categoryConfig.Categories)) {
    $categoryId = [string]$category.id
    if (-not $categoryGroups.ContainsKey($categoryId) -or $categoryGroups[$categoryId].Count -eq 0) {
        continue
    }

    $categoryLines.Add(("## {0}" -f [string]$category.title))
    $categoryLines.Add("")
    $categoryLines.Add([string]$category.description)
    $categoryLines.Add("")
    foreach ($definition in ($categoryGroups[$categoryId] | Sort-Object Name)) {
        $name = [string]$definition.Name
        $description = Get-DescriptionForCvar -Name $name
        $categoryLines.Add(('- `{0}` — {1}' -f $name, $description))
    }
    $categoryLines.Add("")
}

$categoryOutDir = Split-Path -Parent $CategoryCatalogPath
if (-not (Test-Path -LiteralPath $categoryOutDir)) {
    New-Item -ItemType Directory -Path $categoryOutDir -Force | Out-Null
}
[System.IO.File]::WriteAllLines($CategoryCatalogPath, $categoryLines, $utf8NoBom)

Write-Output ("Wrote {0} with {1} source CVAR entries{2}." -f $OutputPath, $sourceByName.Count, ($(if ($SourceOnly) { "" } else { " and {0} imported runtime CVAR entries" -f $cvars.Count })))
Write-Output ("Wrote {0}." -f $CategoryCatalogPath)
