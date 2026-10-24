# Chunk 754 — Post-serialize through behavior transfer, link, and `InStateSequence` (`p_mobj.cpp` 431–822).

## Scope

Post-serialize through behavior transfer, link, and `InStateSequence` (`p_mobj.cpp` 431–822).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::PostSerialize** 431–455 (25 lines)
- `src/playsim/p_mobj.cpp` **DBehavior::Serialize** 464–469 (6 lines)
- `src/playsim/p_mobj.cpp` **DBehavior::OnDestroy** 471–477 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::RemoveBehavior** 479–495 (17 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, RemoveBehavior, RemoveBehavior)** 502–507 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::AddBehavior** 509–554 (46 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, AddBehavior, AddBehavior)** 561–566 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::TickBehaviors** 568–607 (40 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, TickBehaviors, TickBehaviors)** 614–619 (6 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, FindBehavior, FindBehavior)** 626–631 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::MoveBehaviors** 633–692 (60 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, MoveBehaviors, MoveBehaviors)** 699–705 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::ClearBehaviors** 707–726 (20 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, ClearBehaviors, ClearBehaviors)** 733–739 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::UnlinkBehaviorsFromLevel** 741–758 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::LinkBehaviorsToLevel** 760–777 (18 lines)
- `src/playsim/p_mobj.cpp` **InStateSequence** 788–801 (14 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, InStateSequence, InStateSequence)** 803–809 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsMapActor** 818–822 (5 lines)

**Credited native LOC:** 321 (tally `nativeLoc` 321).

## Managed

`VmPMobjPostSerializeBehaviors.cs`, `VmPMobjBehaviorTransfer.cs`
Managed LOC (tally): 188.
Conversion commit: `771a1cbaef97a6a211a1df8df79786ad6ce81488`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `PostSerializeClearTouchLists` / `ShouldRestorePlayerSkin` / `ShouldRestoreDefaultSpeed` | `AActor::PostSerialize` 431–455 |
| `SerializeBehaviorOwnerFields` | `DBehavior::Serialize` 464–469 |
| `RemoveBehaviorOnLevelDestroy` | `DBehavior::OnDestroy` 471–477 |
| `RemoveBehaviorFound` | `AActor::RemoveBehavior` 479–495 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, RemoveBehavior)` 502–507 |
| `AddBehaviorAbstract` / `AddBehaviorCreatesNew` / `AddBehaviorReinitialize` | `AActor::AddBehavior` 509–554 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, AddBehavior)` 561–566 |
| `TickBehaviorsQueuesNull` / `FlushBehaviorRemovals` | `AActor::TickBehaviors` 568–607 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, TickBehaviors)` 614–619 |
| `FindBehaviorInMap` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, FindBehavior)` 626–631 |
| `MoveBehaviorsRejected` / `TransferBehaviorOwner` / `RelinkBehaviorLevel` | `AActor::MoveBehaviors` 633–692 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, MoveBehaviors)` 699–705 |
| `ClearBehaviorsMatching` / `ClearAllBehaviorsWhenTypeNull` | `AActor::ClearBehaviors` 707–726 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, ClearBehaviors)` 733–739 |
| `UnlinkBehaviorNull` | `AActor::UnlinkBehaviorsFromLevel` 741–758 |
| `UnlinkBehaviorNull` | `AActor::LinkBehaviorsToLevel` 760–777 |
| `InStateSequence` | `InStateSequence` 788–801 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, InStateSequence)` 803–809 |
| `IsMapActorInventory` | `AActor::IsMapActor` 818–822 |


**Post-review ratio:** 188/321 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjPostSerializeBehaviorsTests.PostSerialize_ClearsTouchLists`
- `VmPMobjPostSerializeBehaviorsTests.ShouldRestorePlayerSkin_WhenEligible`
- `VmPMobjPostSerializeBehaviorsTests.RemoveBehaviorFound_Destroys`
- `VmPMobjPostSerializeBehaviorsTests.AddBehaviorAbstract_RejectsAbstract`
- `VmPMobjPostSerializeBehaviorsTests.TickBehaviorsQueuesNull_AddsKey`
- `VmPMobjBehaviorTransferTests.MoveBehaviorsRejected_ClientMismatch`
- `VmPMobjBehaviorTransferTests.ClearBehaviorsMatching_AddsKey`
- `VmPMobjBehaviorTransferTests.UnlinkBehaviorNull_QueuesMissing`
- `VmPMobjBehaviorTransferTests.InStateSequence_FindsAdjacent`
- `VmPMobjBehaviorTransferTests.IsMapActorInventory_OwnerNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 754 --to 754` (0 hard / 0 soft).

## Seams

`LinkToWorld`, `VMCall`, `I_Error`, and level behavior maps are injected.

## Credit rationale

Whole behavior-management band in the long-uncredited 422–855 region (merged 754/755 plan).
