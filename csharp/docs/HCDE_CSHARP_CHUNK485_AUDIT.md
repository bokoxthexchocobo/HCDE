# Chunk 485 — AdjustFloorClip, PlayerPointerSubstitution, MorphPointerSubstitution gates

## Scope

AdjustFloorClip, PlayerPointerSubstitution, MorphPointerSubstitution gates

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::AdjustFloorClip** 6006–6071 (66 lines)
- `src/playsim/p_mobj.cpp` **PlayerPointerSubstitution** 6093–6133 (41 lines)
- `src/playsim/p_mobj.cpp` **MorphPointerSubstitution** 6143–6190 (48 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPMobjAdjustFloorClip.cs`, `VmPMobjAdjustFloorClipTests.cs`
Managed LOC (tally): 104.
Conversion commit: `c04bb549c102bbec472773ba87c2bd912771c9d0`.

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
