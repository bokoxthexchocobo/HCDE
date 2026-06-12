# Shared CVAR metadata helpers: source scanning, category resolution, and description lookup.
# Dot-source from tools/generate-cvars-doc.ps1 and tools/audit-cvar-metadata.ps1.

Set-StrictMode -Version Latest

function Get-CvarCategoriesConfigPath {
    param([string]$ToolsRoot = $PSScriptRoot)
    return Join-Path $ToolsRoot "cvar-categories.json"
}

function Import-CvarCategoriesConfig {
    param([string]$ConfigPath = "")

    if ([string]::IsNullOrWhiteSpace($ConfigPath)) {
        $ConfigPath = Get-CvarCategoriesConfigPath
    }
    if (-not (Test-Path -LiteralPath $ConfigPath)) {
        throw "CVAR category config not found: $ConfigPath"
    }

    $raw = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $byId = @{}
    foreach ($category in @($raw.categories)) {
        $byId[[string]$category.id] = $category
    }

    return [pscustomobject]@{
        Version              = [int]$raw.version
        Categories           = @($raw.categories | Sort-Object { [int]$_.order }, { [string]$_.title })
        CategoryById         = $byId
        Rules                = @($raw.rules)
        Overrides            = @{}
        RequireDescription   = $raw.require_description
        ConfigPath           = $ConfigPath
    }
}

function Initialize-CvarCategoryConfig {
    param(
        [Parameter(Mandatory = $true)]$Config,
        [hashtable]$ManualOverrides = @{}
    )

    foreach ($property in $Config.PSObject.Properties) {
        if ($property.Name -eq "Overrides") {
            continue
        }
    }

    $overrides = @{}
    $configPath = Join-Path $PSScriptRoot "cvar-categories.json"
    if (Test-Path -LiteralPath $configPath) {
        $raw = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
        if ($null -ne $raw.overrides) {
            foreach ($property in $raw.overrides.PSObject.Properties) {
                $overrides[[string]$property.Name] = [string]$property.Value
            }
        }
    }
    foreach ($key in $ManualOverrides.Keys) {
        $overrides[[string]$key] = [string]$ManualOverrides[$key]
    }
    $Config | Add-Member -NotePropertyName ResolvedOverrides -NotePropertyValue $overrides -Force
    return $Config
}

function Test-CvarNameMatchesRule {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$Rule
    )

    if ($null -ne $Rule.PSObject.Properties["exact"]) {
        foreach ($exact in @($Rule.exact)) {
            if ([string]$exact -eq $Name) {
                return $true
            }
        }
    }

    if ($null -ne $Rule.PSObject.Properties["prefixes"]) {
        foreach ($prefix in @($Rule.prefixes)) {
            $prefixText = [string]$prefix
            if ([string]::IsNullOrWhiteSpace($prefixText)) {
                continue
            }
            if ($Name.StartsWith($prefixText, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
    }

    if ($null -ne $Rule.PSObject.Properties["patterns"]) {
        foreach ($pattern in @($Rule.patterns)) {
            $patternText = [string]$pattern
            if ([string]::IsNullOrWhiteSpace($patternText)) {
                continue
            }
            if ($Name -match $patternText) {
                return $true
            }
        }
    }

    return $false
}

function Get-CvarCategoryId {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$CategoryConfig
    )

    if ($CategoryConfig.ResolvedOverrides.ContainsKey($Name)) {
        return [string]$CategoryConfig.ResolvedOverrides[$Name]
    }

    foreach ($rule in @($CategoryConfig.Rules)) {
        if (Test-CvarNameMatchesRule -Name $Name -Rule $rule) {
            return [string]$rule.category
        }
    }

    return "misc"
}

function Get-CvarCategoryForName {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$CategoryConfig
    )

    $categoryId = Get-CvarCategoryId -Name $Name -CategoryConfig $CategoryConfig
    if ($CategoryConfig.CategoryById.ContainsKey($categoryId)) {
        return $CategoryConfig.CategoryById[$categoryId]
    }

    return $CategoryConfig.CategoryById["misc"]
}

