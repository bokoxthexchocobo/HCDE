# Chunk 699 — Whole `P_Recalculate3DFloors` (`p_3dfloors.cpp` 291–535).

## Scope

Whole `P_Recalculate3DFloors` (`p_3dfloors.cpp` 291–535).

## Native ranges

- `src/playsim/p_3dfloors.cpp` **P_Recalculate3DFloors** 291–535 (245 lines)

**Credited native LOC:** 245 (tally `nativeLoc` 245).

## Managed

`VmP3dfloorsRecalculate.cs`
Managed LOC (tally): 316.
Conversion commit: `c33ca7a87435954a7119bb23af1d03609fcb4949`.

## Padding review (2026-10-21)

| Managed | Native (`p_3dfloors.cpp`) |
|---------|-------------------------|
| `VmP3dfloorsRecalculate` | `P_Recalculate3DFloors` 291–535 |


**Post-review ratio:** 316/245 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmP3dfloorsRecalculateTests.Recalculate_BuildsCeilingLightHead`
- `VmP3dfloorsRecalculateTests.Recalculate_SkipsNoshadeTier`
- `VmP3dfloorsRecalculateTests.Recalculate_ClipsTranslucentUnderSolid`
- `VmP3dfloorsRecalculateTests.Recalculate_ThisInsideBypassesClipSort`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 699 --to 699` (0 hard / 0 soft).

## Seams

Sector thing lists, pit iterators, and `UpdateWaterLevel` remain native; dynamic `F3DFloor` allocation uses injected `CloneFloor`.

## Credit rationale

3D-floor sort/clip and sector lightlist rebuild after trace/setup chunks; completes the recalc hook used when ffloors change.
