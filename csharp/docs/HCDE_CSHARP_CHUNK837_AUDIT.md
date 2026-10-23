# Chunk 837 — `FParser` type helpers and early `SF_*` from `fragglescript/t_func.cpp` (219–501).

## Scope

`FParser` type helpers and early `SF_*` from `fragglescript/t_func.cpp` (219–501).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::T_GetMobjType** 219–245 (27 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::T_GetPlayerNum** 253–280 (28 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::T_GetPlayerActor** 282–286 (5 lines)
- `src/playsim/fragglescript/t_func.cpp` **T_ClassType** 288–291 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **T_GetAmmo** 300–329 (30 lines)
- `src/playsim/fragglescript/t_func.cpp` **T_FindSound** 338–361 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::GetFormatString** 370–375 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::CheckArgs** 377–385 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Print** 408–411 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Rnd** 419–423 (5 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::looping_section** 432–463 (32 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Continue** 471–482 (12 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Break** 490–501 (12 lines)

**Credited native LOC:** 198 (tally `nativeLoc` 198).

## Managed

`VmFScriptParser.cs`
Managed LOC (tally): 223.
Conversion commit: `23be4b60fc4e16038a9013b4d8f4ccf5aff27586`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `GetMobjType` | **219–245** |
| `GetPlayerNum` | **253–280** |
| `GetPlayerActor` | **282–286** |
| `ClassType` | **288–291** |
| `GetAmmo` | **300–329** |
| `FindSound` | **338–361** |
| `GetFormatString` | **370–375** |
| `CheckArgs` | **377–385** |
| `SfPrint` | **408–411** |
| `SfRnd` | **419–423** |
| `LoopingSection` | **432–463** |
| `SfContinue` | **471–482** |
| `SfBreak` | **490–501** |

**Post-review ratio:** 223/198 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserTests.GetPlayerNum_NonPlayerMobj_ReturnsMinusOne`
- `VmFScriptParserTests.CheckArgs_Insufficient_ReturnsFalse`
- `VmFScriptParserTests.LoopingSection_PicksInnermostLoop`
- `VmFScriptParserTests.SfBreak_AdvancesRoverPastLoopEnd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 837 --to 837` (0 hard / 0 soft).

## Seams

`PClass::FindActor`, level player roster, sound table, and `script_error` are host callbacks; no `FParser` runtime VM.

## Credit rationale

Whole-function band for mobj/player resolution, sound lookup, and loop control before mid-file `SF_*` spawn API (**838+**).
