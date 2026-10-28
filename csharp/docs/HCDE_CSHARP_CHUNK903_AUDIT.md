# Chunk 903 — `ThrustThingHelper` through `LS_Thing_Deactivate` (`p_lnspec.cpp` **1205–1479**). Follows **902** (**≤1203**); stops before `LS_Thing_Remove` (**1481+**). Wider than **1204–1450** to finish deactivate function.

## Scope

`ThrustThingHelper` through `LS_Thing_Deactivate` (`p_lnspec.cpp` **1205–1479**). Follows **902** (**≤1203**); stops before `LS_Thing_Remove` (**1481+**). Wider than **1204–1450** to finish deactivate function.

## Native ranges

- `src/playsim/p_lnspec.cpp` **ThrustThingHelper** 1205–1479 (275 lines)

**Credited native LOC:** 275 (tally `nativeLoc` 275).

## Managed

`VmPLnspecThrustThingLifecycleSlice.cs`
Managed LOC (tally): 166.
Conversion commit: `a2024e01792e5333889bc01a878a5ade28ddf85d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `ThrustThingHelper` / `ThrustThing` | **1205–1237** |
| `ThrustThingZ` | **1239–1271** |
| `ThingSetSpecial` / `ThingChangeTid` | **1273–1328** |
| `DamageThing` / `HealThing` | **1331–1395** |
| `ToggleActivationType` | **1399–1419** |
| `ThingActivate` / `ThingDeactivate` | **1421–1479** |

### vs **902**

| | Lines |
|--|------:|
| **902** | **915–1203** |
| **903** (this row) | **1205–1479** (**275**) |
| `LS_Thing_Remove` | **1481+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **275** |
| Net behavior-ported | **275** |


**Post-review ratio:** 166/275 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecThrustThingLifecycleSliceTests.ThrustThingHelper_ClampsVelocity`
- `VmPLnspecThrustThingLifecycleSliceTests.ThrustThing_HexenBacksideRejectsActivator`
- `VmPLnspecThrustThingLifecycleSliceTests.ToggleActivationType_SwitchClearsActivate`
- `VmPLnspecThrustThingLifecycleSliceTests.DamageThing_ZeroUsesTelefrag`
- `VmPLnspecThrustThingLifecycleSliceTests.ApplyHealCap_RespectsMax`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 903 --to 903` (0 hard / 0 soft).

## Seams

`Thrust`, `P_GiveBody`, `P_DamageMobj`, `CallActivate`/`CallDeactivate`, and actor iterators are not hosted.

## Credit rationale

BYTEANGLE thrust clamp, Hexen backside reject, Z-thrust set/add, thing special/TID writes, damage/heal branches, THINGSPEC toggle, and activate/deactivate iterator prefetch via `VmPLnspecThrustThingLifecycleSlice`.
