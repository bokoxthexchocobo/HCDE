# Chunk 421 — Sector rotation, line align, texture offset/scale, line blocking flags

## Scope

Sector rotation, line align, texture offset/scale, line blocking flags

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Sector_SetRotation** 2568–2582 (15 lines)
- `src/playsim/p_lnspec.cpp` **LS_Line_AlignCeiling / AlignFloor** 2584–2610 (27 lines)
- `src/playsim/p_lnspec.cpp` **LS_Line_SetTextureOffset** 2612–2665 (54 lines)
- `src/playsim/p_lnspec.cpp` **LS_Line_SetTextureScale** 2667–2719 (53 lines)
- `src/playsim/p_lnspec.cpp` **LS_Line_SetBlocking** 2721–2765 (45 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmPLnspecLineCameraLineSpecials.cs`, `VmPLnspecLineCameraTests.cs`, `VmPLnspecLineTextureLineSpecials.cs`, `VmPLnspecLineTextureTests.cs`, `VmPLnspecSetPlayerProperty.cs`, `VmPLnspecSetPlayerPropertyTests.cs`
Managed LOC (tally): 257.
Conversion commit: `3d694e24ce6f34e385d8e45b2ea4aac51bfe2c27`.

## Tests

- `VmPLnspecLineTextureTests.SectorRotation_PacksFloorAndCeilingAngles`
- `VmPLnspecLineTextureTests.LineAlign_CeilingUsesTopPlane`
- `VmPLnspecLineTextureTests.TextureOffset_RejectsInvalidSide`
- `VmPLnspecLineTextureTests.TextureScale_Uses65536Factors`
- `VmPLnspecLineTextureTests.Blocking_RejectsZeroLineId`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 421 --to 421` (0 hard / 0 soft).

## Seams

`AlignFlat`, `side_t` mutation, and live line iterators are not hosted.

## Credit rationale

Credits angle assignment, align side selection, 65536 texture deltas, and `flagtrans` set/clear wiring from `p_lnspec.cpp`.
