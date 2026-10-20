# Chunk 300 — setslot slot index guard

## Scope

setslot slot index guard

## Native ranges

- `src/gamedata/a_weapons.cpp` **setslot** 516–665 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 4).

## Managed

`VmWeaponSlotsSetSlot.cs`, `VmWeaponSlotsSetSlotTests.cs`
Managed LOC (tally): 12.
Conversion commit: `a10c1fa0cfb18e9845d6cb30929973f18145406e`.

## Tests

- `VmWeaponSlotsSetSlotTests.IsValidSlotNumber_RejectsTen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 300 --to 300` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
