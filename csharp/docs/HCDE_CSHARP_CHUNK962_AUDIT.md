# Chunk 962 — `AActor::SetState` tail, static `DestroyAllInventory`, and `UseInventory` header from `p_mobj.cpp` (**872–1021**).

## Scope

`AActor::SetState` tail, static `DestroyAllInventory`, and `UseInventory` header from `p_mobj.cpp` (**872–1021**).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetState** 872–1021 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjSetStateInventory962Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `452edf2f906e0c11fdd13849b8effccb58c2c9a7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetState` null-state destroy (**872–876**) | yes |
| Sprite/frame/skin branch (**880–930**) | yes |
| Action jump / zero-tics loop (**933–953**) | yes |
| Dynamic light recreate flags (**955–959**) | yes |
| `AActor::SetState` export (**963–969**) | seam |
| `DestroyAllInventory` two-stage delete (**978–1005**) | yes |
| `DestroyAllInventory` export (**1007–1012**) | seam |
| `UseInventory` comment header (**1014–1021**) | seam (body **1024+**) |

| Managed (`VmPMobjSetStateInventory962Slice.cs`) | Native (`p_mobj.cpp`) |
|------------------------------------------------|---------------------|
| `StateLoopExhausted` / null-state destroy gates | **865–876** (hull **872+**) |
| `ResolveSpriteAfterStateChange` / skin rules | **899–929** |
| `ShouldAbortSetStateAfterAction` / zero-tic loop | **940–953** |
| `ShouldMarkRecreateLights` / `ApplyRecreateLightsFlags` | **955–958** |
| `CollectInventoryForDestruction` / `DestroyPendingInventory` | **988–1003** |
| Band hull (exclusive slice) | **872–1021** |

### Chunk **962** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**961** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSetStateInventory962SliceTests.StateLoopExhausted_MatchesNativeGuard`
- `VmPMobjSetStateInventory962SliceTests.ResolveSpriteAfterStateChange_UsesSkinWhenEligible`
- `VmPMobjSetStateInventory962SliceTests.ResolveSpriteAfterStateChange_KeepsSpriteWhenNoSkinAndSameBodySprite`
- `VmPMobjSetStateInventory962SliceTests.ShouldMarkRecreateLights_WhenAnyLightAssociationOrStateLight`
- `VmPMobjSetStateInventory962SliceTests.ApplyRecreateLightsFlags_SetsMf8Bit`
- `VmPMobjSetStateInventory962SliceTests.CollectInventoryForDestruction_UnlinksChain`
- `VmPMobjSetStateInventory962SliceTests.DestroyPendingInventory_SkipsEuthanizedItems`
- `VmPMobjSetStateInventory962SliceTests.ShouldAbortSetStateAfterAction_WhenEuthanizing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 962 --to 962` (0 hard / 0 soft).

## Seams

`CallAction`, `GetTics`, `FState::StaticGetStateName`, `Destroy`, and VM DEFINE exports remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmPMobjSetStateInventory962Slice.cs` (retired **737** row; off-tally `VmPMobjSetStateUse` not credited).
