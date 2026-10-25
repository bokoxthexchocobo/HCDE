# Chunk 836 — `DFraggleThinker`, `T_PreprocessScripts`, and `T_RunScript` from `fragglescript/t_script.cpp` (344–628).

## Scope

`DFraggleThinker`, `T_PreprocessScripts`, and `T_RunScript` from `fragglescript/t_script.cpp` (344–628).

## Native ranges

- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::DFraggleThinker** 344–349 (6 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::Construct** 357–364 (8 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::OnDestroy** 372–392 (21 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::Serialize** 400–407 (8 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::wait_finished** 415–466 (52 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::Tick** 474–516 (43 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::PropagateMark** 526–533 (8 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::PointerSubstitution** 542–554 (13 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFraggleThinker::AddRunningScript** 562–578 (17 lines)
- `src/playsim/fragglescript/t_script.cpp` **T_PreprocessScripts** 586–600 (15 lines)
- `src/playsim/fragglescript/t_script.cpp` **T_RunScript** 608–628 (21 lines)

**Credited native LOC:** 212 (tally `nativeLoc` 212).

## Managed

`VmFScriptThinker.cs`
Managed LOC (tally): 246.
Conversion commit: `134114ab58b62943644134885b9a908ddf6906b5`.

## Padding review (2026-10-21)

| Managed | Native (`t_script.cpp`) |
|---------|-------------------------|
| `ConstructThinker` | **344–349** |
| `DFraggleThinker::Construct` | **357–364** |
| `DFraggleThinker::OnDestroy` | **372–392** |
| `DFraggleThinker::Serialize` | **400–407** |
| `WaitFinished` | **414–466** |
| `Tick` | **474–516** |
| `PropagateMark` | **526–533** |
| `PointerSubstitution` | **542–554** |
| `AddRunningScript` | **562–578** |
| `PreprocessScripts` | **586–600** |
| `RunScript` | **608–628** |

### Out-of-body uncredit (836)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **213** |
| **Out-of-body removed** | **1** |
| **Honest** | **212** |


**Post-review ratio:** 246/212 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptThinkerTests.WaitFinished_Delay_CountsDown`
- `VmFScriptThinkerTests.RunScript_QueuesChildScript`
- `VmFScriptThinkerTests.PropagateMark_MarksSpawnedThenSuper`
- `VmFScriptThinkerTests.PointerSubstitution_ReplacesSpawnedAndAddsSuperCount`
- `VmFScriptThinkerTests.RunScript_InvalidNum_ReturnsFalse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 836 --to 836` (0 hard / 0 soft).

## Seams

GC, sector tag iteration, preprocess, and `ParseScript` continuation are injected; `CCMD(fpuke)` deferred.

## Credit rationale

Thinker tick/wait queue and script start API after **835** script objects.
