# p_switch textures, button thinker, and sounds — chunk 5 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `p_switch.cpp` (full file) | 452 | `SwitchActions`, `ActiveButton`, `SwitchCatalog` |
| `anim_switches.cpp` (full file) | 387 | Builtin `SW1*`/`SW2*` pairs, `RegisterPair` |
| `p_spec.cpp` `P_ActivateLine` switch hook | 155–184 | `LineSpecials.ActivateMapLine` + `NotifySwitch` |

**Behavior native LOC credited:** 839 (`p_switch.cpp` + `anim_switches.cpp`).

## Tests

`SwitchTextureTests.UseLineChangesSwitchTextureAndPlaysSound`, `SwitchTextureTests.ButtonReturnsToOffTextureAfterHoldTime`, `SwitchTextureTests.RePressingActiveButtonAdvancesTimer`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17032 tests).
