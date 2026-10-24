# Chunk 785 — 3D-floor colormap/blend, `P_Add3DFloor`, special-floor tests, and solid hit checks (`p_3dfloors.cpp` 48–280; before chunk699 at 291).

## Scope

3D-floor colormap/blend, `P_Add3DFloor`, special-floor tests, and solid hit checks (`p_3dfloors.cpp` 48–280; before chunk699 at 291).

## Native ranges

- `src/playsim/p_3dfloors.cpp` **F3DFloor::GetColormap** 48–60 (13 lines)
- `src/playsim/p_3dfloors.cpp` **F3DFloor::GetBlend** 62–73 (12 lines)
- `src/playsim/p_3dfloors.cpp` **F3DFloor::UpdateColormap** 75–87 (13 lines)
- `src/playsim/p_3dfloors.cpp` **P_Add3DFloor** 94–183 (90 lines)
- `src/playsim/p_3dfloors.cpp` **P_ActorOnSpecial3DFloor** 193–224 (32 lines)
- `src/playsim/p_3dfloors.cpp` **P_CheckFor3DFloorHit** 232–252 (21 lines)
- `src/playsim/p_3dfloors.cpp` **P_CheckFor3DCeilingHit** 260–280 (21 lines)

**Credited native LOC:** 202 (tally `nativeLoc` 202).

## Managed

`VmP3DFloorColormapAdd.cs`
Managed LOC (tally): 378.
Conversion commit: `e5c4996b85ac5aceb35d803fee692b39c1bbae5b`.

## Padding review (2026-10-21)

| Managed | Native (`p_3dfloors.cpp`) |
|---------|-------------------------|
| `GetColormap` | `F3DFloor::GetColormap` **48–60** |
| `GetBlend` | `F3DFloor::GetBlend` **62–73** |
| `UpdateColormap` | **75–87** |
| `Add3DFloor` | `P_Add3DFloor` **94–183** |
| `ActorOnSpecial3DFloor` | **193–224** |
| `CheckFor3DFloorHit` | **232–252** |
| `CheckFor3DCeilingHit` | **260–280** |


**Post-review ratio:** 378/202 managed/native ≈ 187% (src file LOC at HEAD; see tally).

## Tests

- `VmP3DFloorColormapAddTests.GetColormap_MergesTargetFadeWhenNeeded`
- `VmP3DFloorColormapAddTests.GetBlend_ReturnsModelFadeWhenNotFadeWalls`
- `VmP3DFloorColormapAddTests.Add3DFloor_PushesFloorOntoTargetSector`
- `VmP3DFloorColormapAddTests.Add3DFloor_BothPlanesAddsInsideCopy`
- `VmP3DFloorColormapAddTests.Add3DFloor_SkipsWhenAlreadyAttached`
- `VmP3DFloorColormapAddTests.CheckFor3DFloorHit_DetectsSolidTop`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 785 --to 785` (0 hard / 0 soft).

## Seams

`P_ActorInSpecialSector`, `P_ActorOnSpecialFlat`, and `checkForSpecialSector` remain injected host hooks.

## Credit rationale

Faithful ports for colormap and add/hit band preceding recalc chunk699 and support chunk781.
