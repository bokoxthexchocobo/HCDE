# Chunk 262 — ReadChars, ReplaceSpecialChars, skip/strip white, GetLine key split.

## Scope

ReadChars, ReplaceSpecialChars, skip/strip white, GetLine key split.

## Native ranges

- `src/gamedata/d_dehacked.cpp` **ReadChars** 600–620 (21 lines)
- `src/gamedata/d_dehacked.cpp` **ReplaceSpecialChars** 623–690 (68 lines)
- `src/gamedata/d_dehacked.cpp` **skipwhite and stripwhite** 692–707 (16 lines)
- `src/gamedata/d_dehacked.cpp` **GetLine** 730–785 (56 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmDehackedPatchLexer.cs`, `VmDehackedPatchLexerTests.cs`
Managed LOC (tally): 118.
Conversion commit: `ec45132df27491c72092e912dc1741a0681a0a90`.

## Tests

- `VmDehackedPatchLexerTests.ReadChars_SkipsCarriageReturns`
- `VmDehackedPatchLexerTests.ReplaceSpecialChars_DecodesNewlineAndHex`
- `VmDehackedPatchLexerTests.TryParsePatchLine_SplitsKeyValue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 262 --to 262` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
