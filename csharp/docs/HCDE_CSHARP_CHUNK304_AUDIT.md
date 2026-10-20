# Chunk 304 — LoadDehSupp AmmoNames brace list

## Scope

LoadDehSupp AmmoNames brace list

## Native ranges

- `src/gamedata/d_dehacked.cpp` **LoadDehSupp AmmoNames** 3725–3746 (22 lines)

**Credited native LOC:** 22 (tally `nativeLoc` 22).

## Managed

`VmCmpgninfParser.cs`, `VmCmpgninfParserTests.cs`, `VmDehackedDehSuppAmmoNames.cs`, `VmDehackedDehSuppAmmoNamesTests.cs`, `VmDehackedIncludeDirectiveTests.cs`, `VmDehackedMbfParamEmittersTests.cs`, `VmDehackedPatchSpriteTableTests.cs`, `VmDehackedPatchTextTests.cs`, `VmEMapInfoPropertyTailTests.cs`
Managed LOC (tally): 32.
Conversion commit: `3bc60909bddc164448cb40ef18f757f90d5e384d`.

## Tests

- `VmDehackedDehSuppAmmoNamesTests.ParseAmmoNameList_AcceptsNullAndTypes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 304 --to 304` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
