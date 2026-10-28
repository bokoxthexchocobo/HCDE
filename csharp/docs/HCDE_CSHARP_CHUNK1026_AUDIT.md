# Chunk 1026 — `ClearLocks`, sorted key list, `P_InitKeyMessages` orchestration, and `P_CheckKeys` head from `a_keys.cpp` (**344–493**). **Pivot:** primary **1026** target `p_sight.cpp` **1–150** aborted (`VmPSightInit` **126–145**, `VmPSightPtrSightTraverse` **73–76** / chunk **663** adjacency). Playsim scan had no other EOF-safe ≥150 band without off-tally duplicate (`VmPThingsRaiseRemoveWarpProximity`, `VmPEnemyChaseRandom`, etc.).

## Scope

`ClearLocks`, sorted key list, `P_InitKeyMessages` orchestration, and `P_CheckKeys` head from `a_keys.cpp` (**344–493**). **Pivot:** primary **1026** target `p_sight.cpp` **1–150** aborted (`VmPSightInit` **126–145**, `VmPSightPtrSightTraverse` **73–76** / chunk **663** adjacency). Playsim scan had no other EOF-safe ≥150 band without off-tally duplicate (`VmPThingsRaiseRemoveWarpProximity`, `VmPEnemyChaseRandom`, etc.).

## Native ranges

- `src/gamedata/a_keys.cpp` **P_InitKeyMessages and P_CheckKeys head** 344–493 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAKeysInitCheck1026Slice.cs`
Managed LOC (tally): 142.
Conversion commit: `5f0ab768cc93253408de4c289ce8f31e363aa011`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ClearLocks` (**352–369**) | yes |
| `ktcmp` / `CreateSortedKeyList` (**380–415**) | yes |
| `P_InitKeyMessages` lump loop + `CLEARLOCKS` (**423–454**) | yes (delegates `ParseLock` to chunk **173** / `VmLockDefParser`) |
| `P_CheckKeys` gates + lock lookup + `lock->check` (**466–493**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **344–351**, **417–421**, **456–465** | comment banners |
| **237–343** | chunk **173** `ParseLock` / `ParseKeygroup` |
| **1–150**, **151–300** | `OneKey`/`Lock` check structs — off-tally `VmActorKeyCheck` duplicate (retired **238**) |
| **494+** | message/sound tail outside hull |

| Managed (`VmAKeysInitCheck1026Slice.cs`) | Native (`a_keys.cpp`) |
|---------------------------------------------|---------------------------|
| `ClearLocks` / `ApplyKeyNumbersFromDatabase` | **352–369** |
| `CompareKeyNumbers` / `CreateSortedKeyList` | **380–415** |
| `TryInitializeFromLockDefTexts` | **423–454** |
| `TryEvaluateCheckKeysHead` / `LockCheck` | **466–493** |
| Band hull (exclusive slice) | **344–493** |

### Chunk **1026** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1025** |


**Post-review ratio:** 142/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmAKeysInitCheck1026SliceTests.ClearLocks_ResetsKeyNumbersAndDatabase`
- `VmAKeysInitCheck1026SliceTests.CreateSortedKeyList_OrdersAssignedThenUnassigned`
- `VmAKeysInitCheck1026SliceTests.TryInitializeFromLockDefTexts_BuildsLocksAndSortedList`
- `VmAKeysInitCheck1026SliceTests.TryEvaluateCheckKeysHead_AllowsNonPositiveKeyNum`
- `VmAKeysInitCheck1026SliceTests.TryEvaluateCheckKeysHead_RejectsNullOwner`
- `VmAKeysInitCheck1026SliceTests.TryEvaluateCheckKeysHead_MissingLockUsesSharewareText`
- `VmAKeysInitCheck1026SliceTests.TryEvaluateCheckKeysHead_SucceedsWhenLockMatchesInventory`
- `VmAKeysInitCheck1026SliceTests.TryEvaluateCheckKeysHead_FailureUsesRemoteMessageWhenRequested`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1026 --to 1026` (0 hard / 0 soft).

## Seams

`FScanner`, `fileSystem.FindLump`, `PClassActor::AllActorClasses`, `GetDefaultByType`, `qsort`, `PrintMessage`, and sound playback past **493** remain native.

## Credit rationale

UNCredited tail after chunk **173** (`ParseLock` **164–343**); chunk **238** retire left `OneKey`/`Lock` check uncredited. No overlap with `VmLockDefParser` parse body or off-tally `VmActorKeyCheck` tally rows.
