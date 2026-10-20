# Chunk 285 — PatchMusic and ReplaceSpriteInData (re-audit 265-286: tested ranges only)

## Scope

PatchMusic and ReplaceSpriteInData (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchPointer tail and PatchMusic** 2837–2986 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 11).

## Managed

`VmDehackedPatchMusic.cs`, `VmDehackedPatchMusicTests.cs`, `VmDehackedReplaceSprite.cs`, `VmDehackedReplaceSpriteTests.cs`
Managed LOC (tally): 42.
Conversion commit: `acd0ae62e9f636e65ed7f30f727c97578e7b0bdc`.

## Tests

- `VmDehackedPatchMusicTests.TryParseMusicLine_BuildsMusicKey`
- `VmDehackedReplaceSpriteTests.IsPlayerSpritePrefix_DetectsPlay`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 285 --to 285` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
