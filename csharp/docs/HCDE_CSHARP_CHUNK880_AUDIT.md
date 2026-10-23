# Chunk 880 — Whole TU `p_linkedsectors.cpp` (landed as chunk **880**; supersedes the partial chunk **174** row by name).

## Scope

Whole TU `p_linkedsectors.cpp` (landed as chunk **880**; supersedes the partial chunk **174** row by name).

## Native ranges

- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors headers** 1–30 (30 lines)
- `src/playsim/p_linkedsectors.cpp` **LINK_* enums** 31–50 (20 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 51–66 (16 lines)
- `src/playsim/p_linkedsectors.cpp` **sector_t::IsLinked** 67–77 (11 lines)
- `src/playsim/p_linkedsectors.cpp` **DEFINE_ACTION_FUNCTION isLinked** 79–85 (7 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 86–92 (7 lines)
- `src/playsim/p_linkedsectors.cpp` **MoveCeiling** 93–106 (14 lines)
- `src/playsim/p_linkedsectors.cpp` **MoveFloor** 108–121 (14 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 122–132 (11 lines)
- `src/playsim/p_linkedsectors.cpp` **P_MoveLinkedSectors** 133–200 (68 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 201–210 (10 lines)
- `src/playsim/p_linkedsectors.cpp` **P_StartLinkedSectorInterpolations** 211–226 (16 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 227–236 (10 lines)
- `src/playsim/p_linkedsectors.cpp` **AddSingleSector** 237–265 (29 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 266–274 (9 lines)
- `src/playsim/p_linkedsectors.cpp` **RemoveTaggedSectors** 275–285 (11 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 286–296 (11 lines)
- `src/playsim/p_linkedsectors.cpp` **P_AddSectorLinks** 297–337 (41 lines)
- `src/playsim/p_linkedsectors.cpp` **p_linkedsectors separators** 338–348 (11 lines)
- `src/playsim/p_linkedsectors.cpp` **P_AddSectorLinksByID** 349–384 (36 lines)

**Credited native LOC:** 382 (tally `nativeLoc` 382).

## Managed

`VmLinkedSectorTypes.cs`, `VmLinkedSectorMovement.cs`, `VmLinkedSectorPlaneMove.cs`, `VmLinkedSectorInterpolation.cs`, `VmLinkedSectorAttach.cs`
Managed LOC (tally): 266.
Conversion commit: `49f1fb7833839f58783d43114d16ba9629850852`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `LINK_*` enums | **yes** (`VmLinkedSectorLinkType`) |
| `sector_t::IsLinked` | **yes** |
| `DEFINE_ACTION_FUNCTION(_Sector, isLinked)` | **no** (VM export only) |
| `MoveCeiling` / `MoveFloor` | **yes** (plane height + tex Z + in-sector solid block + floor/ceiling crossing guard) |
| `P_MoveLinkedSectors` | **yes** (calls plane movers on sim) |
| `P_StartLinkedSectorInterpolations` | **yes** (marker list) |
| `AddSingleSector` / `RemoveTaggedSectors` | **yes** |
| `P_AddSectorLinks` | **yes** |
| `P_AddSectorLinksByID` | **yes** (`Static_Init` / `Init_SectorLink` lines) |

| Managed | Native (`p_linkedsectors.cpp`) |
|---------|------------------------------|
| `VmLinkedSectorLinkType` | **31–50** |
| `IsLinked` | **67–77** |
| (VM `isLinked`) | **79–85** |
| `MoveCeiling` | **93–106** |
| `MoveFloor` | **108–121** |
| `MoveLinkedSectors` | **133–200** |
| `StartLinkedSectorInterpolations` | **211–226** |
| `AddSingleSector` | **237–265** |
| `RemoveTaggedSectors` | **275–285** |
| `TryAddSectorLinks` | **297–337** |
| `AddSectorLinksById` | **349–384** |
| (headers / separators) | **1–30**, **51–66**, **86–92**, **122–132**, **201–210**, **227–236**, **266–274**, **286–296**, **338–348** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **382** |
| Net behavior-ported | **375** |

(VM **79–85** gross only; net excludes **7** lines.)

**Post-review ratio:** 266/382 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmLinkedSectorTests.IsLinked_MatchesFloorOrCeilingFlag`
- `VmLinkedSectorTests.MoveLinkedSectors_AppliesMirrorAndBothOrdering`
- `VmLinkedSectorTests.MoveFloor_BlocksWhenSolidActorAndNoCrush`
- `VmLinkedSectorTests.MoveFloor_RejectsFloorAboveCeiling`
- `VmLinkedSectorTests.MoveLinkedSectors_RunsAllLinksEvenWhenOneFails`
- `VmLinkedSectorTests.StartLinkedSectorInterpolations_RegistersFloorAndCeilingPlanes`
- `VmLinkedSectorTests.AddSectorLinks_MergesDuplicateSectorTypes`
- `VmLinkedSectorTests.TryAddSectorLinks_RemovesTaggedSectorsWhenParamZero`
- `VmLinkedSectorTests.TryAddSectorLinks_SkipsSelfPlaneConflict`
- `VmLinkedSectorTests.AddSectorLinksById_AttachesFromStaticInitLines`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 880 --to 880` (0 hard / 0 soft).

## Seams

Direct `AuthoritySimulation` floor/ceiling arrays and `LevelSector` texture offsets for plane moves. `LinkedSectorChangeSector` implements crush/no-crush blocking for solid actors in-sector; full `P_ChangeSector` iterator/crush damage remains on `p_map.cpp` chunks. `PortalIsLinked` / sloped planes return false until sector portal/slope state is wired on the sim.

## Credit rationale

Completes the only uncredited playsim TU after **879**; chunk **174** credited attach/move dispatch without `MoveFloor`/`MoveCeiling`, interpolation registration, tag removal, or Eternity line-id attach.
