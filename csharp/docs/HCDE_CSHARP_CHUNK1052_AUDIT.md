# Chunk 1052 — `CheckWarpTransMap` tail, cluster lookup, snapshot/defered clears, and `level_info_t::Reset` head from `g_mapinfo.cpp` (**151–300**). Follows **1051** (**1–150**).

## Scope

`CheckWarpTransMap` tail, cluster lookup, snapshot/defered clears, and `level_info_t::Reset` head from `g_mapinfo.cpp` (**151–300**). Follows **1051** (**1–150**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **SecretLevelVisited / level_info_t::Reset head** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMid1052Slice.cs`
Managed LOC (tally): 183.
Conversion commit: `b50d0194ac31197f0ff7e38a1c51fed116f0ba71`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CheckWarpTransMap` tail (**151–154**) | yes (fallthrough **false**) |
| `SecretLevelVisited` (**161–168**) | yes |
| `FindWadClusterInfo` / `FindClusterInfo` (**175–197**) | yes |
| `G_ClearSnapshots` (**204–220**) | yes (attached-level + runtime extras predicates) |
| `P_RemoveDefereds` (**229–235**) | yes |
| `level_info_t::Reset` through **slideshow** (**243–300**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–150** | chunk **1051** |
| **208–216** | host `Snapshot.Clean` on live levels |
| **220** | `GlobalACSStrings.UnlockAll` host |
| **301+** | remainder of `Reset` + later MAPINFO parse (future band) |
| **833+** | chunks **1027–1036** |
| `VmMapInfoWarpTrans` / `VmMapInfoLevelReset` | uncredited off-tally helpers — seam only |

| Managed (`VmMapInfoParseMid1052Slice.cs`) | Native (`g_mapinfo.cpp`) |
|-------------------------------------------|---------------------------|
| `CheckWarpTransMapTailReturnsFalse` | **151–154** |
| `SecretLevelVisited` | **161–168** |
| `FindWadClusterIndex` / `ResolveClusterInfo` | **175–197** |
| `ShouldCleanAttachedLevelSnapshot` / `ExtraRuntimeSnapshotClears` | **204–220** |
| `ClearDeferedForEachLevel` | **229–235** |
| `ApplyLevelInfoResetHead` | **243–300** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1052** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1051** |


**Post-review ratio:** 183/150 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMid1052SliceTests.CheckWarpTransMapTailReturnsFalse_MatchesNativeFallthrough`
- `VmMapInfoParseMid1052SliceTests.SecretLevelVisited_DetectsVisitedSecret`
- `VmMapInfoParseMid1052SliceTests.ResolveClusterInfo_UsesDefaultWhenMissing`
- `VmMapInfoParseMid1052SliceTests.ResolveClusterInfo_FindsWadCluster`
- `VmMapInfoParseMid1052SliceTests.ShouldCleanAttachedLevelSnapshot_RequiresBoth`
- `VmMapInfoParseMid1052SliceTests.ExtraRuntimeSnapshotClears_CountsPrimaryAndVm`
- `VmMapInfoParseMid1052SliceTests.ClearDeferedForEachLevel_InvokesPerIndex`
- `VmMapInfoParseMid1052SliceTests.ApplyLevelInfoResetHead_SetsDoomLaxMonsterFlags2`
- `VmMapInfoParseMid1052SliceTests.ApplyLevelInfoResetHead_ClearsHexenFlags2`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1052 --to 1052` (0 hard / 0 soft).

## Seams

`Snapshot.Clean`, `ClearDefered`, `GlobalACSStrings.UnlockAll`, `primaryLevel` / `currentVMLevel`, and `gameinfo.gametype` remain host-injected; uncredited `VmMapInfoWarpTrans` / `VmMapInfoLevelReset` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **151–300** (+150); excludes **1–150** (**1051**). Hull ends at **`slideshow = NAME_None`** (**300**); **301+** deferred without padding.