function Get-RelativeRepoPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $rootFull = (Resolve-Path -LiteralPath $Root).ProviderPath.TrimEnd('\', '/') + '\'
    $pathFull = (Resolve-Path -LiteralPath $Path).ProviderPath
    if ($pathFull.StartsWith($rootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $pathFull.Substring($rootFull.Length).Replace('\', '/')
    }
    return $pathFull.Replace('\', '/')
}

function Split-CvarMacroArguments {
    param([Parameter(Mandatory = $true)][string]$Text)

    $args = New-Object System.Collections.Generic.List[string]
    $current = New-Object System.Text.StringBuilder
    $depth = 0
    $inString = $false
    $escaped = $false

    for ($i = 0; $i -lt $Text.Length; $i++) {
        $ch = $Text[$i]

        if ($inString) {
            [void]$current.Append($ch)
            if ($escaped) {
                $escaped = $false
                continue
            }
            if ($ch -eq [char]'\') {
                $escaped = $true
                continue
            }
            if ($ch -eq [char]'"') {
                $inString = $false
            }
            continue
        }

        if ($ch -eq [char]'"') {
            $inString = $true
            [void]$current.Append($ch)
            continue
        }

        if ($ch -eq [char]'(' -or $ch -eq [char]'[' -or $ch -eq [char]'{' -or $ch -eq [char]'<') {
            $depth++
        } elseif ($ch -eq [char]')' -or $ch -eq [char]']' -or $ch -eq [char]'}' -or $ch -eq [char]'>') {
            if ($depth -gt 0) {
                $depth--
            }
        }

        if ($ch -eq [char]',' -and $depth -eq 0) {
            $args.Add($current.ToString().Trim())
            [void]$current.Clear()
            continue
        }

        [void]$current.Append($ch)
    }

    $args.Add($current.ToString().Trim())
    return [string[]]$args.ToArray()
}

function Find-MatchingParenIndex {
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][int]$OpenParenIndex
    )

    $depth = 0
    $inString = $false
    $escaped = $false

    for ($i = $OpenParenIndex; $i -lt $Content.Length; $i++) {
        $ch = $Content[$i]

        if ($inString) {
            if ($escaped) {
                $escaped = $false
                continue
            }
            if ($ch -eq [char]'\') {
                $escaped = $true
                continue
            }
            if ($ch -eq [char]'"') {
                $inString = $false
            }
            continue
        }

        if ($ch -eq [char]'"') {
            $inString = $true
            continue
        }
        if ($ch -eq [char]'(') {
            $depth++
            continue
        }
        if ($ch -eq [char]')') {
            $depth--
            if ($depth -eq 0) {
                return $i
            }
        }
    }

    return -1
}

function Get-LineNumberForOffset {
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][int]$Offset
    )

    if ($Offset -le 0) {
        return 1
    }

    return ([regex]::Matches($Content.Substring(0, $Offset), "`n").Count + 1)
}

function Convert-CvarDescriptionLiteral {
    param([string]$Raw)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        return ""
    }

    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($m in [regex]::Matches($Raw, '"((?:\\.|[^"\\])*)"')) {
        $piece = [string]$m.Groups[1].Value
        $piece = $piece.Replace('\"', '"').Replace('\\', '\')
        $parts.Add($piece)
    }

    if ($parts.Count -gt 0) {
        return (($parts.ToArray()) -join "").Trim()
    }

    return $Raw.Trim().Trim('"')
}

