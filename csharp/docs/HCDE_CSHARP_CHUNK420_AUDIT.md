# Chunk 420 — Sector link, texture/wall scroll, floor/ceiling scroll, damage/gravity/color/fade/panning

## Scope

Sector link, texture/wall scroll, floor/ceiling scroll, damage/gravity/color/fade/panning

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Sector_SetLink** 2269–2281 (13 lines)
- `src/playsim/p_lnspec.cpp` **LS_Scroll_*** 2286–2358 (73 lines)
- `src/playsim/p_lnspec.cpp` **LS_Sector_SetDamage** 2366–2402 (37 lines)
- `src/playsim/p_lnspec.cpp` **LS_Sector_SetGravity** 2404–2419 (16 lines)
- `src/playsim/p_lnspec.cpp` **LS_Sector_SetColor / Fade / Panning** 2421–2460 (40 lines)
- `src/playsim/p_lnspec.cpp` **LS_Sector_SetFloorPanning / SetFloorScale** 2462–2499 (38 lines)
- `src/playsim/p_lnspec.cpp` **LS_Sector_SetCeilingScale / Scale2** 2501–2566 (66 lines)

**Credited native LOC:** 283 (tally `nativeLoc` 283).

## Managed

`VmPLnspecScrollSectorLineSpecials.cs`, `VmPLnspecScrollSectorLineSpecialsTests.cs`
Managed LOC (tally): 174.
Conversion commit: `8c874d5aa5798ebc433b48dcd87aea85402fc209`.

## Tests

- `VmPLnspecScrollSectorLineSpecialsTests.SectorLink_SkipsControlTagZero`
- `VmPLnspecScrollSectorLineSpecialsTests.ScrollTextureBoth_NormalizesNegativeId`
- `VmPLnspecScrollSectorLineSpecialsTests.Damage_LegacyLeakinessForLowAmount`
- `VmPLnspecScrollSectorLineSpecialsTests.Gravity_ClampsFraction`
- `VmPLnspecScrollSectorLineSpecialsTests.Panning_CombinesIntAndFrac`
- `VmPLnspecScrollSectorLineSpecialsTests.FloorScale_InvertsNonZero`
- `VmPLnspecScrollSectorLineSpecialsTests.Scale2_Uses65536Factors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 420 --to 420` (0 hard / 0 soft).

## Seams

`P_AddSectorLinks`, `SetScroller`, and live `sector_t` mutation are not hosted.

## Credit rationale

Credits scroll id normalization, floor carry modes, legacy damage leakiness, gravity clamping, and panning offsets from `p_lnspec.cpp`.
