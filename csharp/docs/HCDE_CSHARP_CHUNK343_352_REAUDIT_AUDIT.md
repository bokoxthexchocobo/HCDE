# Chunks 343–352 re-audit (2026-10-20)

Bulk landing credited **2,177** native LOC while code commits added ~**252** `csharp/src` lines (tests/docs extra). Managed helpers were mostly policy stubs over wide CVAR/parser bands; `reaudit_chunks.py` reported **0/0** because tests existed on paper but did not cover most credited symbols.

This pass trims each row to symbol ranges with a named test per credited native symbol group. **`reaudit_chunks.py`** now soft-warns on `thin_managed_coverage` when commit `csharp/src` additions are **&lt; 50%** of credited `nativeLoc`.

## Per-chunk function map (native → managed → tests)

### Chunk 343 — `am-map-commands-colors-chunk343` (166 → **36** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `CCMD(togglemap)` | 246–251 | `VmAutomapCommands.ShouldRunToggleMap` | `VmAutomapCommandsTests.ShouldRunToggleMap_OnlyWhenIdle` |
| `CCMD(am_zoom)` | 254–260 | `VmAutomapCommands.ParseZoomDir` | `VmAutomapCommandsTests.ParseZoomDir_UsesArgument` |
| `AMColor` pack/valid | 326–348 | `VmAutomapColorArgb` | `VmAutomapColorArgbTests.FromRgb_PacksAlpha`, `Invalid_IsNotValid` |

**Removed:** CVAR blocks 184–318, unrelated CCMD bodies, `ColorNames` (352).  
**Commit:** `f7667961…` — managed src additions **33** LOC.

### Chunk 344 — `maploader-teleport-textures-chunk344` (173 → **43** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `MISSING_TEXTURE_WARN_LIMIT` | 60–63 | `VmMapLoaderMissingTexturePolicy.WarnLimit` | `ShouldPrintWarning_UntilLimit` |
| `TranslateTeleportThings` TID gate | 100–105 | `ShouldAssignTid` | `ShouldAssignTid_WhenSectorUntagged` |
| `TranslateTeleportThings` line args | 111–130 | `ApplyTeleportLineArgs` | `ApplyTeleportLineArgs_SetsTidWhenTagZero` |
| `SetTexture` warn gate | 151–157 | `ShouldPrintWarning` | `ShouldPrintWarning_UntilLimit` |
| `SetTexture` flat truncate | 187–192 | `TruncateFlatName` | `TruncateFlatName_EightChars` |

**Removed:** full `SetTexture` TexMan path, scanner loops. **Commit:** `049dbe1e…` — **37** src LOC.

### Chunk 345 — `g-mapinfo-warp-reset-chunk345` (207 → **31** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `CheckWarpTransMap` | 134–154 | `VmMapInfoWarpTrans.TryResolveWarpTransMap` | `TryResolveWarpTransMap_UsesLevelRecord` |
| `SecretLevelVisited` | 161–168 | `VmMapInfoWarpTrans.SecretLevelVisited` | `SecretLevelVisited_DetectsVisitedSecret` |
| `level_info_t::Reset` `flags2` | 257–260 | `VmMapInfoLevelReset.CreateDefaults` | `CreateDefaults_SetsHexenFlags2`, `CreateDefaults_SetsDoomLaxMonster` |

**Removed:** cluster lookup, snapshot clear, full `Reset` body. **Commit:** `7d8fadf3…` — **101** src LOC.

### Chunk 346 — `g-mapinfo-lookup-parser-format-chunk346` (182 → **57** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `LookupLevelName` header strip | 359–375 | `VmMapInfoLookupLevelName.StripLocalizedHeader` | `StripLocalizedHeader_RemovesMapPrefix` |
| `ParseOpenBrace` | 480–498 | `VmMapInfoParserFormat.ResolveOpenBrace` | `ResolveOpenBrace_DetectsNewFormat` |
| `ParseCloseBrace` | 505–518 | `VmMapInfoParserFormat.IsCloseBrace` | `ResolveOpenBrace_DetectsNewFormat` |
| `CheckEndOfFile` | 661–667 | `VmMapInfoParserFormat.EnsureNotUnexpectedEof` | `EnsureNotUnexpectedEof_ThrowsForNewFormat` |

**Removed:** `CheckLevelRedirect`, `cluster_info_t::Reset`, assign/float helpers. **Commit:** `6546c28c…` — **54** src LOC.

### Chunk 347 — `g-mapinfo-parser-mid-chunk347` (251 → **17** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `ParseLookupName` lookup/`$` | 678–694 | `VmMapInfoParseLookupName.TryParseLookupToken` | `TryParseLookupToken_StripsDollarPrefix` |

