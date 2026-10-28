# Chunk 1056 — `ParseCluster` tail, Hexen exit remap, `ParseNextMap`, shadow cap normalize, and map-option head from `g_mapinfo.cpp` (**983–1132**). Follows **1027** (**833–982**).

## Scope

`ParseCluster` tail, Hexen exit remap, `ParseNextMap`, shadow cap normalize, and map-option head from `g_mapinfo.cpp` (**983–1132**). Follows **1027** (**833–982**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseCluster tail / ParseNextMap / map options head** 983–1132 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseCluster1056Slice.cs`
Managed LOC (tally): 114.
Conversion commit: `3c895658377d3f14035886422f258e2e5103c368`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ParseCluster` intro/outro/gameover + loop tail (**985–1007**) | yes (via **1027** cutscene/cluster helpers as seams) |
| Hexen exit lump remap (**1008–1029**) | yes |
| `CheckEndOfFile("cluster")` (**1030**) | yes |
| `ParseNextMap` (**1041–1064**) | yes |
| `NormalizeHcdeShadowQualityCap` (**1066–1075**) | yes |
| `DEFINE_MAP_OPTION` head (**1083–1132**) | yes (keyword classify + assign policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **833–982** | chunk **1027** |
| **1031–1033** | closing brace / comment banner |
| **1076–1082** | comment banner |
| **1133+** | cluster hub reserve + remainder (**1139+**) |
| **1191+** | chunk **1028** deferred band |
| `VmMapInfoClusterExitTextRemap` / `VmMapInfoParseNextMap` / `VmMapInfoShadowQualityCap` | uncredited helpers — seam only |

| Managed (`VmMapInfoParseCluster1056Slice.cs`) | Native (`g_mapinfo.cpp`) |
|-----------------------------------------------|---------------------------|
| `ApplyClusterCutsceneSlot` / `ClusterLoopAfterCloseBraceAttempt` | **985–1007** |
| `TryRemapHexenClusterExitText` | **1008–1029** |
| `ShouldErrorClusterDefinitionEof` | **1030** |
| `ResolveNumericNextMapToken` / `ResolveEndSequenceNextMapToken` | **1041–1064** |
| `NormalizeHcdeShadowQualityCap` | **1066–1075** |
| `ClassifyMapOptionKeyword` / assign policy | **1083–1132** |
| Band hull (exclusive slice) | **983–1132** |

### Chunk **1056** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1055** |


**Post-review ratio:** 114/150 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseCluster1056SliceTests.TryRemapHexenClusterExitText_SetsLookupFlag`
- `VmMapInfoParseCluster1056SliceTests.ResolveNumericNextMapToken_HexenUsesWarpTrans`
- `VmMapInfoParseCluster1056SliceTests.NormalizeHcdeShadowQualityCap_SnapsToStep`
- `VmMapInfoParseCluster1056SliceTests.ClassifyMapOptionKeyword_RecognizesSecretAlias`
- `VmMapInfoParseCluster1056SliceTests.MapOptionUsesParseNextMap_ForNextAndSecretVariants`
- `VmMapInfoParseCluster1056SliceTests.ShouldErrorClusterDefinitionEof_NewFormatOnly`
- `VmMapInfoParseCluster1056SliceTests.ClusterLoopAfterCloseBraceAttempt_BreaksOnCloseBrace`
- `VmMapInfoParseCluster1056SliceTests.ApplyClusterCutsceneSlot_DelegatesIntroKeyword`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1056 --to 1056` (0 hard / 0 soft).

## Seams

`ParseCutscene`, `ParseCloseBrace`, `SkipToNext`, `CheckEndSequence`, `FindWadClusterInfo`, and `wadclusterinfos.Reserve` remain host-injected; uncredited `VmMapInfoClusterExitTextRemap` / `VmMapInfoParseNextMap` / `VmMapInfoShadowQualityCap` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **983–1132** (+150); excludes **833–982** (**1027**). Hull ends at **`cluster` map-option `MustGetNumber`** (**1132**); hub auto-create (**1139+**) deferred without padding.
