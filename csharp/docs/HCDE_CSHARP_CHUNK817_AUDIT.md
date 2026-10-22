# Chunk 817 — `DSectorScrollInterpolation`, `DWallScrollInterpolation`, `DPolyobjInterpolation`, and `SetInterpolation` hooks from `r_interpolate.cpp` (547–970).

## Scope

`DSectorScrollInterpolation`, `DWallScrollInterpolation`, `DPolyobjInterpolation`, and `SetInterpolation` hooks from `r_interpolate.cpp` (547–970).

## Native ranges

- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::DSectorScrollInterpolation** 547–554 (8 lines)
- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::UnlinkFromMap** 562–577 (16 lines)
- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::UpdateInterpolation** 585–589 (5 lines)
- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::Restore** 597–601 (5 lines)
- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::Interpolate** 609–624 (16 lines)
- `src/r_data/r_interpolate.cpp` **DSectorScrollInterpolation::Serialize** 632–639 (8 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::DWallScrollInterpolation** 654–661 (8 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::UnlinkFromMap** 669–677 (9 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::UpdateInterpolation** 685–689 (5 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::Restore** 697–701 (5 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::Interpolate** 709–724 (16 lines)
- `src/r_data/r_interpolate.cpp` **DWallScrollInterpolation::Serialize** 732–739 (8 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::DPolyobjInterpolation** 753–761 (9 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::UnlinkFromMap** 769–776 (8 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::UpdateInterpolation** 784–793 (10 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::Restore** 801–810 (10 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::Interpolate** 818–848 (31 lines)
- `src/r_data/r_interpolate.cpp` **DPolyobjInterpolation::Serialize** 856–864 (9 lines)
- `src/r_data/r_interpolate.cpp` **side_t::SetInterpolation** 873–882 (10 lines)
- `src/r_data/r_interpolate.cpp` **side_t::StopInterpolation** 890–896 (7 lines)
- `src/r_data/r_interpolate.cpp` **sector_t::SetInterpolation** 904–935 (32 lines)
- `src/r_data/r_interpolate.cpp` **FPolyObj::SetInterpolation** 943–956 (14 lines)
- `src/r_data/r_interpolate.cpp` **FPolyObj::StopInterpolation** 964–970 (7 lines)

**Credited native LOC:** 256 (tally `nativeLoc` 256).

## Managed

`VmRInterpolatorScrollPoly.cs`
Managed LOC (tally): 267.
Conversion commit: `32e59ba0224226417c230643cd5eafbfbae1c5f1`.

## Padding review (2026-10-21)

| Managed | Native (`r_interpolate.cpp`) |
|---------|-------------------------------|
| `ScrollInterp2D` | `DSectorScrollInterpolation` / `DWallScrollInterpolation` **547–724** |
| `PolyobjInterp` | `DPolyobjInterpolation` **753–848** |
| `GetOrCreate*` / `Stop*` | `side_t` / `sector_t` / `FPolyObj` **873–970** |

**Post-review ratio:** 267/256 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmRInterpolatorScrollPolyTests.ScrollInterp_LerpsOffsets`
- `VmRInterpolatorScrollPolyTests.ScrollInterp_UnlinksWhenUnchanged`
- `VmRInterpolatorScrollPolyTests.ScrollSerialize_CapturesOffsets`
- `VmRInterpolatorScrollPolyTests.PolyInterp_DetectsVertexChange`
- `VmRInterpolatorScrollPolyTests.GetOrCreateSideInterpolation_CreatesOnce`
- `VmRInterpolatorScrollPolyTests.MapSectorInterpolationPosition_KnownSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 817 --to 817` (0 hard / 0 soft).

## Seams

Live `sector_t` / `side_t` / `FPolyObj` slots and `Create<>` factories are injected; plane-move types from band 1 are not re-hosted here.

## Credit rationale

Second tally credit for texture scroll and polyobj vertex interpolation plus refcounted map hooks. Scroll/wall/poly Serialize kept here to avoid sub-150 orphans after interpolate bodies.
