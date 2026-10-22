# Chunk 818 — `FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Scope

`FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Native ranges

- `src/playsim/p_user.cpp` **FObjectBackup::FObjectBackup** 394–394 (1 lines)
- `src/playsim/p_user.cpp` **FObjectBackup::FObjectBackup** 395–398 (4 lines)
- `src/playsim/p_user.cpp` **FObjectBackup::GetObject** 400–404 (5 lines)
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

**Credited native LOC:** 156 (tally `nativeLoc` 156).

### CLI span 389–616 vs credited LOC

`uncredited_range_check.py --file src/playsim/p_user.cpp --start 389 --end 616` reports **228** physical lines in the band. Whole-function credit sums to **156**; the other **72** lines are not separate uncredited *functions* (nothing skipped except the two default ctors now credited at **394** and **563**). They are:

| Lines (approx.) | What |
| ---: | --- |
| 389–393 | `struct FObjectBackup` / `private:` / `TObjPtr` member |
| 405–415 | `struct FActorBackup` declaration, `private` backup fields (`_link`, `_statNum`, `_tid`, flags) |
| 503–537 | `FPredictionData` data members (`bResetPrediction`, `Rollback*` arrays, weapon-predict scalars, comments) |
| 519–521 | `WeaponBobBackup` / `WeaponBobBackupPlayer` / `WeaponBobBackupValid` |
| 565–570 | Nested anonymous `LastPos` struct + default member initializers |
| 617 | `} static PredictionData = {};` (global instance; not a member function) |
| (scattered) | Blank lines and `//` comments inside credited function bodies |

**Deliberately outside band:** `CheckSkin` at **632+** (chunk **778**); band ends at **616** before that symbol.

## Managed

`VmPUserActorRollback.cs`
Managed LOC (tally): 214.
Conversion commit: `365f6f3e788ddb196ebce189a49efb7b54f703ea`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|----------------------|
| `ObjectBackup` | `FObjectBackup` **395–404** |
| `ActorBackup` | `FActorBackup` **416–500** |
| `PredictionRollbackState` | `FPredictionData` **539–616** |

**Post-review ratio:** 214/156 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserActorRollbackTests.ObjectBackup_DefaultConstructs`
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
