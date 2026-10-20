# Chunk 276 — PatchSound consumer and PatchPointer Boom hack (re-audit 265-286: tested ranges only)

## Scope

PatchSound consumer and PatchPointer Boom hack (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchSound** 1913–1964 (52 lines)
- `src/gamedata/d_dehacked.cpp` **SetPointer and PatchPointer** 2439–2536 (98 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 25).

## Managed

`VmDehackedPatchPointer.cs`, `VmDehackedPatchPointerTests.cs`, `VmDehackedPatchSound.cs`, `VmDehackedPatchSoundTests.cs`
Managed LOC (tally): 64.
Conversion commit: `9c5914c5857510533a2fe6bc70e3b4ae654192fc`.

## Tests

- `VmDehackedPatchSoundTests.ConsumePatchLines_StopsOnNonOne`
- `VmDehackedPatchPointerTests.ResolveBoomPointerZero_FindsIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 276 --to 276` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
