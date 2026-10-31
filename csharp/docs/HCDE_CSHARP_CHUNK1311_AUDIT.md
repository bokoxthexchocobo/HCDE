# Chunk 1311 — Joystick config actions and the options-menu rebuild from joystickmenu.cpp (1-281).

## Scope

Joystick config actions and the options-menu rebuild from joystickmenu.cpp (1-281).

## Native ranges

- `src/common/menu/joystickmenu.cpp` **UpdateJoystickMenu** 1–281 (281 lines)

**Credited native LOC:** 281 (tally `nativeLoc` 281).

## Managed

`VmJoystickMenu1311Slice.cs`
Managed LOC (tally): 399.
Conversion commit: `b333739856881e75417c6e63324fe11b28e2e591`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Config actions (**31–205**) | yes |
| Options rebuild and close (**207–281**) | yes |
| Band hull (**1–281**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Device enumeration | host returns the joystick objects |
| Menu item Init | host builds the joy-config row |

A null self aborts with the parameter name before the getter or setter runs. Getters still evaluate when numret is not positive, then return 0. Setters return 0. Float arguments narrow to float; the curve-point value narrows through the float parameter. The curve integer is stored as passed, including -1 and values above the preset count. Reset runs default config first, then enables the device, copies AllowsEnabledInBackground, and sets sensitivity to 1. Update returns when the defaults key is missing, when the options key is missing, when the options descriptor is not an option menu, or when the defaults object is null. A present options key whose descriptor is null is dereferenced. Names compare case-insensitively. The device list is cleared only when the rebuild runs, and an early return keeps the previous list. The initial selection index is the last device because the starting -1 fails the unsigned size test; a size of 0 stores -1. A selected device that is actually in the list replaces that index, and the first match wins. Items are the default rows by pointer, then one new row per device. The matching row stores the list size after that push. A value that is still at least the final size clamps to the last index, so the last device lands on its index and an earlier device keeps the size from its own push. ConfigureMessage is 1 when any device exists. Both connect rows are 1 when use_joystick is false. GetItem sets only the first matching action. An open JoystickConfigMenu closes when its mJoy pointer is non-null and absent from the new list. A null pointer stays open. A missing mJoy field aborts after the rows are built. CalcIndent stays commented out.

| Managed (`VmJoystickMenu1311Slice.cs`) | Native (`joystickmenu.cpp`) |
|----------------------------------------|-----------------------------|
| Actions, rebuild, and close | **1–281** |
| Band hull (exclusive slice) | **1–281** |

### Chunk **1311** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **281** |
| **Net headline** | **+281** vs post-**1310** |


**Post-review ratio:** 399/281 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmJoystickMenu1311SliceTests.Actions_NarrowFloatsAndResetRunsTheHeaderSequence`
- `VmJoystickMenu1311SliceTests.Update_RebuildsFromDefaultsAndKeepsTheDeviceList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1311 --to 1311` (0 hard / 0 soft).

## Seams

I_GetJoysticks replaces the process device list. CreateOptionMenuItemJoyConfigMenu and the write barrier are host calls. MenuDescriptors is a case-insensitive name map. GetItem matches mAction, not the label. Reset's body is the header inline: default config, enable, background from AllowsEnabledInBackground, then sensitivity 1.

## Credit rationale

UNCredited joystickmenu.cpp 1-281 (+281). A full tally line scan shows no overlap. The file ends at line 281.
