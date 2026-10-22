# Chunk 718 — Whole `FLevelLocals::CollectConnectedGroups` (`portal.cpp` 1049–1184) plus `P_UseLines` / `P_UsePuzzleItem` (`p_map.cpp` 6003–6100).

## Scope

Whole `FLevelLocals::CollectConnectedGroups` (`portal.cpp` 1049–1184) plus `P_UseLines` / `P_UsePuzzleItem` (`p_map.cpp` 6003–6100).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::CollectConnectedGroups** 1049–1184 (136 lines)
- `src/playsim/p_map.cpp` **P_UseLines** 6003–6027 (25 lines)
- `src/playsim/p_map.cpp` **P_UsePuzzleItem** 6037–6100 (64 lines)

**Credited native LOC:** 225 (tally `nativeLoc` 225).

## Managed

`VmPortalCollectConnectedGroups.cs`, `VmPMapUseLinesPuzzle.cs`
Managed LOC (tally): 262.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `CollectConnectedGroups` / `WalkSectorPortals` / `WalkVerticalPortal` / `ShouldScanLinkedLinePortals` | `CollectConnectedGroups` 1049–1184 (`portal.cpp`) |
| `UseLines` / `UseButtonPressed` | `P_UseLines` 6003–6027 |
| `UsePuzzleItem` | `P_UsePuzzleItem` 6037–6100 |


**Post-review ratio:** 262/225 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalCollectConnectedGroupsTests.CollectConnectedGroups_SingleDisplacementGroup`
- `VmPortalCollectConnectedGroupsTests.CollectConnectedGroups_FollowsLinkedPortal`
- `VmPMapUseLinesPuzzleTests.UseLines_PlaysFailWhenBlocked`
- `VmPMapUseLinesPuzzleTests.UsePuzzleItem_MatchesLineArgs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 718 --to 718` (0 hard / 0 soft).

## Seams

`FPathTraverse`, blockmap iterators, `P_StartScript`, and `Net_DiagTraceUseLine` remain native.

## Credit rationale

Portal group discovery for linked portals plus use-line/puzzle item drivers hosted together (sub-150 portal tail paired with use helpers).
