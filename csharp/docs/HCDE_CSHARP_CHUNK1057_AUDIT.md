# Chunk 1057 — `DEFINE_MAP_OPTION` continuation from `g_mapinfo.cpp` (**1341–1490**). Follows **1028** (**1191–1340**).

## Scope

`DEFINE_MAP_OPTION` continuation from `g_mapinfo.cpp` (**1341–1490**). Follows **1028** (**1191–1340**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION tail** 1341–1490 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoMapOptionsTail1057Slice.cs`
Managed LOC (tally): 132.
Conversion commit: `81f9a76c028f7d0e404390473786ffdb4d3beb93`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `horizwallshade` (**1342–1347**) | yes (via **1028** clamp seam) |
| `gravity` / `nogravity` (**1349–1359**) | yes |
| `aircontrol` / `airsupply` (**1361–1373**) | yes |
| inter/exit/enter pic & anim (**1375–1408**) | yes |
| `specialaction` (**1410–1433**) | yes (lookup fail + arg count) |
| precache lists (**1435–1487**) | yes |
| `redirect` head (**1489–1490**) | yes (type token only) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1191–1340** | chunk **1028** |
| **1341** | blank separator |
| **1491+** | `RedirectMapName` / `cvar_redirect` (**1498+**) |
| **754–832** | ParseExit* denylist |
| Retired **323** precache/redirect row | not re-tallied |

| Managed (`VmMapInfoMapOptionsTail1057Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `ApplyHorizWallShade` | **1342–1347** |
| gravity / air helpers | **1349–1373** |
| `StoreStringMapOption` | **1375–1408** |
| `SpecialActionLookupFailed` / arg count | **1410–1433** |
| precache push / comma list count | **1435–1487** |
| `BeginRedirectOption` | **1489–1490** |
| Band hull (exclusive slice) | **1341–1490** |

### Chunk **1057** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1056** |


**Post-review ratio:** 132/150 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoMapOptionsTail1057SliceTests.ApplyHorizWallShade_HalvesAndClamps`
- `VmMapInfoMapOptionsTail1057SliceTests.ApplyNoGravity_UsesDblMaxMarker`
- `VmMapInfoMapOptionsTail1057SliceTests.SpecialActionLookupFailed_RejectsZeroAction`
- `VmMapInfoMapOptionsTail1057SliceTests.CountTrailingSpecialActionArgs_StopsAtFive`
- `VmMapInfoMapOptionsTail1057SliceTests.ShouldPushPrecacheSound_SkipsNoSound`
- `VmMapInfoMapOptionsTail1057SliceTests.CountCommaSeparatedEntries_CountsCommas`
- `VmMapInfoMapOptionsTail1057SliceTests.ClassifyTailOptionKeyword_RecognizesPrecacheClasses`
- `VmMapInfoMapOptionsTail1057SliceTests.BeginRedirectOption_StoresTypeToken`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1057 --to 1057` (0 hard / 0 soft).

## Seams

`DEFINE_MAP_OPTION` macro expansion, `parse.ParseAssign`, `P_FindLineSpecial`, `S_FindSound`, and `ParseNextMap` on **1491+** remain host-injected; **1028** `ClampWallShade` reused without re-crediting **1191–1340**.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **1341–1490** (+150); excludes **1191–1340** (**1028**). Hull ends at **`redirect` option opening** (**1490**); map/cvar redirect tail (**1491+**) deferred without padding.
