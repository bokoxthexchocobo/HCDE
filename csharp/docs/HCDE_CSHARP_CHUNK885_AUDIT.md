# Chunk 885 — Retired (2026-10-23): honest function-boundary bands on `b_func.cpp` **454–585** sum **114** native (`SetBodyAt`, `FakeFire`, `FireRox`, `SafeCheckPosition`, `StartTravel`, `FinishTravel`) — below **150** `check_port_coverage` floor. Stretched hull **450–600** (**151**) pads blank/separator lines **450–453**, **479–489**, **552–555**, **564**; not landed. Off-tally `VmBBotFuncReachFire` + `VmBBotGame.StartTravel`/`FinishTravel` updated for fidelity.

## Scope

Retired (2026-10-23): honest function-boundary bands on `b_func.cpp` **454–585** sum **114** native (`SetBodyAt`, `FakeFire`, `FireRox`, `SafeCheckPosition`, `StartTravel`, `FinishTravel`) — below **150** `check_port_coverage` floor. Stretched hull **450–600** (**151**) pads blank/separator lines **450–453**, **479–489**, **552–555**, **564**; not landed. Off-tally `VmBBotFuncReachFire` + `VmBBotGame.StartTravel`/`FinishTravel` updated for fidelity.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`ActorSimulation.cs`, `ThinkerCollection.cs`, `VmBBotFuncReachFire.cs`, `VmBBotGame.cs`, `VmBBotGameTests.cs`
Managed LOC (tally): 0.
Conversion commit: `038ae0fb9f8f466dd7bdd574e5719af3b8e77149`.

## Padding review (2026-10-21)

### Honest function bands (no separator top-up)

| Symbol | Lines | Lines |
|--------|------:|------:|
| `FCajunMaster::SetBodyAt` | **454–478** | 25 |
| `FCajunMaster::FakeFire` | **490–510** | 21 |
| `DBot::FireRox` | **512–551** | 40 |
| `FCajunMaster::SafeCheckPosition` | **556–563** | 8 |
| `FCajunMaster::StartTravel` | **565–574** | 10 |
| `FCajunMaster::FinishTravel` | **576–585** | 10 |
| **Sum (nativeLoc)** | **454–585** | **114** |

### Behavior ported (matrix credit)

| Native | Behavior ported (off-tally) | Matrix credit |
|--------|------------------------------|---------------|
| `SetBodyAt` | **yes** (`VmBBotFuncReachFire.SetBodyAt`) | **no** (retired) |
| `FakeFire` | **yes** (trace spawn + `CleanAhead`) | **no** |
| `FireRox` | **yes** (`PosPlusZ` + vel×5, `ONFLOORZ` predict) | **no** |
| `SafeCheckPosition` | **partial** (`TryMove`, not full `P_CheckPosition`) | **no** |
| `StartTravel` / `FinishTravel` | **yes** (`CajunBotThinker.ChangeStatNum`) | **no** |
| `Reachable` / `Dofire` (**883** bands) | remain off-tally on **875** callers | **no** |

### Padding map (dishonest hull **450–600** — not credited)

| Native lines | Reason |
|--------------|--------|
| **450–453** | blank separators after `Find_enemy` (**879** ends **449**) |
| **479–489** | blanks + `FireRox` comment block outside function bodies |
| **552–555** | blanks between `FireRox` and `SafeCheckPosition` |
| **564** | blank before `StartTravel` |
| **586+** | EOF (file ends **585**) |

### Gross vs net (**885**)

| | Value |
|--|------:|
| Gross honest native | **114** |
| Gross padded hull **450–600** | **151** (invalid — separator padding) |
| Net behavior-ported (matrix) | **0** |

### Clarification: “passes only when excluding 882”

That phrase referred to **`uncredited_range_check.py --chunk 882 --band`**: the tool skips **only the chunk-under-test’s own tally row** (`self_chunk_name`) so a row can validate its band does not overlap *other* rows. It does **not** exempt **882** from overlap checks on **885**. **`b_game.cpp`** lifecycle credit stays on **882** only; **885** must not claim any **`b_game.cpp`** line.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 885 --to 885` (0 hard / 0 soft).

## Seams

Headless Cajun uses `PlayerPawn` + `CajunBotThinker` (`STAT_BOT` **104**) instead of separate `player->mo` / `DBot` pair; `ChangeStatNum` matches native travel stat moves. `SafeCheckPosition` still uses `ActorPhysics.TryMove` (native `P_CheckPosition` + `MF_PICKUP` clear) until `VmPMapCheckPosition` is wired on `AuthoritySimulation`. `FakeFire` spawns Cajun trace aux actor + `VmBBotMove.CleanAhead` missile path.

## Credit rationale

User-approved **450–600** hull only when function boundaries are honest and `uncredited_range_check --chunk 885 --band` passes all tally rows (**879**, **882**, …) with nothing excluded; honest credit **114** &lt; **150** → defer per standing rules (same pattern as retired **438**).