**Removed:** 525–775 band (exit text parsers, `SkipToNext`, etc.). **Commit:** `2b1afe40…` — **32** src LOC.

### Chunk 348 — `g-mapinfo-cluster-nextmap-chunk348` (307 → **54** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| Cluster Hexen exit remap | 1008–1027 | `VmMapInfoClusterExitTextRemap.TryRemapHexenExitLump` | `TryRemapHexenExitLump_BuildsKey` |
| `ParseNextMap` | 1041–1064 | `VmMapInfoParseNextMap` | `FormatNumericNextMap_HexenUsesWarpTrans`, `FormatEndSequenceMap_EncodesIndex` |
| `NormalizeHcdeShadowQualityCap` | 1066–1075 | `VmMapInfoShadowQualityCap.Normalize` | `Normalize_SnapsToKnownStep` |

**Removed:** exit music/backdrop parsers, full cluster parse loop. **Commit:** `3996db61…` — **51** src LOC.

### Chunk 349 — `dehacked-lookup-defaults-chunk349` (345 → **24** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `FindInfoName` index guard | 118–122 | `VmDehackedLookupCore.ResolveInfoNameIndex` | `ResolveInfoNameIndex_RespectsBounds` |
| `DehFindSound` index guard | 146–147 | `VmDehackedLookupCore.IsValidSoundIndex` | `IsValidSoundIndex_RespectsBounds` |
| `ReplaceSoundName` ds prefix | 162–163 | `VmDehackedLookupCore.DsSoundLumpName` | `DsSoundLumpName_PrefixesDs` |
**Removed:** DSDHacked actor spawn, sprite remap, `deh` struct, `DehSpriteMappings` table stub, **`Modes[]` (owned by chunk 263)**. **Commit:** `102214ee…` — **55** src LOC. **Trimmed native:** **9**.

### Chunk 350 — `dehacked-lump-loader-chunk350` (272 → **21** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `isDehFile` | 3213–3220 | `VmDehackedLumpLoader.IsRootDehOrBex` | `IsRootDehOrBex_RejectsNestedPaths` |
| `D_LoadDehLumps` IWAD skip | 3235–3238 | `ShouldSkipIwadLump` | `IsRootDehOrBex_RejectsNestedPaths` |
| `DoDehPatch` header version | 3349–3357 | `IsSupportedDehackedHeader` | `IsSupportedDehackedHeader_AcceptsV3` |

**Removed:** include stack, autoload `dehload` scan, full `D_LoadDehFile`. **Commit:** `225870cb…` — **22** src LOC.

### Chunk 351 — `d-main-cvar-policy-chunk351` (199 → **16** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `CUSTOM_CVAR(i_timescale)` net guard | 424–428 | `VmDMainCvarPolicy.ClampTimeScaleForNetGame` | `ClampTimeScaleForNetGame_ForcesOne` |
| `CUSTOM_CVAR(vid_rendermode)` clamp | 446–453 | `VmDMainCvarPolicy.ClampVidRenderMode` | `ClampVidRenderMode_RemapsSoftpoly` |

**Removed:** fraglimit, wipe, HUD toggle, FARG strings. **Commit:** `18e6c38b…` — **29** src LOC.

### Chunk 352 — `am-map-colorset-table-chunk352` (75 → **26** native)

| Native symbol | Lines | Managed | Named test(s) |
|---------------|------:|---------|---------------|
| `ColorNames[]` | 357–382 | `VmAutomapColorsetIndex.ColorNames` | `ColorNames_MatchesNativeTableLength`, `Indices_AlignWithFirstEntries` |

**Removed:** `AMColorset` enum body beyond mirrored indices. **Commit:** `1e946392…` — **25** src LOC.

## Totals

| Metric | Before trim | After trim |
|--------|------------:|-----------:|
| Chunks 343–352 native sum | 2,177 | **309** |
| `convertedBehaviorNative` (cumulative) | 34,918 | **33,139** (pre-chunk-353) |
| `recognitionBehaviorNative` | 5,591 | 5,591 |
| `remainingNative` | 467,577 | **469,356** (pre-chunk-353) |

## Tooling

`reaudit_chunks.py audit` now emits **`thin_managed_coverage`** when commit-measured `csharp/src` LOC &lt; 50% of `nativeLoc` (post-trim: **0** warnings on 343–352).

## Verified

`python3 tools/sync_loc_tally.py`; `python3 tools/generate_native_managed_matrix.py`; `python3 tools/reaudit_chunks.py audit --from 343 --to 352` (0 hard / 0 soft); full solution build + test after tally commit.
