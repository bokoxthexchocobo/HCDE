# Chunk 281 — FWeaponSlots locate and defaults (re-audit 265-286: tested ranges only)

## Scope

FWeaponSlots locate and defaults (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/a_weapons.cpp` **FWeaponSlots** 216–365 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 30).

## Managed

`VmWeaponSlotsRuntime.cs`, `VmWeaponSlotsRuntimeTests.cs`
Managed LOC (tally): 74.
Conversion commit: `82527e063d9f6b891fbaf8128d17efc5931f5940`.

## Tests

- `VmWeaponSlotsRuntimeTests.TryLocateWeapon_FindsAcrossSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 281 --to 281` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
