# Chunk 761 — `P_SetPsprite`, player `GetPSprite`, `DPSprite::GetRenderStyle`, `DPSprite::NewTick`, and DEFINE SetPSprite/GetPSprite (`p_pspr.cpp` 278–477; excludes `DPSprite::SetState` 485+ / chunk700).

## Scope

`P_SetPsprite`, player `GetPSprite`, `DPSprite::GetRenderStyle`, `DPSprite::NewTick`, and DEFINE SetPSprite/GetPSprite (`p_pspr.cpp` 278–477; excludes `DPSprite::SetState` 485+ / chunk700).

## Native ranges

- `src/playsim/p_pspr.cpp` **P_SetPsprite** 278–288 (11 lines)
- `src/playsim/p_pspr.cpp` **player_t::GetPSprite** 300–366 (67 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::GetRenderStyle** 377–450 (74 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::NewTick** 458–477 (20 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPPSpriteLayers.cs`
Managed LOC (tally): 309.
Conversion commit: `0abc8461127518d85d4252007dae5388ab92189c`.

## Padding review (2026-10-21)

**Trim (2026-10-22):** removed file/summary XML and unused `StateRef` type.

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `P_SetPsprite` | `P_SetPsprite` **278–288** |
| `SetPspriteAction` | `DEFINE_ACTION_FUNCTION(_PlayerInfo, SetPSprite)` **290–298** |
| `GetPsprite` | `player_t::GetPSprite` **300–366** |
| `GetPspriteAction` | `DEFINE_ACTION_FUNCTION(_PlayerInfo, GetPSprite)` **368–373** |
| `GetRenderStyle` / `PspriteRenderStyle.CheckFuzz` | `DPSprite::GetRenderStyle` **377–450** |
| `NewTick` | `DPSprite::NewTick` **458–477** |

### Out-of-body uncredit (761)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **187** |
| **Out-of-body removed** | **15** |
| **Honest** | **172** |


**Post-review ratio:** 309/172 managed/native ≈ 180% (src file LOC at HEAD; see tally).

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