function Get-SourceCvarDefinitions {
    param([string]$Root)

    $definitions = New-Object System.Collections.Generic.List[object]
    $srcRoot = Join-Path $Root "src"
    if (-not (Test-Path -LiteralPath $srcRoot)) {
        return @()
    }

    $files = Get-ChildItem -Path (Join-Path $srcRoot "*") -Recurse -Include *.cpp,*.c,*.h,*.inl -File
    $rxMacro = [regex]'\b(?<macro>CUSTOM_CVARD|CUSTOM_CVAR_NAMED|CUSTOM_CVAR|CVARD_NAMED|CVARD|CVAR)\s*\('

    foreach ($file in $files) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($m in $rxMacro.Matches($content)) {
            $lineNumber = Get-LineNumberForOffset -Content $content -Offset $m.Index
            $lineStart = $content.LastIndexOf("`n", [Math]::Max($m.Index - 1, 0))
            if ($lineStart -lt 0) {
                $lineStart = 0
            }
            $lineEnd = $content.IndexOf("`n", $m.Index)
            if ($lineEnd -lt 0) {
                $lineEnd = $content.Length
            }
            $lineText = $content.Substring($lineStart, $lineEnd - $lineStart).Trim()
            if ($lineText.StartsWith("#define")) {
                continue
            }

            $openParen = $m.Index + $m.Length - 1
            $closeParen = Find-MatchingParenIndex -Content $content -OpenParenIndex $openParen
            if ($closeParen -lt 0) {
                continue
            }

            $argsText = $content.Substring($openParen + 1, $closeParen - $openParen - 1)
            $args = @(Split-CvarMacroArguments -Text $argsText)
            if (@($args).Count -lt 4) {
                continue
            }

            try {
            $macro = [string]$m.Groups["macro"].Value
            $type = [string]$args[0]
            $name = ""
            $refName = ""
            $defaultValue = ""
            $flags = ""
            $description = ""

            if ($macro -eq "CUSTOM_CVAR_NAMED" -or $macro -eq "CVARD_NAMED") {
                if ($args.Count -lt 5) {
                    continue
                }
                $refName = [string]$args[1]
                $name = [string]$args[2]
                $defaultValue = [string]$args[3]
                $flags = [string]$args[4]
                if ($macro -eq "CVARD_NAMED" -and $args.Count -ge 6) {
                    $description = Convert-CvarDescriptionLiteral -Raw ([string]$args[5])
                }
            } elseif ($type -eq "Flag") {
                $name = [string]$args[1]
                $refName = [string]$args[1]
                $defaultValue = [string]$args[2]
                $flags = [string]$args[3]
                $description = ("Flag alias backed by `{0}`." -f $defaultValue)
            } else {
                $name = [string]$args[1]
                $refName = [string]$args[1]
                $defaultValue = [string]$args[2]
                $flags = [string]$args[3]
                if (($macro -eq "CUSTOM_CVARD" -or $macro -eq "CVARD") -and $args.Count -ge 5) {
                    $description = Convert-CvarDescriptionLiteral -Raw ([string]$args[4])
                }
            }

            $name = $name.Trim()
            $refName = $refName.Trim()
            if ([string]::IsNullOrWhiteSpace($name) -or $name.Contains("#")) {
                continue
            }

            $definitions.Add([pscustomobject]@{
                Name        = $name
                RefName     = $refName
                Type        = $type.Trim()
                Default     = $defaultValue.Trim()
                Flags       = $flags.Trim()
                Macro       = $macro
                Description = $description
                Source      = (Get-RelativeRepoPath -Root $Root -Path $file.FullName)
                Line        = $lineNumber
            })
            } catch {
                continue
            }
        }
    }

    return ,@($definitions | Sort-Object Name, Source, Line)
}

function Get-SourceDescriptions {
    param([string]$Root)

    $map = @{}
    $srcRoot = Join-Path $Root "src"
    if (-not (Test-Path -LiteralPath $srcRoot)) {
        return $map
    }

    $files = Get-ChildItem -Path (Join-Path $srcRoot "*") -Recurse -Include *.cpp,*.h,*.inl -File
    $rxCvarde = [regex]'(?:CUSTOM_CVARD|CVARD)\(\s*[^,]+,\s*([A-Za-z_][A-Za-z0-9_]*)\s*,\s*[^,]+,\s*[^,]+,\s*([^)]+)\)'

    foreach ($file in $files) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($m in $rxCvarde.Matches($content)) {
            $name = [string]$m.Groups[1].Value
            $descRaw = [string]$m.Groups[2].Value
            if ([string]::IsNullOrWhiteSpace($name) -or [string]::IsNullOrWhiteSpace($descRaw)) {
                continue
            }
            $desc = $descRaw.Trim().Trim('"')
            if (-not [string]::IsNullOrWhiteSpace($desc)) {
                $map[$name] = $desc
            }
        }
    }
    return $map
}

function Get-ServerGuiDescriptions {
    param([string]$Root)

    $map = @{}
    $file = Join-Path $Root (Join-Path "src" (Join-Path "common" (Join-Path "platform" (Join-Path "win32" "i_mainwindow.cpp"))))
    if (-not (Test-Path -LiteralPath $file)) {
        return $map
    }

    $lines = Get-Content -LiteralPath $file
    $rx = [regex]'\{\s*L"([^"]+)"\s*,\s*"([A-Za-z_][A-Za-z0-9_]*)"'
    foreach ($line in $lines) {
        $m = $rx.Match($line)
        if (-not $m.Success) {
            continue
        }
        $label = [string]$m.Groups[1].Value
        $name = [string]$m.Groups[2].Value
        if ([string]::IsNullOrWhiteSpace($label) -or [string]::IsNullOrWhiteSpace($name)) {
            continue
        }
        $map[$name] = ("Server setting: {0}" -f $label)
    }
    return $map
}

