# Chunk 292 — A_Mushroom DEH vrange/hrange defaults

## Scope

A_Mushroom DEH vrange/hrange defaults

## Native ranges

- `src/gamedata/d_dehacked.cpp` **MBF param defaults** 786–935 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 5).

## Managed

`VmDehackedMbfParamDefaults.cs`, `VmDehackedMbfParamDefaultsTests.cs`
Managed LOC (tally): 14.
Conversion commit: `f5131a9454e792e38fb8d9f6209f0d98e7728ca7`.

## Tests

- `VmDehackedMbfParamDefaultsTests.MushroomVerticalRange_UsesDefaultWhenZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 292 --to 292` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
