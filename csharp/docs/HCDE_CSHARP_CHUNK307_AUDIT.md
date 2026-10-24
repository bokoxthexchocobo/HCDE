# Chunk 307 — LoadDehSupp Aliases brace list

## Scope

LoadDehSupp Aliases brace list

## Native ranges

- `src/gamedata/d_dehacked.cpp` **LoadDehSupp Aliases** 3765–3780 (16 lines)

**Credited native LOC:** 16 (tally `nativeLoc` 16).

## Managed

`VmDehackedDehSuppAliases.cs`, `VmDehackedDehSuppAliasesTests.cs`
Managed LOC (tally): 28.
Conversion commit: `9b01c411a8e5dfd9cf6a22143db163bdcbb2df64`.

## Tests

- `VmDehackedDehSuppAliasesTests.ParseAliasEntries_ReadsPairsInOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 307 --to 307` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
