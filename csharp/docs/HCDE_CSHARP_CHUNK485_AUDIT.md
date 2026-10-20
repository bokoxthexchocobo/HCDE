# Chunk 485 — AActor AdjustFloorClip terrain and 3D-floor foot clip

## Scope

AActor AdjustFloorClip terrain and 3D-floor foot clip

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::AdjustFloorClip** 6006–6071 (66 lines)
- `src/playsim/p_mobj.cpp` **PlayerPointerSubstitution** 6093–6133 (41 lines)
- `src/playsim/p_mobj.cpp` **MorphPointerSubstitution** 6143–6190 (48 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 66).

## Managed

`VmPMobjAdjustFloorClip.cs`, `VmPMobjAdjustFloorClipTests.cs`
Managed LOC (tally): 55.
Conversion commit: `16d6e11b1954cc0c3ff4cf4c8795ce0a4e640fbd`.

## Tests

- `VmPMobjAdjustFloorClipTests.Skip_SpecialFloorclipFlag`
- `VmPMobjAdjustFloorClipTests.TerrainClip_PicksShallowest`
- `VmPMobjAdjustFloorClipTests.PlayerView_AdjustsWhenClipChanges`
- `VmPMobjAdjustFloorClipTests.PlayerSub_RejectsInvalidPair`
- `VmPMobjAdjustFloorClipTests.MorphSub_BlocksUnmorphedDummy`
- `VmPMobjAdjustFloorClipTests.MorphSub_PlayerRequiresRealMo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 485 --to 485` (0 hard / 0 soft).

## Seams

`touching_sectorlist`, `GC::Root` pointer substitution, and `VMCall` PreMorph are not hosted.

## Credit rationale

Credits floorclip terrain/3D-floor selection, player pointer rewiring on pawn swap, and morph substitution eligibility from `p_mobj.cpp` (6006–6190).