function Get-ReadableHintFromName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return "No description available."
    }

    $work = $Name
    $scope = ""
    if ($work.StartsWith("sv_")) {
        $scope = "server"
        $work = $work.Substring(3)
    } elseif ($work.StartsWith("cl_")) {
        $scope = "client"
        $work = $work.Substring(3)
    } elseif ($work.StartsWith("co_")) {
        $scope = "co-op"
        $work = $work.Substring(3)
    } elseif ($work.StartsWith("g_")) {
        $scope = "gameplay"
        $work = $work.Substring(2)
    } elseif ($work.StartsWith("net_")) {
        $scope = "network"
        $work = $work.Substring(4)
    }

    $tokens = $work -split "_"
    $tokenText = (($tokens | Where-Object { $_ -ne "" }) -join " ").Trim()
    if ([string]::IsNullOrWhiteSpace($tokenText)) {
        $tokenText = $Name
    }

    if ([string]::IsNullOrWhiteSpace($scope)) {
        return ("Likely controls {0}." -f $tokenText)
    }
    return ("Likely controls {0} behavior for {1}." -f $tokenText, $scope)
}

function Get-ManualCvarDescriptions {
    return @{
        "hcde_lag_hud"                       = 'Persistent on-screen lag/invasion overlay (top-left). Also enable with `stat hcde_lag`.'
        "hcde_hud_debug"                     = 'Mirror net diagnostics to the HUD console for live operator visibility.'
        "hcde_startup_profile"               = 'Emit startup timing profile data for engine initialization diagnostics.'
        "hcde_nanobsp_loader"                = 'Selects NanoBSP loader mode for map geometry ingestion (0=off, 1=on, 2=force).'
        "hcde_shadow_autofallback"           = 'Automatically disable shadow maps when the renderer reports unsupported or failing shadow-map paths.'
        "hcde_shadow_autobudget"             = 'Adaptively reduce shadow-casting light count to stay near the target shadow-map frame budget.'
        "hcde_shadow_autobudget_targetms"    = 'Target milliseconds per frame allocated to shadow-map rendering when auto-budget is enabled.'
        "hcde_shadow_autobudget_minlights"   = 'Minimum number of shadow-casting lights retained while auto-budget throttles the light count.'
        "hcde_shadow_autobudget_step"        = 'Number of shadow-casting lights removed or restored per auto-budget adjustment step.'
        "hcde_shadow_forcealllights"         = 'Force eligible dynamic lights onto the shadow-map path even when not explicitly marked shadowmapped.'
        "hcde_k8vavoom_auto_profile"         = 'Automatically apply the k8vavoom lighting profile on capable hardware at video init (default on).'
        "hcde_k8vavoom_shadow_boost"         = 'Raise shadow-map quality floor and opt into Vulkan ray-query shadows when supported.'
        "hcde_k8vavoom_raylight_probe"       = 'Enable Vulkan ray-query dynamic light shadow attenuation when VK_KHR_ray_query is available.'
        "hcde_k8vavoom_lighting_profile"     = 'Master k8vavoom lighting preset (0=off, 1=on); composes shadowmaps and postprocess CVARs.'
        "vk_raytrace"                        = 'Enable Vulkan ray-query acceleration structures for dynamic light shadow attenuation.'
        "cl_hcde_predict_dedicated"          = 'Enable client-side movement prediction when connected to a dedicated HCDE server.'
        "snd_backend"                        = 'Audio backend selector: `openal` (default), `null` (silent), or `eternity` (spatial facade).'
        "sv_invasioncountdowntime"           = 'Seconds before wave 1 starts ("Prepare for invasion" countdown).'
        "sv_invasionspawntime"               = 'Wave spawn window length in seconds before cleanup phase.'
        "sv_invasioncleanuptime"             = 'Seconds allowed for cleanup phase after spawning ends.'
        "sv_invasionintermissiontime"        = 'Seconds between completed waves before the next wave starts.'
        "sv_invasionresulttime"              = 'Seconds to keep the final victory/failure state visible.'
        "sv_invasionwaves"                   = 'Maximum number of invasion waves in a run.'
        "sv_invasionbasebudget"              = 'Base monster budget each wave starts with.'
        "sv_invasionbudgetstep"              = 'Budget increase applied per wave number.'
        "sv_invasionperplayer"               = 'Additional budget per extra active player.'
        "sv_invasionspawninterval"           = 'Seconds between spawn ticks while wave spawning is active.'
        "sv_invasionspawnburst"              = 'Maximum monsters spawned per spawn tick burst.'
        "sv_invasionmaxactive"               = 'Optional cap for active invasion monsters. 0 disables the cap; positive values are clamped by the engine.'
        "sv_invasionbosswaveevery"           = 'Boss wave cadence (e.g. 5 = every 5th wave, 0 = never).'
        "sv_invasionbossbonus"               = 'Extra budget added during boss waves.'
        "sv_invasionspotusemaptags"          = 'Restrict native invasion spots by map thing TID/tag. Keep disabled for Skulltag/Zandronum map compatibility; the spot arguments already control wave timing.'
        "sv_invasionspotfallback"            = 'Fallback to generic spawning when tagged invasion spots cannot be used.'
        "sv_invasionsimlod"                  = 'Enables server-side simulation LOD for invasion monsters so distant actors think less often under heavy load.'
        "sv_invasionsimlodfullrange"         = 'Distance within which invasion monsters keep full-rate simulation.'
        "sv_invasionsimlodreducedrange"      = 'Distance within which invasion monsters use reduced-rate simulation before becoming dormant.'
        "sv_invasionsimlodreducedinterval"   = 'Think interval in tics for reduced-rate invasion simulation.'
        "sv_invasionsimloddormantinterval"   = 'Think interval in tics for dormant distant invasion simulation.'
        "sv_usemapsettingswavelimit"         = 'If enabled, map-defined invasion wavelimit metadata overrides sv_invasionwaves when present.'
        "wavelimit"                          = 'Legacy Skulltag compatibility override for invasion waves. 0 disables the override; 1..255 forces that wave count.'
        "duellimit"                          = 'Legacy Skulltag compatibility value for duel limit metadata.'
        "sv_corpsequeuesize"                 = 'Maximum queued corpses retained by corpse cleanup; used with sv_corpsefilter.'
        "sv_corpsefilter"                    = 'Selects which corpse queues sv_corpsequeuesize trims: 0 off, 1 monsters, 2 players, 3 both.'
        "net_predict_debug"                  = 'Controls HCDE prediction diagnostics: off, CSV sampling, and/or on-screen/debug trace output depending on level.'
        "net_predict_debug_interval"         = 'Tic interval used by prediction CSV/debug sampling.'
        "net_predict_softwarn_ack_lag"       = 'Soft warning threshold for client ack lag during prediction diagnostics.'
        "net_predict_softwarn_mirror_delta"  = 'Soft warning threshold for invasion mirror drift during prediction diagnostics.'
        "net_predict_softwarn_passive_storm" = 'Soft warning threshold for passive update storms during prediction diagnostics.'
        "net_hcde_native_only"               = 'Requires HCDE-native networking/capability paths for multiplayer sessions.'
    }
}

