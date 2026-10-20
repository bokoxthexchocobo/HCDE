# Chunk 288 — PatchStrings line fold and red skull key alias

## Scope

PatchStrings line fold and red skull key alias

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchStrings** 3137–3286 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 16).

## Managed

`VmDehackedPatchSpriteTable.cs`, `VmDehackedPatchSpriteTableTests.cs`, `VmDehackedPatchStrings.cs`, `VmDehackedPatchStringsTests.cs`
Managed LOC (tally): 25.
Conversion commit: `cdd4fbae2646ddfd181e263e12fdf9657a5d688e`.

## Tests

- `VmDehackedPatchStringsTests.FoldContinuationLines_JoinsBackslash`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 288 --to 288` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
