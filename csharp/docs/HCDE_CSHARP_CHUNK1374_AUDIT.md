# Chunk 1374 — Switch texture change, button construct, switch serialization, and the button tick through AdvanceFrame from p_switch.cpp (235-452).

## Scope

Switch texture change, button construct, switch serialization, and the button tick through AdvanceFrame from p_switch.cpp (235-452).

## Native ranges

- `src/playsim/p_switch.cpp` **ChangeSwitchTexture** 235–452 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmSwitchBand1374Slice.cs`
Managed LOC (tally): 325.
Conversion commit: `b153b5e77d1b5715b3e939cb1a9c74c6edeb766a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner before `P_ChangeSwitchTexture` (**235–242**) | yes |
| `P_ChangeSwitchTexture` (**243–310**) | yes |
| Button banner and `IMPLEMENT_CLASS` (**312–319**) | yes |
| `DActiveButton::Construct` (**320–331**) | yes |
| `FSwitchDef` serialize specialization (**339–353**) | yes |
| `DActiveButton::Serialize` (**361–372**) | yes |
| `DActiveButton::Tick` (**380–419**) | yes |
| `DActiveButton::AdvanceFrame` (**427–452**) | yes |
| Band hull (**235–452**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| p_switch.cpp **71–88** and **98–234** | `P_StartButton` and `P_CheckSwitchRange`, chunk 804 |
| p_switch.cpp **39** | `pr_switchanim` declaration; the call at 448 is injected |
| anim_switches.cpp | not this hole |

`FindSwitch` tries the top, mid, and bottom textures and stops at the first hit. A miss with a quest pointer stores false. `NO_SOUND` is 0, so a defined sound is kept. Otherwise exit specials 243, 244, 74, and 75 resolve `switches/exitbutn` and every other special resolves `switches/normbutn`. The sound point is `v1 + delta / 2` with z 0. `SetTexture` runs before the button. `useAgain` nonzero or more than one frame calls `P_StartButton` with `!!useAgain`. A false return skips `S_Sound`. The played sound uses channel 2, flag 8, volume 1, and attenuation 3. `IMPLEMENT_CLASS(DActiveButton, false, false)` is a concrete class with no pointer offsets, and the in-place constructor allocates the button. `Construct` stores `int8_t(Where)`, clears returning, sets the frame to -1, and calls `AdvanceFrame`. The switch serializer writes `PreTexture` and, on read, starts from invalid texture -1 before `FindSwitch`. Button keys are `side`, `part`, `switchdef`, `frame`, `timer`, `fippable`, `pos`, and `returning`, after `Super::Serialize`. `Tick` destroys a null definition without changing the timer. The timer is `uint32_t`, so 0 wraps and does not fire. On the last frame a null pair destroys before `AdvanceFrame`. A pair plays its sound, or `switches/normbutn` when that sound is 0, then clears flippable. `AdvanceFrame` sets the timer to `BUTTONTIME` (35) on a flippable last frame. Any other frame stores `TimeMin` and adds `pr_switchanim(TimeRnd)` only when `TimeRnd` is not 0.

| Managed (`VmSwitchBand1374Slice.cs`) | Native (`p_switch.cpp`) |
|--------------------------------------|-------------------------|
| Texture change through `AdvanceFrame` | **235–452** |
| Band hull (exclusive slice) | **235–452** |

### Chunk **1374** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **218** |
| **Net headline** | **+218** vs the prior chunk |


**Post-review ratio:** 325/218 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmSwitchBand1374SliceTests.ChangeSwitchTextureSelectsPartSoundAndButton`
- `VmSwitchBand1374SliceTests.ButtonTickSerializeAndAdvance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1374 --to 1374` (0 hard / 0 soft).

## Seams

P_StartButton stays chunk 804. TexAnim.FindSwitch, S_FindSound, S_Sound, pr_switchanim, and DThinker::Serialize are callees. The FTextureID read writes through the serializer.

## Credit rationale

UNCredited p_switch.cpp 235-452 (+218). A full tally line scan, including recognition ranges, shows this is the unique largest hole of at most 400 lines. CVarScopeStateNode.cpp 1-217 and d_net.cpp 3685-3901 are 217. Chunk 804 already owns 71-88 and 98-234. The file ends at 452. Gaps longer than 400 lines stay whole and are not sliced.
