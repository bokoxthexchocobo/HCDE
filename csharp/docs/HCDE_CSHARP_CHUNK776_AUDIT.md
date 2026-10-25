# Chunk 776 — Special-sector lookup, danger heuristics, and surrounding height queries (`p_sectors.cpp` 65–447; excludes `CUSTOM_CVAR` 53–57).

## Scope

Special-sector lookup, danger heuristics, and surrounding height queries (`p_sectors.cpp` 65–447; excludes `CUSTOM_CVAR` 53–57).

## Native ranges

- `src/playsim/p_sectors.cpp` **P_NextSpecialSectorVC** 65–78 (14 lines)
- `src/playsim/p_sectors.cpp` **P_NextSpecialSector** 81–94 (14 lines)
- `src/playsim/p_sectors.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(_Sector, NextSpecialSector)** 96–102 (7 lines)
- `src/playsim/p_sectors.cpp` **IsDamaging** 104–128 (25 lines)
- `src/playsim/p_sectors.cpp` **IsCrushing** 130–138 (9 lines)
- `src/playsim/p_sectors.cpp` **sector_t::IsDangerous** 140–162 (23 lines)
- `src/playsim/p_sectors.cpp` **FindLowestFloorSurrounding** 168–201 (34 lines)
- `src/playsim/p_sectors.cpp` **FindHighestFloorSurrounding** 207–240 (34 lines)
- `src/playsim/p_sectors.cpp` **FindNextHighestFloor** 252–291 (40 lines)
- `src/playsim/p_sectors.cpp` **FindNextLowestFloor** 304–343 (40 lines)
- `src/playsim/p_sectors.cpp` **FindNextLowestCeiling** 356–395 (40 lines)
- `src/playsim/p_sectors.cpp` **FindNextHighestCeiling** 408–447 (40 lines)

**Credited native LOC:** 320 (tally `nativeLoc` 320).

## Managed

`VmPSectorNextSpecial.cs`, `VmPSectorSurroundHeights.cs`
Managed LOC (tally): 463.
Conversion commit: `97cc0b5c8ddffe3a96308f118fa908c592758752`.

## Padding review (2026-10-21)

**Trim (2026-10-22):** dropped file/summary XML on both VM files.

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `NextSpecialSectorVc` | `P_NextSpecialSectorVC` **65–78** |
| `NextSpecialSector` | `P_NextSpecialSector` **81–94** |
| `NextSpecialSectorAction` | DEFINE **96–102** |
| `IsDamaging` | **104–128** |
| `IsCrushing` | **130–138** |
| `IsDangerous` | `sector_t::IsDangerous` **140–162** |
| `FindLowestFloorSurrounding` | **168–201** |
| `FindHighestFloorSurrounding` | **207–240** |
| `FindNextHighestFloor` | **252–291** |
| `FindNextLowestFloor` | **304–343** |
| `FindNextLowestCeiling` | **356–395** |
| `FindNextHighestCeiling` | **408–447** |

### Out-of-body uncredit (776)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **320** |
| **Out-of-body removed** | **0** |
| **Honest** | **320** |


**Post-review ratio:** 463/320 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorNextSpecialTests.NextSpecialSectorVc_SkipsMatchingValidCount`
- `VmPSectorNextSpecialTests.NextSpecialSector_RespectsNogood`
- `VmPSectorNextSpecialTests.NextSpecialSectorAction_MatchesNextSpecialSector`
- `VmPSectorNextSpecialTests.IsDamaging_DetectsDamageAmountAndRecentManualDamage`
- `VmPSectorNextSpecialTests.IsDamaging_SectorAction214RequiresTag`
- `VmPSectorNextSpecialTests.IsCrushing_UsesCrushDelegates`
- `VmPSectorNextSpecialTests.IsDangerous_Checks3dFloors`
- `VmPSectorSurroundHeightsTests.FindLowestFloorSurrounding_PrefersLowerNeighbor`
- `VmPSectorSurroundHeightsTests.FindLowestFloorSurrounding_WritesSpotVertex`
- `VmPSectorSurroundHeightsTests.FindHighestFloorSurrounding_PrefersHigherNeighbor`
- `VmPSectorSurroundHeightsTests.FindNextHighestFloor_SkipsLinkedNeighbors`
- `VmPSectorSurroundHeightsTests.FindNextLowestFloor_PicksClosestDrop`
- `VmPSectorSurroundHeightsTests.FindNextLowestCeiling_AndHighestCeiling`
- `VmPSectorSurroundHeightsTests.EmptyLines_ReturnsPlaneTexZ`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 776 --to 776` (0 hard / 0 soft).

## Seams

`getNextSector`, `dyn_cast` crush readers, `SectorHasTag`, `GameTicRate`, and `validcount` are injected via delegates or parameters; 3D-floor rovers use typed plane seams.

## Credit rationale

Faithful whole-function ports for the early `p_sectors.cpp` sector query band before ceiling-surround helpers at 453+.
