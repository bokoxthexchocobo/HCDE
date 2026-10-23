# Chunk 850 — `FParser::SF_Wait` … `SF_ScriptRunning` from `fragglescript/t_func.cpp` (3850–4008).

## Scope

`FParser::SF_Wait` … `SF_ScriptRunning` from `fragglescript/t_func.cpp` (3850–4008).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Wait** 3850–3866 (17 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_TagWait** 3874–3889 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ScriptWait** 3897–3912 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ScriptWaitPre** 3920–3934 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_StartScript** 3942–3973 (32 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ScriptRunning** 3981–4008 (28 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF wait separators** 3867–3873 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF wait separators** 3890–3896 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF wait separators** 3913–3919 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF wait separators** 3935–3941 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF wait separators** 3974–3980 (7 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmFScriptParserSfGzd.cs`
Managed LOC (tally): 203.
Conversion commit: `13e203de8fd2cb4a900a39241640c014ec407741`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_Wait` | yes |
| `SF_TagWait` | yes |
| `SF_ScriptWait` | yes |
| `SF_ScriptWaitPre` | yes |
| `SF_StartScript` | yes |
| `SF_ScriptRunning` | yes |

| Managed | Native (`t_func.cpp` 3850–4008) |
|---------|-------------------------|
| `SfWait` | **3850–3866** |
| `SfTagWait` | **3874–3889** |
| `SfScriptWait` | **3897–3912** |
| `SfScriptWaitPre` | **3920–3934** |
| `SfStartScript` | **3942–3973** |
| `SfScriptRunning` | **3981–4008** |
| (separators) | **3867–3873**, **3890–3896**, **3913–3919**, **3935–3941**, **3974–3980** |

**Post-review ratio:** 203/159 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfWaitTests.SfWait_SchedulesDelay`
- `VmFScriptParserSfWaitTests.SfTagWait_SchedulesTag`
- `VmFScriptParserSfWaitTests.SfScriptWait_SchedulesScript`
- `VmFScriptParserSfWaitTests.SfScriptWaitPre_SchedulesScript`
- `VmFScriptParserSfWaitTests.SfStartScript_QueuesChild`
- `VmFScriptParserSfWaitTests.SfScriptRunning_DetectsRunner`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 850 --to 850` (0 hard / 0 soft).

## Seams

Uses `SaveCurrentScript`, `VmFScriptThinker.AddRunningScript`, and `FsTerminatorException` for script yield.

## Credit rationale

Script wait/start band after **849** `SaveCurrentScript`; **4017+** `InitFunctions` in **851**.
