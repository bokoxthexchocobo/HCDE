# Chunk 287 — PatchText Doom→MUSIC_DOOM hack only (sprite replace credited on chunk 285)

## Scope

PatchText Doom→MUSIC_DOOM hack only (sprite replace credited on chunk 285)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchText** 2987–3136 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 7).

## Managed

`VmDehackedPatchText.cs`, `VmDehackedPatchTextTests.cs`
Managed LOC (tally): 12.
Conversion commit: `c4d9001f4848d6dafeaf3916fcd6ba84551d924b`.

## Tests

- `VmDehackedPatchTextTests.IsDoomMusicHack_MatchesDoom`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 287 --to 287` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
