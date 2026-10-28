# Chunk 818 — `FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Scope

`FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Native ranges

- `src/playsim/p_user.cpp` **FObjectBackup::FObjectBackup** 395–398 (4 lines)
- `src/playsim/p_user.cpp` **FObjectBackup::GetObject** 401–404 (4 lines)
- `src/playsim/p_user.cpp` **FActorBackup::FActorBackup** 416–419 (4 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PostBackup** 421–439 (19 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PreRollback** 441–453 (13 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PostRollback** 455–500 (46 lines)
- `src/playsim/p_user.cpp` **FPredictionData::FPredictionData** 563–563 (1 lines)
- `src/playsim/p_user.cpp` **FPredictionData::CaptureWeaponPredictState** 539–548 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::RestoreWeaponPredictState** 550–561 (12 lines)
- `src/playsim/p_user.cpp` **FPredictionData::ResetPos** 572–575 (4 lines)
- `src/playsim/p_user.cpp` **FPredictionData::ClearBackup** 577–586 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::BackupWeaponBob** 588–595 (8 lines)
- `src/playsim/p_user.cpp` **FPredictionData::RestoreWeaponBob** 597–606 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::Mark** 608–616 (9 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmPUserActorRollback.cs`
Managed LOC (tally): 214.
Conversion commit: `e1bea4d40e204d636941b5a15e75b0339a8629bf`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|----------------------|
| `ObjectBackup` | `FObjectBackup` **395–404** |
| `ActorBackup` | `FActorBackup` **416–500** |
| `PredictionRollbackState` | `FPredictionData` **539–616** |

### Out-of-body uncredit (818)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **156** |
| **Out-of-body removed** | **2** |
| **Honest** | **154** |


**Post-review ratio:** 214/154 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserActorRollbackTests.PostBackup_ClearsPickupAndPushwallFlags`
- `VmPUserActorRollbackTests.PredictionState_DefaultConstructs`
- `VmPUserActorRollbackTests.CaptureRestoreWeaponPredictState_RoundTrips`
- `VmPUserActorRollbackTests.RestoreWeaponBob_RestoresPlayerSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 818 --to 818` (0 hard / 0 soft).

## Seams

`FPhysicsLinkBackup::Restore`, `GC::Mark`, and live `AActor` world links are injected; avoids overlap with chunk 815 physics capture and chunk 778 `CheckSkin`.

## Credit rationale

First tally credit for actor/object rollback snapshots used during prediction rewind (pickup/pushwall flags, stat/tid relink, weapon bob).