function Get-ManualCvarRanges {
    return @{
        "sv_invasioncountdowntime"           = ">= 0"
        "sv_invasionspawntime"               = ">= 0"
        "sv_invasioncleanuptime"             = ">= 0"
        "sv_invasionintermissiontime"        = ">= 0"
        "sv_invasionresulttime"              = ">= 0"
        "sv_invasionwaves"                   = "1..255"
        "sv_invasionbasebudget"              = ">= 1"
        "sv_invasionbudgetstep"              = ">= 0"
        "sv_invasionperplayer"               = ">= 0"
        "sv_invasionspawninterval"           = ">= 0.05"
        "sv_invasionspawnburst"              = ">= 1"
        "sv_invasionmaxactive"               = "0 or 1..1024"
        "sv_invasionbosswaveevery"           = ">= 0"
        "sv_invasionbossbonus"               = ">= 0"
        "sv_invasionspotusemaptags"          = "bool"
        "sv_invasionspotfallback"            = "bool"
        "sv_invasionsimlod"                  = "bool"
        "sv_invasionsimlodfullrange"         = ">= 0"
        "sv_invasionsimlodreducedrange"      = ">= sv_invasionsimlodfullrange"
        "sv_invasionsimlodreducedinterval"   = ">= 1 tic"
        "sv_invasionsimloddormantinterval"   = ">= 1 tic"
        "sv_usemapsettingswavelimit"         = "bool"
        "wavelimit"                          = "0..255"
        "duellimit"                          = "0..255"
        "sv_corpsequeuesize"                 = ">= 0"
        "sv_corpsefilter"                    = "0..3"
    }
}

