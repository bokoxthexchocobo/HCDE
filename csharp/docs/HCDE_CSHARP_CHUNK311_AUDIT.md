# Chunk 311 — OrgHeights and CodePConv

## Scope

OrgHeights and CodePConv

## Native ranges

- `src/gamedata/d_dehacked.cpp` **OrgHeights** 3548–3558 (11 lines)
- `src/gamedata/d_dehacked.cpp` **CodePConv** 3559–3569 (11 lines)

**Credited native LOC:** 22 (tally `nativeLoc` 22).

## Managed

`VmDehackedDehSuppNumericLists.cs`
Managed LOC (tally): 40.
Conversion commit: `8ae97e7f6daa9a22ac5d0e54179b8ae2c7615297`.

## Tests

- `VmDehackedDehSuppNumericListsTests.ParseIntList_ReadsOrgHeights`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 311 --to 311` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
