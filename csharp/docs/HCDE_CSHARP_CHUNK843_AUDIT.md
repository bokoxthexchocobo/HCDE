# Chunk 843 — `FParser::SF_Max` … `SF_IsPlayerObj` from `fragglescript/t_func.cpp` (2242–2393).

## Scope

`FParser::SF_Max` … `SF_IsPlayerObj` from `fragglescript/t_func.cpp` (2242–2393).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Max** 2248–2258 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Min** 2267–2277 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Abs** 2286–2296 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Gameskill** 2308–2312 (5 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Gamemode** 2320–2333 (14 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_IsPlayerObj** 2345–2360 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2242–2247 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2259–2266 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2278–2285 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2297–2307 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2313–2319 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math separators** 2334–2344 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF math inventory preamble** 2361–2393 (33 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmFScriptParserSfMath.cs`
Managed LOC (tally): 76.
Conversion commit: `d4e319648cad965d97c786ac7a0ebe407c66ebf5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_Max` | yes |
| `SF_Min` | yes |
| `SF_Abs` | yes |
| `SF_Gameskill` | yes (`Simulation.Skill` ACS-style return + 1) |
| `SF_Gamemode` | yes (`Simulation.GameMode` → 0/1/2) |
| `SF_IsPlayerObj` | yes |

| Managed | Native (`t_func.cpp` 2242–2393) |
|---------|-------------------------|
| `FsFixValue` | **58–63** (`fixedvalue` in `t_variable.cpp`) |
| `SfMax` | **2248–2258** |
| `SfMin` | **2267–2277** |
| `SfAbs` | **2286–2296** |
| `SfGameskill` | **2308–2312** |
| `SfGamemode` | **2320–2333** |
| `SfIsPlayerObj` | **2345–2360** |
| (separators / inventory preamble) | **2242–2247**, **2259–2266**, **2278–2285**, **2297–2307**, **2313–2319**, **2334–2344**, **2361–2393** |

**Post-review ratio:** 76/152 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfMathTests.SfMax_PicksLargerFixed`
- `VmFScriptParserSfMathTests.SfMin_PicksSmallerFixed`
- `VmFScriptParserSfMathTests.SfAbs_NegatesNegative`
- `VmFScriptParserSfMathTests.SfGameskill_ReturnsSkillPlusOne`
- `VmFScriptParserSfMathTests.SfGamemode_DeathmatchIsTwo`
- `VmFScriptParserSfMathTests.SfIsPlayerObj_TriggerPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 843 --to 843` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. In-band `fixedvalue` coercion matches `t_variable.cpp`; `gameskill` / `gamemode` read `AuthoritySimulation.Skill` and `GameMode`; `isplayerobj` uses `PlayerPawn` / `PlayerNum`.

## Credit rationale

First honest full-port band after **842** while **1381–2241** (camera, sector motion, doors, lights, textures) stays uncredited until level/playsim hooks exist without seams.
