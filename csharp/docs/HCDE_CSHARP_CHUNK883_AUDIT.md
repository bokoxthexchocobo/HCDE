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
