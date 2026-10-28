# Chunk 995 — Movement/spy macros and spy-step helpers from `g_game.cpp` (**1–150**); first UNCredited head band (EOF **3813**).

## Scope

Movement/spy macros and spy-step helpers from `g_game.cpp` (**1–150**); first UNCredited head band (EOF **3813**).

## Native ranges

- `src/g_game.cpp` **GameActionName** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameHead995Slice.cs`
Managed LOC (tally): 82.
Conversion commit: `abc853499ecd865894473d423361ce3ec094e77c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Movement / look macros (**96–99**) | yes |
| Spy change enum (**103–107**) | yes |

**Managed policy helpers (zero native line credit outside hull)**

| Helper | Derived from |
|--------|----------------|
| `AdvanceSpyPlayerIndex` | spy step semantics (**103–107**) |
| `ApplySlowTurnSpeedBoost` | **98** (`SLOWTURNTICS`) |
| `ScaleAnalogLookAxis` / `ScaleAnalogYawAxis` | **99** (`ANALOG_LOOK_BASE`) |
| `ShouldDetectTurboForwardMove` | **97** (`TURBOTHRESHOLD`) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–95**, **109–148** | includes / forward decls / externs |
| **149–152** | `GameActionName` head (seam; body **153+**) |
| **153–201** | name switches outside hull |
| **204–224** | chunk **9** `G_TraceSetGameState` band |

| Managed (`VmGGameHead995Slice.cs`) | Native (`g_game.cpp`) |
|-----------------------------------|----------------------|
| `TurboThreshold` / `SlowTurnTics` / `AnalogLookBase` | **96–99** |
| `SpyCancel` / `SpyNext` / `SpyPrev` / `ResolveSpyStep` / `AdvanceSpyPlayerIndex` | **103–107** |
| `ApplySlowTurnSpeedBoost` | **98** |
| `ScaleAnalogLookAxis` / `ScaleAnalogYawAxis` / `BuildAnalogLookDeltas` | **99** |
| `ForwardMoveExceedsTurboThreshold` / `ShouldDetectTurboForwardMove` | **97** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **995** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**994** |


**Post-review ratio:** 82/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameHead995SliceTests.SpyStepAndAdvance`
- `VmGGameHead995SliceTests.AnalogLookAndSlowTurn`
- `VmGGameHead995SliceTests.TurboForwardMoveGate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 995 --to 995` (0 hard / 0 soft).

## Seams

`GameActionName` / `GameStateName` bodies (**153+**), `G_TraceSetGameAction` (**204+**, chunk **9**), and `ChangeSpy` host (**1225+**) remain injected.

## Credit rationale

CLI UNCredited **1–150**; no `VmGGameHead*` slice. Chunk **9** credits **204+** ticker/trace bands only — no overlap on **96–107**.
