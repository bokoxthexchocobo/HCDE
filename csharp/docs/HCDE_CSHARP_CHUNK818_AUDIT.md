# Chunk 818 — `FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Scope

`FObjectBackup`, `FActorBackup`, and `FPredictionData` rollback helpers from `p_user.cpp` (389–616).

## Native ranges (credited whole functions)

| Symbol | Lines | LOC |
|--------|-------|-----|
| `FObjectBackup::FObjectBackup` (default) | 394–394 | 1 |
| `FObjectBackup::FObjectBackup` | 395–398 | 4 |
| `FObjectBackup::GetObject` | 400–404 | 5 |
| `FActorBackup::FActorBackup` | 416–419 | 4 |
| `FActorBackup::PostBackup` | 421–439 | 19 |
| `FActorBackup::PreRollback` | 441–453 | 13 |
| `FActorBackup::PostRollback` | 455–500 | 46 |
| `FPredictionData::FPredictionData` | 563–563 | 1 |
| `FPredictionData::CaptureWeaponPredictState` | 539–548 | 10 |
| `FPredictionData::RestoreWeaponPredictState` | 550–561 | 12 |
| `FPredictionData::ResetPos` | 572–575 | 4 |
| `FPredictionData::ClearBackup` | 577–586 | 10 |
| `FPredictionData::BackupWeaponBob` | 588–595 | 8 |
| `FPredictionData::RestoreWeaponBob` | 597–606 | 10 |
| `FPredictionData::Mark` | 608–616 | 9 |

**Credited native LOC:** 156 (tally `nativeLoc` after ctor extension).

## CLI span 389–616 vs credit (228 vs 156)

The **73** lines in the uncredited-range CLI window that are **not** whole-function credit:

| Lines | LOC | What |
|-------|-----|------|
| 389–393 | 5 | `FObjectBackup` struct opener + `private:` `_obj` member (not a function) |
| 405 | 1 | `};` closing `FObjectBackup` |
| 406–415 | 10 | `FActorBackup` struct fields (`_link`, `_statNum`, …) before methods |
| 420, 440, 454 | 3 | blank separators between methods |
| 501–538 | 38 | `FPredictionData` **data members** + comments (rollback arrays, weapon-predict scalars, `WeaponPSpriteTic`, …) — not functions |
| 549, 562, 576, 587, 596, 607 | 6 | blank lines between methods |
| 565–571 | 7 | anonymous `LastPos` nested struct inside `FPredictionData` (fields only) |

No other whole functions exist in 389–616 except those credited above and **chunk 778** `FPlayerClass::CheckSkin` at **632–641** (deliberately excluded). Line **617** (`static PredictionData`) is outside the 389–616 audit window.

## Native ranges (detail)

- `src/playsim/p_user.cpp` **FObjectBackup::FObjectBackup** (default) 394–394 (1 line)
- `src/playsim/p_user.cpp` **FObjectBackup::FObjectBackup** 395–398 (4 lines)
- `src/playsim/p_user.cpp` **FObjectBackup::GetObject** 400–404 (5 lines)
- `src/playsim/p_user.cpp` **FActorBackup::FActorBackup** 416–419 (4 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PostBackup** 421–439 (19 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PreRollback** 441–453 (13 lines)
- `src/playsim/p_user.cpp` **FActorBackup::PostRollback** 455–500 (46 lines)
- `src/playsim/p_user.cpp` **FPredictionData::CaptureWeaponPredictState** 539–548 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::RestoreWeaponPredictState** 550–561 (12 lines)
- `src/playsim/p_user.cpp` **FPredictionData::ResetPos** 572–575 (4 lines)
- `src/playsim/p_user.cpp` **FPredictionData::ClearBackup** 577–586 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::BackupWeaponBob** 588–595 (8 lines)
- `src/playsim/p_user.cpp` **FPredictionData::RestoreWeaponBob** 597–606 (10 lines)
- `src/playsim/p_user.cpp` **FPredictionData::Mark** 608–616 (9 lines)

## Managed

`VmPUserActorRollback.cs`
Managed LOC (tally): 214.
Conversion commit: `42b74e16b9e55eacf072a6f8cf7d50c39f9c9d1b`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|----------------------|
| `ObjectBackup` | `FObjectBackup` **395–404** |
| `ActorBackup` | `FActorBackup` **416–500** |
| `PredictionRollbackState` | `FPredictionData` **539–616** |

**Post-review ratio:** 214/154 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserActorRollbackTests.PostBackup_ClearsPickupAndPushwallFlags`
- `VmPUserActorRollbackTests.CaptureRestoreWeaponPredictState_RoundTrips`
- `VmPUserActorRollbackTests.RestoreWeaponBob_RestoresPlayerSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 818 --to 818` (0 hard / 0 soft).

## Seams

`FPhysicsLinkBackup::Restore`, `GC::Mark`, and live `AActor` world links are injected; avoids overlap with chunk 815 physics capture and chunk 778 `CheckSkin`.

## Credit rationale

First tally credit for actor/object rollback snapshots used during prediction rewind (pickup/pushwall flags, stat/tid relink, weapon bob).
