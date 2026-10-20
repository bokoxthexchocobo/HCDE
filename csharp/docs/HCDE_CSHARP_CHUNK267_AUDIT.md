# Chunk 267 — PatchThing frame/sound labels and Bits parse through translucency tail. (re-audit 265-286: tested ranges only)

## Scope

PatchThing frame/sound labels and Bits parse through translucency tail. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing frame labels** 1563–1602 (40 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing sound labels** 1603–1630 (28 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing Bits parse** 1638–1822 (185 lines)

**Credited native LOC:** 253 (tally `nativeLoc` 36).

## Managed

`VmDehackedPatchThingBits.cs`, `VmDehackedPatchThingBitsTests.cs`, `VmDehackedPatchThingNumeric.cs`, `VmDehackedPatchThingStateLabels.cs`
Managed LOC (tally): 236.
Conversion commit: `4dd14c9f92965d02e45d910cca422e5eac74cb36`.

## Tests

- `VmDehackedPatchThingStateLabelsTests.TryMapFrameLabel_Spawn`
- `VmDehackedPatchThingStateLabelsTests.TryMapSoundLabel_Death`
- `VmDehackedPatchThingBitsTests.RemapMbfFlags_SetsGrenadeBounceStyle`
- `VmDehackedPatchThingBitsTests.ResolveTranslucentAlpha_ReturnsHalf`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 267 --to 267` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
