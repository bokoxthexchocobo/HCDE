# Chunk 302 — addslot argument validation

## Scope

addslot argument validation

## Native ranges

- `src/gamedata/a_weapons.cpp` **addslot** 816–945 (130 lines)

**Credited native LOC:** 130 (tally `nativeLoc` 5).

## Managed

`VmWeaponSlotsAddSlot.cs`, `VmWeaponSlotsAddSlotTests.cs`
Managed LOC (tally): 14.
Conversion commit: `a04301d723b484caea74324eebbff9210994dc7a`.

## Tests

- `VmWeaponSlotsAddSlotTests.TryParseAddSlotArgs_ParsesSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 302 --to 302` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
