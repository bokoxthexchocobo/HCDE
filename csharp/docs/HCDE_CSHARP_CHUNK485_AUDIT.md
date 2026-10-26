# Chunk 485 — AdjustFloorClip, PlayerPointerSubstitution, MorphPointerSubstitution gates

## Scope

AdjustFloorClip, PlayerPointerSubstitution, MorphPointerSubstitution gates

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::AdjustFloorClip** 6006–6064 (59 lines)
- `src/playsim/p_mobj.cpp` **AActor::AdjustFloorClip** 6066–6071 (6 lines)
- `src/playsim/p_mobj.cpp` **PlayerPointerSubstitution** 6093–6133 (41 lines)
- `src/playsim/p_mobj.cpp` **MorphPointerSubstitution** 6143–6190 (48 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmPMobjAdjustFloorClip.cs`
Managed LOC (tally): 205.
Conversion commit: `d1332614e7325e15513c7fcf6097843bbcc3682d`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SkipWhenSpecialFloorclip` / `ComputeShallowestClip` / `RunAdjustFloorClip` / terrain+3D helpers | `AdjustFloorClip` 6006–6071 |
| `PlayerSubValid` / `RunPlayerPointerSubstitution` / `SubstitutePlayerPointers` / `SubstituteSectorSoundTargets` | `PlayerPointerSubstitution` 6093–6133 |
| `MorphSubstitutionBlocked` / `EvaluateMorphSubstitution` / `RunPreMorphHooks` / morph gates | `MorphPointerSubstitution` 6143–6190 |

### Out-of-body uncredit (485)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **1** |
| **Honest** | **154** |


**Post-review ratio:** 205/154 managed/native ≈ 133% (src file LOC at HEAD; see tally).

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
