# Chunk 1028 — MAPINFO media/scalar DEFINE options from `g_mapinfo.cpp` (**1191–1340**). Follows chunk **1027** (**833–982**). Skips **1041–1190** (`VmMapInfoParseNextMap`, `VmMapInfoShadowQualityCap`, `VmMapInfoMapOptionsCore`).

## Scope

MAPINFO media/scalar DEFINE options from `g_mapinfo.cpp` (**1191–1340**). Follows chunk **1027** (**833–982**). Skips **1041–1190** (`VmMapInfoParseNextMap`, `VmMapInfoShadowQualityCap`, `VmMapInfoMapOptionsCore`).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION media scalars** 1191–1340 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoMapOptionsMedia1028Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `b845fe16956a38b7903c74d12fb0a1bc57f914cf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `skymistyscale` clamp (**1194–1198**) | yes |
| `skybox` speed zero (**1202–1207**) | yes |
| `titlepatch` hide-author comma (**1227–1234**) | yes |
| `partime` / `par` (**1238–1249**) | yes |
| `wavelimit` / `duellimit` / `sucktime` (**1252–1270**) | yes |
| `mapintermusic` insert (**1285–1294**) | yes |
| `evenlighting` (**1309–1311**) | yes |
| `cdtrack` / `cdid` / `warptrans` (**1314–1332**) | yes |
| `vertwallshade` / `horizwallshade` (**1335–1340**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **754–832** | ParseExit* / `VmMapInfoMapOptionsExitEnvironment` |
| **833–982** | chunk **1027** |
| **1008–1027** | `VmMapInfoClusterExitTextRemap` |
| **1041–1190** | off-tally Vm next/shadow/core bands |
| **1180–1192** | `skymist` float tail + closing brace (outside hull start **1191**) |
| **1209–1221**, **1273–1307** | color/lump host calls outside ported predicates |
| **1341+** | outside hull |
| Retired **321** `VmMapInfoMapOptionsScalars` | duplicate clamp helpers — not re-listed in audit map rows |

| Managed (`VmMapInfoMapOptionsMedia1028Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `ClampSkyMistYScale` | **1194–1198** |
| `TryApplyOption` (`skybox`, limits, cd*, warp, shades) | **1202–1332** |
| `ApplyTitlePatchFlags` | **1227–1234** |
| `StoreMapInterMusic` | **1285–1294** |
| `ApplyEvenLighting` | **1309–1311** |
| Band hull (exclusive slice) | **1191–1340** |

### Chunk **1028** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1027** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoMapOptionsMedia1028SliceTests.ApplyTitlePatchFlags_SetsHideAuthorOnNewFormatComma`
- `VmMapInfoMapOptionsMedia1028SliceTests.StoreMapInterMusic_InsertsPair`
- `VmMapInfoMapOptionsMedia1028SliceTests.TryApplyOption_WaveAndDuelLimitsClamp`
- `VmMapInfoMapOptionsMedia1028SliceTests.TryApplyOption_WallShadesHalveAndClamp`
- `VmMapInfoMapOptionsMedia1028SliceTests.TryApplyOption_SkyboxClearsSpeed`
- `VmMapInfoMapOptionsMedia1028SliceTests.TryApplyOption_SkymistyScaleClamps`
- `VmMapInfoMapOptionsMedia1028SliceTests.TryParseCdIdHex_ParsesHex`
- `VmMapInfoMapOptionsMedia1028SliceTests.ApplyEvenLighting_ZerosWallLights`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1028 --to 1028` (0 hard / 0 soft).

## Seams

`DEFINE_MAP_OPTION` macro expansion, `parse.ParseAssign`, `V_GetColor`, `ParseMusic`, and `ParseLumpOrTextureName` remain native/host.

## Credit rationale

First tally-clean **150**-line `g_mapinfo.cpp` hull at **≥1191** with no conversion/recognition overlap and no Vm* line-range collision on the band.
