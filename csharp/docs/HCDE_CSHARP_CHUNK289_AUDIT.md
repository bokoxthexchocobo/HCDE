# Chunk 289 — PatchSpriteNames length check and table grow

## Scope

PatchSpriteNames length check and table grow

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchSpriteNames** 3287–3436 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 16).

## Managed

`VmDehackedIncludeDirective.cs`, `VmDehackedIncludeDirectiveTests.cs`
Managed LOC (tally): 18.
Conversion commit: `ce1401529f60012c504ae7972c4f910280c61062`.

## Tests

- `VmDehackedPatchSpriteTableTests.IsValidSpriteName_RequiresFourChars`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 289 --to 289` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
