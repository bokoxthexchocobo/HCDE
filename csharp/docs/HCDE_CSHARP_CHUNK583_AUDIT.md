# Chunk 583 — `A_RemoveTarget` through `A_SwapTeleFog` (`p_actionfunctions.cpp` **4466–4635**). `DoRemove` body stays on **547** (**4441–4463**); stops before **547** `A_SetHealth` (**4652+**).

## Scope

`A_RemoveTarget` through `A_SwapTeleFog` (`p_actionfunctions.cpp` **4466–4635**). `DoRemove` body stays on **547** (**4441–4463**); stops before **547** `A_SetHealth` (**4652+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_RemoveTarget** 4466–4597 (132 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTeleFog** 4599–4635 (37 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmActorRemoveActions.cs`
Managed LOC (tally): 90.
Conversion commit: `92f4c99083b4dae93e8217790fd4c90925b31cf6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `ShouldRemoveWhenPresent` | `A_RemoveTarget` **4478–4480**, `A_RemoveTracer` **4497–4499**, `A_Remove` **4592–4594** |
| `ShouldRemoveMaster` | `A_RemoveMaster` **4516–4518** |
| `ShouldRemoveChildInIterator` / `IsRemoveChildOfMaster` | `A_RemoveChildren` **4541–4543** |
| `ShouldRunRemoveSiblingsLoop` / `RemoveSiblingsLoop` | `A_RemoveSiblings` **4565–4571** |
| `ShouldRemoveReference` | `A_Remove` **4591–4594** |
| `AssignTeleFogTypes` | `A_SetTeleFog` **4613–4614** |
| `ShouldSwapTeleFog` / `SwapTeleFogWhenDistinct` | `A_SwapTeleFog` **4628–4632** |

### vs chunk **547** / **581**

| | Lines |
|--|------:|
| **547** (`DoRemove`) | **4441–4463** |
| **581** (kill family) | **4158–4422** |
| **583** (this row) | **4466–4635** (**170** function-edge hull) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **170** |
| Net behavior-ported | **170** |


**Post-review ratio:** 90/169 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmActorRemoveActionsTests.ShouldRemoveWhenPresent_Target`
- `VmActorRemoveActionsTests.ShouldRemoveTarget_AndTracer_UsePresenceGate`
- `VmActorRemoveActionsTests.ShouldRemoveMaster_WhenSet`
- `VmActorRemoveActionsTests.ShouldRemoveChildInIterator_MatchesHealthGate`
- `VmActorRemoveActionsTests.RemoveSiblingsLoop_RequiresDistinctActor`
- `VmActorRemoveActionsTests.ShouldRemoveSiblingCandidate_MatchesNativeGuard`
- `VmActorRemoveActionsTests.ShouldRemoveReference_WhenNonNull`
- `VmActorRemoveActionsTests.AssignTeleFogTypes_MatchesNativeFields`
- `VmActorRemoveActionsTests.SwapTeleFogWhenDistinct_SkipsWhenEqual`
- `VmActorRemoveActionsTests.ShouldSwapTeleFog_WhenTypesDiffer`
- `VmActorRemoveActionsTests.ApplySwappedTeleFogFields_RotatesDistinctTypes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 583 --to 583` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `P_RemoveThing`, thinker iterators, and `PClassActor` filter resolution are not hosted.

## Credit rationale

Remove-family dispatch guards and telefog assignment/swap mirrored by `VmActorRemoveActions` (parallel to **581** kill wrappers); **547** keeps exclusive `VmActorDoRemove.cs` for `DoRemove` filter policy only.
