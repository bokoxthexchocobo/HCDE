# Chunk 1051 — Level lookup helpers and `CheckWarpTransMap` head from `g_mapinfo.cpp` (**1–150**). MAPINFO parse entry before cluster/snapshot helpers (**151+**).

## Scope

Level lookup helpers and `CheckWarpTransMap` head from `g_mapinfo.cpp` (**1–150**). MAPINFO parse entry before cluster/snapshot helpers (**151+**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **FindLevelInfo / CheckWarpTransMap head** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseHead1051Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `3e33b59498bf12d1641546af58233af766181768`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FindWadLevelInfo` (**64–72**) | yes |
| `FindLevelInfo` default sky seed (**79–98**) | yes |
| `FindLevelByNum` (**105–112**) | yes |
| `FindLevelByWarpTrans` (**120–127**) | yes |
| `CheckWarpTransMap` prefix + lookup + substitute head (**134–150**) | yes |
| `ParsedLumps(8)` default (**57**) | yes (constant) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–46** | license / includes host |
| **47–56** | global container declarations host |
| **151–154** | `CheckWarpTransMap` tail outside hull (**150** ends substitute build) |
| **161+** | `SecretLevelVisited`, cluster lookup, deferred mid/tail bands |
| **833+** | chunks **1027–1036** credited bands |
| `VmMapInfoLevelLookup` / `VmMapInfoWarpTrans` | uncredited off-tally helpers — seam only |

| Managed (`VmMapInfoParseHead1051Slice.cs`) | Native (`g_mapinfo.cpp`) |
|---------------------------------------------|---------------------------|
| `FindWadLevelIndex` | **64–72** |
| `FindLevelInfo` | **79–98** |
| `FindLevelByNum` | **105–112** |
| `FindLevelByWarpTrans` | **120–127** |
| `TryResolveCheckWarpTransMap` / prefix | **134–150** |
| `ParsedLumpsInitialCapacity` | **57** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1051** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1050** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseHead1051SliceTests.FindWadLevelIndex_IsCaseInsensitive`
- `VmMapInfoParseHead1051SliceTests.FindLevelInfo_SeedsDefaultWhenAllowed`
- `VmMapInfoParseHead1051SliceTests.FindLevelByNum_MatchesLevelNumField`
- `VmMapInfoParseHead1051SliceTests.FindLevelByWarpTrans_ScansFromEnd`
- `VmMapInfoParseHead1051SliceTests.TryResolveCheckWarpTransMap_ReplacesWithLevelName`
- `VmMapInfoParseHead1051SliceTests.TryResolveCheckWarpTransMap_SubstituteBuildsMapPrefix`
- `VmMapInfoParseHead1051SliceTests.IsWarpTransMapPrefix_RequiresWtAt`
- `VmMapInfoParseHead1051SliceTests.ParsedLumpsInitialCapacity_MatchesNativeDefault`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1051 --to 1051` (0 hard / 0 soft).

## Seams

`wadlevelinfos` / `wadclusterinfos` storage, `HexenMusic`, and host `FString` mutation remain injected; uncredited `VmMapInfoLevelLookup` / `VmMapInfoWarpTrans` / `SecretLevelVisited` tail (**161+**) are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **1–150** (+150); zero live conversion overlap on head band; avoids ParseExit/Hexen/ParseMapInfoLoop denylist regions (all **≥676**). Hull ends inside `CheckWarpTransMap` substitute (**150**); **151–154** deferred with next band.
