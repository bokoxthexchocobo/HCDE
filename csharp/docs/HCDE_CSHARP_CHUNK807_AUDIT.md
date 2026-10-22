# Chunk 807 — Whole `P_ChangeSwitchTexture`, `DActiveButton` construct/tick/advance, and `FTextureAnimator::AddSwitchPair` (`p_switch.cpp` 243–310, 320–331, 380–452; `anim_switches.cpp` 305–330).

## Scope

Whole `P_ChangeSwitchTexture`, `DActiveButton` construct/tick/advance, and `FTextureAnimator::AddSwitchPair` (`p_switch.cpp` 243–310, 320–331, 380–452; `anim_switches.cpp` 305–330).

## Native ranges

- `src/playsim/p_switch.cpp` **P_ChangeSwitchTexture** 243–310 (68 lines)
- `src/playsim/p_switch.cpp` **DActiveButton::Construct** 320–331 (12 lines)
- `src/playsim/p_switch.cpp` **DActiveButton::Tick** 380–419 (40 lines)
- `src/playsim/p_switch.cpp` **DActiveButton::AdvanceFrame** 427–452 (26 lines)
- `src/gamedata/textures/anim_switches.cpp` **FTextureAnimator::AddSwitchPair** 305–330 (26 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPSwitchChangeTexture.cs`
Managed LOC (tally): 161.
Conversion commit: `65d34e8f1e75bc99502c73e46e1e0f9de55bc214`.

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `ChangeSwitchTexture` | **243–310** |
| `ConstructButton` | **320–331** |
| `TickButton` | **380–419** |
| `AdvanceFrame` | **427–452** |
| `AddSwitchPair` | **305–330** |

**Post-review ratio:** 161/172 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPSwitchChangeTextureTests.ChangeSwitchTexture_FalseWhenNoSwitch`
- `SwitchTextureTests.UseLineChangesSwitchTextureAndPlaysSound`
- `VmPSwitchChangeTextureTests.AdvanceFrame_SetsTimerFromFrame`
- `SwitchTextureTests.ButtonReturnsToOffTextureAfterHoldTime`
- `VmPSwitchChangeTextureTests.AddSwitchPair_LinksDefs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 807 --to 807` (0 hard / 0 soft).

## Seams

`TexAnim::FindSwitch`, `P_StartButton`, `S_Sound`, and thinker destroy are injected.

## Credit rationale

Faithful switch texture animation band; retires remainder of `p-switch-textures-button-chunk5`.
