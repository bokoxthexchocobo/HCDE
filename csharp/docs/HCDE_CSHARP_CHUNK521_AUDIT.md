# Chunk 521 — PatchThing Bits/Mass/ID and finalize (part 2)

## Scope

PatchThing Bits/Mass/ID and finalize (part 2)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing (secondary loop)** 1632–1906 (275 lines)

**Credited native LOC:** 275 (tally `nativeLoc` 275).

## Managed

`VmDehackedPatchThingLoopSecondary.cs`, `VmDehackedPatchThingLoopSecondaryTests.cs`
Managed LOC (tally): 190.
Conversion commit: `0c20470a8f4ef772d5a368196647383eda2f9563`.

## Tests

- `VmDehackedPatchThingLoopSecondaryTests.ResolveTranslucentAlpha_Half`
- `VmDehackedPatchThingLoopSecondaryTests.HeightPairInvalid_WhenMismatched`
- `VmDehackedPatchThingLoopSecondaryTests.NormalizeSpeed_ScalesLarge`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 521 --to 521` (0 hard / 0 soft).

## Seams

`statedef.InstallStates`, `PushTouchedActor`, `I_Error`, and `OrgHeights` are not hosted.

## Credit rationale

Credits DeHackEd thing Bits remaps, Mass/ID#, height-pair validation, shadow/speed finalize, and COUNTKILL flags from `PatchThing` in `d_dehacked.cpp` (1632–1906).
