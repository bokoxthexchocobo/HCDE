# Chunk 520 — PatchThing setup and property/frame/sound loop (part 1)

## Scope

PatchThing setup and property/frame/sound loop (part 1)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing (primary loop)** 1234–1631 (398 lines)

**Credited native LOC:** 398 (tally `nativeLoc` 398).

## Managed

`VmDehackedPatchThingLoopPrimary.cs`, `VmDehackedPatchThingLoopPrimaryTests.cs`
Managed LOC (tally): 212.
Conversion commit: `926c72a69dfa236bbcae8f7e5b938d71faeded6a`.

## Tests

- `VmDehackedPatchThingLoopPrimaryTests.MapFrameLabelPrefix_Spawn`
- `VmDehackedPatchThingLoopPrimaryTests.SkipPlayerMeleeMissile_BlocksPlayer`
- `VmDehackedPatchThingLoopPrimaryTests.ClampInfightingGroup_NegativeToZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 520 --to 520` (0 hard / 0 soft).

## Seams

`GetLine`, `FindState`, `statedef`, `DecalLibrary`, and actor mutation are not hosted.

## Credit rationale

Credits DeHackEd thing setup, numeric bounds, group clamps, obituaries, frame labels, and sound labels from `PatchThing` in `d_dehacked.cpp` (1234–1631).
