# Chunk 910 — `FParser::SF_Max` … `SF_IsPlayerObj` from `fragglescript/t_func.cpp` (**2240–2393**). Stops before `SF_PlayerKeys` (**2394**, chunk **844** inventory band).

## Scope

`FParser::SF_Max` … `SF_IsPlayerObj` from `fragglescript/t_func.cpp` (**2240–2393**). Stops before `SF_PlayerKeys` (**2394**, chunk **844** inventory band).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Max** 2240–2393 (154 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmFScriptParserSfMath.cs`
Managed LOC (tally): 78.
Conversion commit: `de8050a8ee9246b90b79dbc7424336a00c4b09fd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_Max` / `SF_Min` / `SF_Abs` | yes |
| `SF_Gameskill` / `SF_Gamemode` | yes |
| `SF_IsPlayerObj` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfMax` | **2248–2258** |
| `SfMin` | **2267–2277** |
| `SfAbs` | **2286–2296** |
| `SfGameskill` | **2308–2312** |
| `SfGamemode` | **2320–2333** |
| `SfIsPlayerObj` | **2345–2360** |
| (separator) | **2240–2247** |
| (separator) | **2259–2266** |
| (separator) | **2278–2285** |
| (separator) | **2297–2307** |
| (separator) | **2314–2319** |
| (separator) | **2335–2344** |
| (inventory header) | **2361–2393** |

### Chunk **910** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **154** |
| **Net headline** | **+154** vs post-**909** |


**Post-review ratio:** 78/154 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfMathTests.SfMax_PicksLargerFixed`
- `VmFScriptParserSfMathTests.SfMin_PicksSmallerFixed`
- `VmFScriptParserSfMathTests.SfAbs_NegatesNegative`
- `VmFScriptParserSfMathTests.SfGameskill_ReturnsSkillPlusOne`
- `VmFScriptParserSfMathTests.SfGamemode_DeathmatchIsTwo`
- `VmFScriptParserSfMathTests.SfIsPlayerObj_TriggerPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 910 --to 910` (0 hard / 0 soft).

## Seams

`FsFixValue` mirrors native `fixedvalue`; skill/mode from `AuthoritySimulation`; `IsPlayerObj` uses `PlayerPawn` / `PlayerNum`. Inventory ACS forwards deferred at **2394+**.

## Credit rationale

Honest **154**-line UNCredited slice after **909**; off-tally `VmFScriptParserSfMath.cs` credited without overlapping **844**.
