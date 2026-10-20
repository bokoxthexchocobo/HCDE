# Chunk 249 — Re-audit: NormalizeResourceName only (not whole-file EDF header).

## Scope

Re-audit: NormalizeResourceName only (not whole-file EDF header).

## Native ranges

- `src/gamedata/hcde_edf.cpp` **NormalizeResourceName** 232–279 (48 lines)

**Credited native LOC:** 48 (tally `nativeLoc` 48).

## Managed

`HcdeEdfParseUtilities.cs`, `HcdeEdfParseUtilitiesTests.cs`
Managed LOC (tally): 21.
Conversion commit: `92e45538c946ba9a13641ebcd8d048474852f933`.

## Tests

- `HcdeEdfParseUtilitiesTests.NormalizeResourceName_CollapsesDotDot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 249 --to 249` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
