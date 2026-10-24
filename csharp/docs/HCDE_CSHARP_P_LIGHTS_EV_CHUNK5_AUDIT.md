# p_lights EV specials and neighbor lighting — chunk 5 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `EV_LightChange` | 724–738 | `LightActions.LightChange` |
| `EV_LightTurnOnPartway` | 675–717 | `LightActions.LightTurnOnPartway` |
| `EV_TurnTagLightsOff` | 606–625 | `LightActions.TurnTagLightsOff` |
| `EV_StartLightFlickering` | 552–560 | `LightActions.Flicker` (existing) |
| `EV_StartLightStrobing` | 570–596 | `LightActions.Animate` / `DoomStrobe` |
| `EV_StartLightGlowing` / `EV_StartLightFading` | 740–797 | `LightActions.Animate` |
| `EV_StopLightEffect` | 808–819 | `LightActions.Stop` |
| Thinkers `DFlicker`…`DPhased` | 120–540 | `LightEffect` tick + `LightActions.Initialize` |

**Behavior native LOC credited (conversion):** 68 (`EV_LightChange`, `EV_LightTurnOnPartway`, `EV_TurnTagLightsOff` only).

**Recognition:** DFlicker/DStrobe/DGlow thinkers and `EV_StartLightFading` / flicker / strobe / glow / stop paths were already covered by `LightEffect` + `LightActions` before this run; those ranges are booked in `p-lights-thinkers-chunk5-recognition` (752 native LOC), not conversion.

## Tests

`LightEvParityTests.*` (conversion). Thinker recognition ranges cite `FlickerTests`, `AnimatedLightTests`, `LightActionsTests`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
