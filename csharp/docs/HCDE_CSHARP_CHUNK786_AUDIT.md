# Chunk 786 — `FindRefPoint`, `PIT_FindFloorCeiling`, `P_GetFloorCeilingZ`, and `P_FindFloorCeiling` (`p_map.cpp` 214–420; excludes debug `ffcf` CCMD 425–440).

## Scope

`FindRefPoint`, `PIT_FindFloorCeiling`, `P_GetFloorCeilingZ`, and `P_FindFloorCeiling` (`p_map.cpp` 214–420; excludes debug `ffcf` CCMD 425–440).

## Native ranges

- `src/playsim/p_map.cpp` **FindRefPoint** 214–240 (27 lines)
- `src/playsim/p_map.cpp` **PIT_FindFloorCeiling** 252–315 (64 lines)
- `src/playsim/p_map.cpp` **P_GetFloorCeilingZ** 324–346 (23 lines)
- `src/playsim/p_map.cpp` **P_FindFloorCeiling** 354–420 (67 lines)

**Credited native LOC:** 181 (tally `nativeLoc` 181).

## Managed

`VmPMapFindFloorCeiling.cs`
Managed LOC (tally): 357.
Conversion commit: `d1db490a2b295d9b703c6be965850296df475b25`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|---------------------|
| `FindRefPoint` | **214–240** |
| `PitFindFloorCeiling` | **252–315** |
| `GetFloorCeilingZ` | **324–346** |
| `FindFloorCeiling` | **354–420** |

### Out-of-body uncredit (786)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **181** |
| **Out-of-body removed** | **0** |
| **Honest** | **181** |


**Post-review ratio:** 357/181 managed/native ≈ 197% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapFindFloorCeilingTests.FindRefPoint_ProjectsOntoSegmentWhenSlopesPresent`
- `VmPMapFindFloorCeilingTests.PitFindFloorCeiling_LowersCeilingFromOpening`
- `VmPMapFindFloorCeilingTests.PitFindFloorCeiling_RaisesFloorFromOpening`
- `VmPMapFindFloorCeilingTests.FindFloorCeiling_CopiesHeightsToActor`
- `VmPMapFindFloorCeilingTests.FindFloorCeiling_UpdatesWhenFloorPortalGroupDiffers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 786 --to 786` (0 hard / 0 soft).

## Seams

`FMultiBlockLinesIterator`, `P_LineOpening`, blockmap box tests, and `NextHighest/Lowest*` defaults are injected or simplified stubs.

## Credit rationale

Faithful floor/ceiling discovery orchestration for actors before teleport chunk787.
