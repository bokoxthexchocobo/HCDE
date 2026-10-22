# Chunk 798 — `DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095).

## Scope

`DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::Tick** 114–225 (112 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::SetFloorChangeType** 233–251 (19 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::StartFloorSound** 259–262 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::StartFloorSound** 992–995 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoElevator** 1010–1095 (86 lines)

**Credited native LOC:** 225 (tally `nativeLoc` 225).

## Managed

`VmAFloorTickElevator.cs`
Managed LOC (tally): 363.
Conversion commit: `55b4f49b26a7477a0a1d77a6153c9f3001a5068f`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `Tick` | `DFloor::Tick` **114–225** |
| `SetFloorChangeType` | `DFloor::SetFloorChangeType` **233–251** |
| `StartFloorSound` | `DFloor::StartFloorSound` **259–262** |
| `StartElevatorFloorSound` | `DElevator::StartFloorSound` **992–995** |
| `EvDoElevator` | `FLevelLocals::EV_DoElevator` **1010–1095** |


**Post-review ratio:** 363/225 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorTickElevatorTests.Tick_PastDest_ClearsFloorDataAndDestroys`
- `VmAFloorTickElevatorTests.Tick_BuildStairPastDest_BecomesWaitStair`
- `VmAFloorTickElevatorTests.SetFloorChangeType_ModelZero_SetsGenFloorChg0`
- `VmAFloorTickElevatorTests.EvDoElevator_Raise_SetsDestinations`
- `VmAFloorTickElevatorTests.EvDoElevator_CurrentWithoutLine_ReturnsFalse`

## Verified

`bash csharp/tools/verify_full_solution_tests.sh` at green HEAD (build **0** CS errors, **14/14** parsed `Passed!` lines; Playsim **18806** tests in 2026-10-22 re-gate). `python3 tools/reaudit_chunks.py audit --from 798 --to 798` (0 hard / 0 soft).

## Test gate incident (red commit `b2c9e283`)

Tally commit **`b2c9e283`** added `Tick_BuildStairPastDest_BecomesWaitStair` using nonexistent `VmAFloorTickElevator.TickContext` (correct: `FloorTickContext`). **`land_chunk_fast.sh` ran verify before the tally commit**, so origin briefly reported green while HEAD did not compile. Hardened verify (`PIPESTATUS`, CS error count, **14** parsed assemblies) fails on `b2c9e283` (1× CS0426). Fixed at HEAD in a follow-up commit (no amend).

## Helper chunks 799–801 (out of scope; reverted)

| Commit | Author | Content | Hardened gate? | Review |
| --- | --- | --- | --- | --- |
| `e4d6754b` | Cursor Agent | **799** `VmAFloorCreate` / CreateFloor 281–538 | **No** (land ordering + pre-harden script) | **Not accepted** — wrong scope vs locked 798 five-function plan |
| `96b6a1f0` / `d9aa1475` | Cursor Agent | **800** `VmPEnemyLookAction` | **No** | **Reverted** (`25188103`, `358053f3`) |
| `398f9e18` | Cursor Agent | **801** smart assist / Massacre | **No** | **Reverted** (`ae9518e1`) |

Sub-agent **`bc-1ff873dc-1f87-545b-9ef6-33982fc97016`** could not be stopped via Task (still “running”); stray `dotnet test` PIDs and **`land-chunks-596-601`** tmux were killed. Helper re-land **`a79227a9`** (799–801 tally + sources) **reverted** at **`b64eacd4`**.

## Re-gate proof (2026-10-22)

- Intentional scratch failure → verify exit **1** (`/opt/cursor/artifacts/gate_proof_fail.log`).
- Clean HEAD → exit **0**, parsed **14/14** (`/opt/cursor/artifacts/gate_proof_pass.log`).
- Historical: **`b5c017e6`** and **`ffde316c`** pass hardened verify (Playsim **18773** / **18799**); **`b2c9e283`** fails build (TickContext).

## Seams

`MoveFloor`, stair sector chain, tag iterators, height finders, and `CreateThinker` are injected.

## Credit rationale

Floor mover tick/texture tail and parallel elevator EV after donut/waggle/elevator tick (chunk6).