function New-CvarDescriptionResolver {
    param(
        [Parameter(Mandatory = $true)][string]$RepoRoot,
        [hashtable]$ExtraManualDescriptions = @{}
    )

    $manualDescriptions = Get-ManualCvarDescriptions
    foreach ($key in $ExtraManualDescriptions.Keys) {
        $manualDescriptions[[string]$key] = [string]$ExtraManualDescriptions[$key]
    }

    $sourceDescriptions = Get-SourceDescriptions -Root $RepoRoot
    $serverGuiDescriptions = Get-ServerGuiDescriptions -Root $RepoRoot
    $sourceDefinitions = Get-SourceCvarDefinitions -Root $RepoRoot
    foreach ($definition in $sourceDefinitions) {
        if (-not [string]::IsNullOrWhiteSpace($definition.Description)) {
            $sourceDescriptions[[string]$definition.Name] = [string]$definition.Description
        }
    }

    return [pscustomobject]@{
        ManualDescriptions    = $manualDescriptions
        SourceDescriptions    = $sourceDescriptions
        ServerGuiDescriptions = $serverGuiDescriptions
        SourceDefinitions     = $sourceDefinitions
        Resolve               = {
            param([string]$Name)
            if ($manualDescriptions.ContainsKey($Name)) {
                return [string]$manualDescriptions[$Name]
            }
            if ($sourceDescriptions.ContainsKey($Name)) {
                return [string]$sourceDescriptions[$Name]
            }
            if ($serverGuiDescriptions.ContainsKey($Name)) {
                return [string]$serverGuiDescriptions[$Name]
            }
            return (Get-ReadableHintFromName -Name $Name)
        }.GetNewClosure()
    }
}

function Test-CvarDescriptionIsHeuristic {
    param(
        [Parameter(Mandatory = $true)][string]$Description
    )

    return ($Description.StartsWith("Likely controls ", [System.StringComparison]::OrdinalIgnoreCase))
}

function Test-CvarRequiresDescription {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$CategoryConfig
    )

    $requirements = $CategoryConfig.RequireDescription
    if ($null -eq $requirements) {
        return $false
    }

    if ($null -ne $requirements.PSObject.Properties["exact"]) {
        foreach ($exact in @($requirements.exact)) {
            if ([string]$exact -eq $Name) {
                return $true
            }
        }
    }

    if ($null -ne $requirements.PSObject.Properties["prefixes"]) {
        foreach ($prefix in @($requirements.prefixes)) {
            $prefixText = [string]$prefix
            if ($Name.StartsWith($prefixText, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
    }

    return $false
}

function Group-CvarsByCategory {
    param(
        [Parameter(Mandatory = $true)][object[]]$Definitions,
        [Parameter(Mandatory = $true)]$CategoryConfig
    )

    $groups = @{}
    foreach ($category in @($CategoryConfig.Categories)) {
        $groups[[string]$category.id] = New-Object System.Collections.Generic.List[object]
    }
    if (-not $groups.ContainsKey("misc")) {
        $groups["misc"] = New-Object System.Collections.Generic.List[object]
    }

    $seen = @{}
    foreach ($definition in $Definitions) {
        $name = [string]$definition.Name
        if ($seen.ContainsKey($name)) {
            continue
        }
        $seen[$name] = $true
        $categoryId = Get-CvarCategoryId -Name $name -CategoryConfig $CategoryConfig
        if (-not $groups.ContainsKey($categoryId)) {
            $groups[$categoryId] = New-Object System.Collections.Generic.List[object]
        }
        $groups[$categoryId].Add($definition)
    }

    return $groups
}

function Format-MarkdownTableRow {
    param([string[]]$Cells)

    $escaped = @()
    foreach ($cell in $Cells) {
        $text = if ($null -eq $cell) { "" } else { [string]$cell }
        $text = $text.Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
        $escaped += $text
    }
    return ("| {0} |" -f ($escaped -join " | "))
}
