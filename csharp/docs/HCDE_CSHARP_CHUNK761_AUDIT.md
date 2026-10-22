# Chunk 761 — `P_SetPsprite`, player `GetPSprite`, `DPSprite::GetRenderStyle`, `DPSprite::NewTick`, and DEFINE SetPSprite/GetPSprite (`p_pspr.cpp` 278–477; excludes `DPSprite::SetState` 485+ / chunk700).

## Scope

`P_SetPsprite`, player `GetPSprite`, `DPSprite::GetRenderStyle`, `DPSprite::NewTick`, and DEFINE SetPSprite/GetPSprite (`p_pspr.cpp` 278–477; excludes `DPSprite::SetState` 485+ / chunk700).

## Native ranges

- `src/playsim/p_pspr.cpp` **P_SetPsprite** 278–288 (11 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetPSprite)** 290–298 (9 lines)
- `src/playsim/p_pspr.cpp` **player_t::GetPSprite** 300–366 (67 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, GetPSprite)** 368–373 (6 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::GetRenderStyle** 377–450 (74 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::NewTick** 458–477 (20 lines)

**Credited native LOC:** 187 (tally `nativeLoc` 187).

## Managed

`VmPPSpriteLayers.cs`
Managed LOC (tally): 355.
Conversion commit: `7c13df40da3bb7488e18c6d4293b5369c16eb440`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `P_SetPsprite` | `P_SetPsprite` **278–288** |
| `SetPspriteAction` | `DEFINE_ACTION_FUNCTION(_PlayerInfo, SetPSprite)` **290–298** |
| `GetPsprite` | `player_t::GetPSprite` **300–366** |
| `GetPspriteAction` | `DEFINE_ACTION_FUNCTION(_PlayerInfo, GetPSprite)` **368–373** |
| `GetRenderStyle` / `PspriteRenderStyle.CheckFuzz` | `DPSprite::GetRenderStyle` **377–450** |
| `NewTick` | `DPSprite::NewTick` **458–477** |


**Post-review ratio:** 355/187 managed/native ≈ 190% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSpriteLayersTests.P_SetPsprite_SkipsNullPlayer`
- `VmPPSpriteLayersTests.GetPsprite_ReturnsNullForCallerIdLayer`
- `VmPPSpriteLayersTests.GetPsprite_ResetsTargeterFlagsOnCallerChange`
- `VmPPSpriteLayersTests.GetPsprite_StrifeHandsClearsStateAndSetsY`
- `VmPPSpriteLayersTests.GetRenderStyle_ForceAlphaOverridesOwner`
- `VmPPSpriteLayersTests.GetRenderStyle_NormalWithLowAlphaBecomesTranslucent`
- `VmPPSpriteLayersTests.NewTick_SetsProcessPendingOnAllPsprites`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 761 --to 761` (0 hard / 0 soft).

## Seams

`P_CreatePsprite`, inventory lookup, weapon float vars, and `DPSprite::SetState` delegate to `VmPPSpriteSetState`.

## Credit rationale

Faithful VM ports for psprite layer acquisition, render-style blending, and per-tick pending reset.
