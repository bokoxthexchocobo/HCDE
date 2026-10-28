# Chunk 807 — Whole `P_ChangeSwitchTexture`, `DActiveButton` construct/tick/advance, and `FTextureAnimator::AddSwitchPair` (`p_switch.cpp` 243–310, 320–331, 380–452; `anim_switches.cpp` 305–330). Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Scope

Whole `P_ChangeSwitchTexture`, `DActiveButton` construct/tick/advance, and `FTextureAnimator::AddSwitchPair` (`p_switch.cpp` 243–310, 320–331, 380–452; `anim_switches.cpp` 305–330). Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5ab7067ac67e6903b35f49dd7d9498eb5f3933b7`.

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `ChangeSwitchTexture` | **243–310** |
| `ConstructButton` | **320–331** |
| `TickButton` | **380–419** |
| `AdvanceFrame` | **427–452** |
| `AddSwitchPair` | **305–330** |

### Out-of-body uncredit (807)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **172** |
| **Out-of-body removed** | **0** |
| **Honest** | **172** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 807 --to 807` (0 hard / 0 soft).

## Seams

`TexAnim::FindSwitch`, `P_StartButton`, `S_Sound`, and thinker destroy are injected.

## Credit rationale

Faithful switch texture animation band; retires remainder of `p-switch-textures-button-chunk5`.
