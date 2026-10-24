# Chunk 835 — `DFsScript` and `DRunningScript` lifecycle from `fragglescript/t_script.cpp` (97–317).

## Scope

`DFsScript` and `DRunningScript` lifecycle from `fragglescript/t_script.cpp` (97–317).

## Native ranges

- `src/playsim/fragglescript/t_script.cpp` **DFsScript::ClearChildren** 97–105 (9 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFsScript::DFsScript** 113–126 (14 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFsScript::OnDestroy** 134–144 (11 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFsScript::Serialize** 152–165 (14 lines)
- `src/playsim/fragglescript/t_script.cpp` **DFsScript::ParseScript** 175–207 (33 lines)
- `src/playsim/fragglescript/t_script.cpp` **DRunningScript::DRunningScript** 230–271 (42 lines)
- `src/playsim/fragglescript/t_script.cpp` **DRunningScript::OnDestroy** 279–298 (20 lines)
- `src/playsim/fragglescript/t_script.cpp` **DRunningScript::Serialize** 306–317 (12 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmFScriptScript.cs`
Managed LOC (tally): 191.
Conversion commit: `b38f093d2b45417cf8fafda23f16711ce4c2e0f3`.

## Padding review (2026-10-21)

| Managed | Native (`t_script.cpp`) |
|---------|-------------------------|
| `ClearChildren` | **97–105** |
| `DFsScript::DFsScript` | **113–126** |
| `DFsScript::OnDestroy` | **134–144** |
| `DFsScript::Serialize` | **152–165** |
| `DFsScript::ParseScript` | **175–207** |
| `DRunningScript::DRunningScript` | **230–271** |
| `DRunningScript::OnDestroy` | **279–298** |
| `DRunningScript::Serialize` | **306–317** |

**Post-review ratio:** 191/155 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptScriptTests.ClearChildren_DestroyAllSlots`
- `VmFScriptScriptTests.InitNewScript_ClearsArraysAndDefaults`
- `VmFScriptScriptTests.ParseScript_OutOfRange_PrintsAndSkipsBody`
- `VmFScriptScriptTests.InitRunningScript_IndexZero_TrimsToLabels`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 835 --to 835` (0 hard / 0 soft).

## Seams

Parser, serializer, GC barriers, and `ClearVariables`/`ClearSections` are injected; no `FParser` VM.

## Credit rationale

Whole-function band for FraggleScript script objects before thinker tick/run queue (**836**).
