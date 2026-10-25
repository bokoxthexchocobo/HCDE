# Chunk 552 — A_CheckTerrain through SetGetBoneSharedIndex / SetBoneOffsetShared / GetBone* (5090–5318); merged ex-chunk553 bone-index credits Merged: [551].

## Scope

A_CheckTerrain through SetGetBoneSharedIndex / SetBoneOffsetShared / GetBone* (5090–5318); merged ex-chunk553 bone-index credits Merged: [551].

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5090–5111 (22 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5113–5146 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5148–5170 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5206–5220 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5222–5237 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5242–5270 (29 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5272–5296 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5298–5308 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5310–5313 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5315–5318 (4 lines)

**Credited native LOC:** 183 (tally `nativeLoc` 183).

## Managed

`VmActorTerrainSetSizeModel.cs`, `VmActorBoneSetGetShared.cs`, `ZccStackVariableEmit.cs`
Managed LOC (tally): 264.
Conversion commit: `7e934c9600733565394725ab6d9ef6ba198690cc`.

## Padding review (2026-10-21)

Credit fix (2026-10-21): dropped `A_SprayDecal` / `A_SetMugshotState` (no real port). Merged former chunk 553 after removing 5326–5464 credits (not whole-function ports). No filler methods for ratio.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CheckTerrainOnFloor`, `ShouldInstantDeathTerrain`, `ScrollStrifeCurrentThrust` | `A_CheckTerrain` 5090–5111 |
| `ResolveNewRadius`, `ResolveNewHeight`, `ShouldRevertSize` | `A_SetSize` 5113–5146 |
| `SetCameraRequiresPlayer`, `SetCameraNullSelectsSelf`, `ShouldClearPastViewer`, `SetCameraRevertSetsCheat` | `SetCamera` 5148–5170 |
| `EnsureModelDataFlags`, `ShouldCleanupModelData` | `EnsureModelData` 5206–5237 |
| `ModelIndexInRuntimeRange`, `ModelIndexInBaseFrames`, `TryResolveBoneModelIndex` | `SetGetBoneShared` 5242–5270 |
| `SetGetBoneSharedIndexGate`, `TryResolveNamedBoneIndex`, `TryValidateBoneIndex`, `NeedsBoneOverrideModelSlotGrow`, `BoneOverrideModelSlotCount` | `SetGetBoneSharedIndex` 5272–5296 |
| `NormalizeInterpolationDuration`, `IsValidSetBoneMode`, `TryPrepareSetBoneOffset` | `SetBoneOffsetShared` 5298–5308 |
| `SetGetBoneSharedIndexGate` | `GetBoneOffsetShared` 5310–5313, `GetBoneShared` 5315–5318 |

<!-- merged from chunk551 -->
Deepen pass (2026-10-21) added `StackEmitPlan` / `PlanStackVariableEmit` and duplicate wrappers (`ComputeStructMemberWritable`, `StructMemberMetaVariable`, color `NeedsShift`). Removed; see method map below.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `StackEmitOffsetReg`, `StackAddressRequestedUsesFramePointer`, `StackAddressRequestedUsesFrameAdd`, `StackEmitValueLoadUsesFramePointer`, `StackLoadOpForMember`, `StackUseBitLoad`, `StackBitMask`, `StackEmitBitPathAddsFrameOffset` | `FxStackVariable::Emit` 7561–7597 |
| `StructMemberBaseWritable` | `FxStructMember::RequestAddress` ctx/const-pointer gate 7646–7647 |
| `StackVariableWritable` | `FxStackVariable::RequestAddress` 7541–7545 |
| `StructMemberMetaReadOnly`, `StructMemberWritable` | `FxStructMember::RequestAddress` 7635–7664 |
| `VirtualScopeOuterFlags` | outerflags / virtual scope 7651–7656, 7687–7691 |
| `StructMemberInheritBarrierSide` | nested struct barrier 7702–7707 |
| `StructMemberResolveReadable` | scope readable gate 7694–7698 |
| `StaticReadonlyPromotesToGlobal` | static readonly promote 7710–7716 |
| `StructMemberPointerRequiresContainerType`, `StructMemberRequiresContainer` | pointer/container check 7718–7726 |
| `StructMemberColorChannelShift`, `ApplyColorChannelExtract` | color member resolve 7767–7777 |
| `ArrayElementRequestAddress` | `FxArrayElement::RequestAddress` 7914–7919 |

### Out-of-body uncredit (552)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **183** |
| **Out-of-body removed** | **0** |
| **Honest** | **183** |


**Post-review ratio:** 264/183 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmActorTerrainSetSizeModelTests.ShouldInstantDeathTerrain_AtTelefragThreshold`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 552 --to 552` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_TestMobjLocation`, `R_ClearPastViewer`, `FModel::FindJoint`, and live `modelData` allocation are not hosted.

## Credit rationale

Credits only whole native functions with matching managed gates; rotation/translation DEFINE wrappers and thin VM forwards are not credited.
