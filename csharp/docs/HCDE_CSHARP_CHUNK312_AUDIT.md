# Chunk 312 — LoadDehSupp OrgSprNames section

## Scope

LoadDehSupp OrgSprNames section

## Native ranges

- `src/gamedata/d_dehacked.cpp` **OrgSprNames** 3570–3597 (28 lines)

**Credited native LOC:** 28 (tally `nativeLoc` 28).

## Managed

`VmDehackedDehSuppOrgSprNames.cs`
Managed LOC (tally): 52.
Conversion commit: `d5e576884041eaa35317d3ec558a4e05e4fcb296`.

## Tests

- `VmDehackedDehSuppOrgSprNamesTests.TryParseSpriteName_RequiresFourChars`
- `VmDehackedDehSuppOrgSprNamesTests.TryParseOrgSprNamesSection_SetsOrgSprOrgSize`
- `VmDehackedDehSuppOrgSprNamesTests.TryParseOrgSprNamesSection_SkipsAppendWhenExisting`
- `VmDehackedDehSuppOrgSprNamesTests.TryParseOrgSprNamesSection_RejectsInvalidSprite`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 312 --to 312` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
