# Chunk 301 — PrintSettings non-empty slot enumeration

## Scope

PrintSettings non-empty slot enumeration

## Native ranges

- `src/gamedata/a_weapons.cpp` **PrintSettings** 666–815 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 15).

## Managed

`VmWeaponSlotsPrintSettings.cs`, `VmWeaponSlotsPrintSettingsTests.cs`
Managed LOC (tally): 16.
Conversion commit: `578114b1c6593a5e0641f39409b0823dfbaf23e9`.

## Tests

- `VmWeaponSlotsPrintSettingsTests.EnumerateNonEmptySlots_SkipsEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 301 --to 301` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
