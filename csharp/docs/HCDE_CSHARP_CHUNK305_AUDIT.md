# Chunk 305 — LoadDehSupp WeaponNames brace list

## Scope

LoadDehSupp WeaponNames brace list

## Native ranges

- `src/gamedata/d_dehacked.cpp` **LoadDehSupp WeaponNames** 3748–3764 (17 lines)

**Credited native LOC:** 17 (tally `nativeLoc` 17).

## Managed

`VmDehackedDehSuppWeaponNames.cs`, `VmDehackedDehSuppWeaponNamesTests.cs`
Managed LOC (tally): 32.
Conversion commit: `d297ab0ecc3fc5f55a3f7207877674d98af2f3eb`.

## Tests

- `VmDehackedDehSuppWeaponNamesTests.ParseWeaponNameList_ClearsAndAppendsTypes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 305 --to 305` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
