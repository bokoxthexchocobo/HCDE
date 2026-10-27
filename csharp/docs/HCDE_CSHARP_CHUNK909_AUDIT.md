# Chunk 909 — `FParser::SF_SectorColormap` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (**1876–2038**, **2058–2070**, **2115–2239**). Re-lands retired **872**/**871** at honest **301** LOC; skips unhosted `runcommand` / `changemusic` (**2047–2105**).

## Scope

`FParser::SF_SectorColormap` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (**1876–2038**, **2058–2070**, **2115–2239**). Re-lands retired **872**/**871** at honest **301** LOC; skips unhosted `runcommand` / `changemusic` (**2047–2105**).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SectorColormap** 1876–2038 (163 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LineTrigger** 2058–2070 (13 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetLineBlocking** 2115–2239 (125 lines)

**Credited native LOC:** 301 (tally `nativeLoc` 301).

## Managed

`VmFScriptParserSfTexDoor.cs`, `VmFScriptParserSfLineOps.cs`, `VmFScriptInitFunctionsReg872.cs`, `VmFScriptInitFunctionsReg871.cs`
Managed LOC (tally): 207.
Conversion commit: `a2b536e6cfc347c5155d484733728d16f77be841`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SectorColormap` | yes (no-op) |
| `SF_CeilingTexture` | yes |
| `SF_ChangeHubLevel` / `SF_StartSkill` | yes (`script_error`) |
| `SF_OpenDoor` / `SF_CloseDoor` | yes |
| `SF_LineTrigger` | yes |
| `SF_SetLineBlocking` / `SF_SetLineMonsterBlocking` | yes |
| `SF_SetLineTexture` | yes |

**Deferred (zero credit)**

| Native | Lines |
|--------|------:|
| `SF_RunCommand` | **2047–2050** |
| `FS_ChangeMusic` / `SF_ChangeMusic` | **2077–2105** |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfSectorColormap` | **1883–1922** |
| `SfCeilingTexture` | **1931–1965** |
| `SfChangeHubLevel` | **1973–1976** |
| `SfStartSkill` | **1979–1982** |
| `SfOpenDoor` | **1992–2013** |
| `SfCloseDoor` | **2021–2038** |
| `SfLineTrigger` | **2058–2070** |
| `SfSetLineBlocking` | **2115–2134** |
| `SfSetLineMonsterBlocking` | **2142–2156** |
| `SfSetLineTexture` | **2172–2239** |
| (separator) | **1876–1882** |
| (separator) | **1923–1930** |
| (separator) | **1966–1972** |
| (separator) | **1983–1991** |
| (separator) | **2014–2020** |
| (separator) | **2071–2076** |
| (separator) | **2106–2114** |
| (separator) | **2157–2171** |

### Chunk **909** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **301** |
| **Net headline** | **+301** vs post-**908** |


**Post-review ratio:** 207/301 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTexDoorTests.SfCeilingTexture_SetsAndReturnsName`
- `VmFScriptParserSfTexDoorTests.SfOpenDoor_StartsDoorMotion`
- `VmFScriptParserSfTexDoorTests.SfCloseDoor_StartsClosingMotion`
- `VmFScriptParserSfLineOpsTests.SfLineTrigger_CallsFloorSpecial`
- `VmFScriptParserSfLineOpsTests.SfSetLineBlocking_SetsBlockingFlag`
- `VmFScriptParserSfLineOpsTests.SfSetLineMonsterBlocking_SetsMonsterBlock`
- `VmFScriptParserSfLineOpsTests.SfSetLineTexture_EternityLayout_UpdatesMid`
- `VmFScriptParserSfLineOpsTests.SfSetLineTexture_LegacyLayout_UpdatesTopAndBottom`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 909 --to 909` (0 hard / 0 soft).

## Seams

Colormap native no-op; doors via `LineSpecials.ExecuteDoorSpecial`; line specials via `ActorSpecialActions` / `LevelLine` flags; textures via `LevelSide` pics. `FS_EmulateCmd` / `FS_ChangeMusic` not hosted.

## Credit rationale

Function-bounded UNCredited band after **908** / **869**; hub/skill `script_error` stubs match native; gap **2039–2057** + **2071–2114** left for future console/music hosts.
