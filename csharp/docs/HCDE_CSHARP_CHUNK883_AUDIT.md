# Chunk 883 — Cajun `Reachable`, `Dofire`, `SetBodyAt`, `FakeFire`, `FireRox`, and `SafeCheckPosition` from `b_func.cpp` (disjoint from **879**).

## Scope

Cajun `Reachable`, `Dofire`, `SetBodyAt`, `FakeFire`, `FireRox`, and `SafeCheckPosition` from `b_func.cpp` (disjoint from **879**).

## Native ranges

- `src/playsim/bots/b_func.cpp` **DBot::Reachable** 45–124 (80 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Dofire** 151–266 (116 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::SetBodyAt** 454–478 (25 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::FakeFire** 490–510 (21 lines)
- `src/playsim/bots/b_func.cpp` **DBot::FireRox** 512–551 (40 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::SafeCheckPosition** 556–563 (8 lines)

**Credited native LOC:** 290 (tally `nativeLoc` 290).

## Managed

`VmBBotFuncReachFire.cs`
Managed LOC (tally): 415.
Conversion commit: `b39ac64732430e28a22ecc22ce27e8d96225b471`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Reachable` | **yes** |
| `DBot::Dofire` | **yes** |
| `FCajunMaster::SetBodyAt` | **yes** |
| `FCajunMaster::FakeFire` | **yes** (`SafeCheckPosition` stepping only; no **875** `CleanAhead` `Func`) |
| `DBot::FireRox` | **yes** |
| `FCajunMaster::SafeCheckPosition` | **yes** |
| `FCajunMaster::BotTick` (**284–322**) | **no** (deferred) |
| `pr_botdofire` static (**41**) | **no** (RNG via `Func<int>` at call sites) |

| Managed | Native (`b_func.cpp`) |
|---------|----------------------|
| `Reachable` | **45–124** |
| `Dofire` | **151–266** |
| `SetBodyAt` | **454–478** |
| `FakeFire` | **490–510** |
| `FireRox` | **512–551** |
| `SafeCheckPosition` | **556–563** |

**Padding map (retired / not credited)**

| Native lines | Reason |
|--------------|--------|
| **41** (`pr_botdofire`) | FRandom decl; tests pass `Func<int>` |
| **125–150**, **267–449** | credited to **879** |
| **479–489**, **564+** | comments / unrelated tail |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **295** |
| Net behavior-ported | **295** |

**Post-review ratio:** 415/290 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncReachFireTests.Reachable_TrueInOpenRoom`
- `VmBBotFuncReachFireTests.Reachable_FalseThroughOneSidedWall`
- `VmBBotFuncReachFireTests.Dofire_SetsAttackWhenMeleeInRangeAndLos`
- `VmBBotFuncReachFireTests.Dofire_WaitsWhileReactionTimerRuns`
- `VmBBotFuncReachFireTests.SafeCheckPosition_IgnoresPickupSideEffects`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 883 --to 883` (0 hard / 0 soft).

## Seams

Direct `FPathTraverse`-style intercept walk on `AuthoritySimulation.Level`; `VmBBotFunc.CheckLos`; `ActorPhysics.TryMove` for `SafeCheckPosition` / `FakeFire` stepping. Production: `AuthoritySimulation.CajunReachable` / `CajunDoFire` with headless `CajunMaster` bind. No `Func` fields on **875** / **706**.

## Credit rationale

Second `b_func.cpp` band: movement aim/fire callees deferred from **879** so `ThinkForMove` / `b_think` can land later.

## Uncredited gate (`uncredited_range_check.py`) — corrective (2026-10-23)

**883 land incorrectly changed `--band`** in commit `b39ac647` (reverted at HEAD). Full diff of that mistake:

```diff
--- a/csharp/tools/uncredited_range_check.py
+++ b/csharp/tools/uncredited_range_check.py
@@ -98,17 +98,17 @@ def main() -> int:
-            ranges.append((band_file, min(...), max(...), "cli"))  # single hull
+            for r in rs:
+                ranges.append((r["file"], int(r["start"]), int(r["end"]), r.get("symbol", "cli")))
```

**Why that was wrong:** `--band` is documented as one CLI hull (`min start`–`max end`). Replacing it with per-row iteration made `--band` identical to `--chunk` without `--band`, so a hull that should fail (gap lines credited to **879** inside **45–563**) would pass.

**Not a loosening — land gate semantics:** `land_chunk_fast.sh` now calls `uncredited_range_check.py --chunk N` **without** `--band`. Each `nativeRanges` row is checked as a whole interval `[start,end]` against **every** live credited row (same loop as always: `overlaps(start,end,cs,ce)` on lines **130–151**). Every line in each claimed band is still gated.

| Check | Exit | Meaning |
| --- | ---: | --- |
| `--chunk 883` (land gate) | **0** | All six rows uncredited |
| `--file b_func.cpp --start 134 --end 144` | **1** | Overlaps **879** `CheckLos` |
| `--chunk 883 --band` (hull only) | **1** | Hull **45–563** overlaps **879** in gaps **883 does not claim** |

**`UncreditedRangeCheckGateTests`:** `Chunk883_EachNativeRangeRowIsUncredited`, `OverlappingBandStillFails`, `BandEnvelopeFlagsGapCreditBetween883Bands`, `LandGateUsesPerNativeRangeRowsNotEnvelope`.

### `nativeRanges` integrity (whole functions, no credited lines inside)

| Symbol | Lines | Contiguous native | Credited lines inside band |
| --- | --- | --- | --- |
| `DBot::Reachable` | **45–124** | `Reachable` body **45–124** | **none** |
| `DBot::Dofire` | **151–266** | `Dofire` **151–266** (after **879** `CheckLos` **134–144**) | **none** |
| `FCajunMaster::SetBodyAt` | **454–478** | **454–478** | **none** |
| `FCajunMaster::FakeFire` | **490–510** | **490–510** | **none** |
| `DBot::FireRox` | **512–551** | **512–551** | **none** |
| `FCajunMaster::SafeCheckPosition` | **556–563** | **556–563** | **none** |

Gaps **125–150**, **267–449**, **479–489**, **564+** are **879** or uncredited tail — not stitched into **883** credit.
