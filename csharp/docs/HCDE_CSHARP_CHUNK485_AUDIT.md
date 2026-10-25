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
Managed LOC (tally): 100.
Conversion commit: `dd34931aecc0488132dc77a78fc3aced1f4d15b1`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SkipWhenSpecialFloorclip` / `TouchingFloorAtZ` / `Valid3dFloor` / `PickShallowestClip` / `ResolveFloorClip` / `AdjustViewHeight` / `TryTerrainClip` / `Try3dFloorClip` / `PlayerViewAdjust` | `AdjustFloorClip` 6006–6071 (6008: `if (flags3 & MF3_SPECIALFLOORCLIP)`; 6010: `return;`; 6024: `if (hsec == NULL)`) |
| `PlayerSubValid` | `PlayerPointerSubstitution` 6093–6133 (6095: `if (oldPlayer == nullptr || newPlayer == nullptr || oldPlayer == newP…`; 6098: `return;`; 6104: `if (!oldPlayer->Level->PlayerInGame(i))`) |
| `MorphSubstitutionBlocked` / `PlayerMorphRequiresValidMo` / `MorphInventoryOwnedBlocked` / `MorphNonPlayerPathAllowed` / `ShouldRunPreMorph` | `MorphPointerSubstitution` 6143–6190 (6148: `if (from == nullptr || to == nullptr || from == to || to->player != n…`; 6153: `return false;`; 6157: `if (from->IsKindOf(NAME_PlayerPawn))`) |

### Out-of-body uncredit (485)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **1** |
| **Honest** | **154** |


**Post-review ratio:** 100/154 managed/native ≈ 65% (src file LOC at HEAD; see tally).

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
