# Chunk 821 — `DSectorEffect` / `DMover` lifecycle and `sector_t::MoveAttached`, `MoveFloor`, `MoveCeiling` from `dsectoreffect.cpp` (33–424).

## Scope

`DSectorEffect` / `DMover` lifecycle and `sector_t::MoveAttached`, `MoveFloor`, `MoveCeiling` from `dsectoreffect.cpp` (33–424).

## Native ranges

- `src/playsim/mapthinkers/dsectoreffect.cpp` **DSectorEffect::OnDestroy** 33–51 (19 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DSectorEffect::Construct** 53–56 (4 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DSectorEffect::Serialize** 58–62 (5 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMover::Construct** 78–82 (5 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMover::OnDestroy** 84–88 (5 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMover::Serialize** 90–94 (5 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMover::StopInterpolation** 96–103 (8 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMovingFloor::Construct** 108–113 (6 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **DMovingCeiling::Construct** 118–123 (6 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **sector_t::MoveAttached** 125–151 (27 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **sector_t::MoveFloor** 159–288 (130 lines)
- `src/playsim/mapthinkers/dsectoreffect.cpp` **sector_t::MoveCeiling** 303–424 (122 lines)

**Credited native LOC:** 342 (tally `nativeLoc` 342).

## Managed

`VmDSectorEffect.cs`
Managed LOC (tally): 352.
Conversion commit: `4e0706b9182000bfbf25b98573edc4a12ea23242`.

## Padding review (2026-10-21)

| Managed | Native (`dsectoreffect.cpp`) |
|---------|-------------------------------|
| `SectorEffectOnDestroy` / `Construct` | **33–62** |
| `MoverConstruct` / `StopInterpolation` | **78–103** |
| `MovingFloor` / `MovingCeiling` `Construct` | **108–123** |
| `MoveAttached` | **125–151** |
| `MoveFloor` | **159–288** |
| `MoveCeiling` | **303–424** |

### Out-of-body uncredit (821)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **342** |
| **Out-of-body removed** | **0** |
| **Honest** | **342** |


**Post-review ratio:** 352/342 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmDSectorEffectTests.SectorEffectOnDestroy_ClearsSectorPointers`
- `VmDSectorEffectTests.MoveAttached_RollsBackOnLinkedSectorFailure`
- `VmDSectorEffectTests.MoveFloor_DownDestinationClamp_PastDestAfterChangeSectorRollback`
- `VmDSectorEffectTests.MoveFloor_UpDestinationClamp_PastDestAfterStopMovingRollback`
- `VmDSectorEffectTests.MoveFloor_IncrementalDown_ObstructionReturnsCrushed`
- `VmDSectorEffectTests.MoveFloor_DownDestinationClamp_SetsFloorToDestAndPastDest`
- `VmDSectorEffectTests.MoveCeiling_CloseDestinationClamp_PastDestAfterChangeSectorRollback`
- `VmDSectorEffectTests.MoveCeiling_UpDestinationClamp_SetsCeilingToDestAndPastDest`
- `VmDSectorEffectTests.MoveCeiling_IncrementalDown_ObstructionReturnsCrushed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 821 --to 821` (0 hard / 0 soft).

## Seams

`P_Scroll3dMidtex`, `P_MoveLinkedSectors`, `P_ChangeSector`, plane types, and interpolation objects are injected.

## Credit rationale

First tally credit for sector-effect thinker teardown and plane move/crush helpers shared by floors and ceilings.
