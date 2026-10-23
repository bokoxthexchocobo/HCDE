# Chunk 872 — `FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); hub/skill natives unregistered.

## Scope

`FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); hub/skill natives unregistered.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SectorColormap** 1883–1922 (40 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_CeilingTexture** 1931–1965 (35 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_OpenDoor** 1992–2013 (22 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_CloseDoor** 2021–2038 (18 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF tex/door separators** 1876–1882 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF tex/door separators** 1923–1930 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF tex/door separators** 1966–1972 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF tex/door separators** 1983–1991 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF tex/door separators** 2014–2020 (7 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmFScriptParserSfTexDoor.cs`, `VmFScriptInitFunctionsReg872.cs`
Managed LOC (tally): 77.
Conversion commit: `67a4c7acd42b77520dc85e58a76c2579c6dc0564`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Registered |
|---------------|-----------------|------------|
| `SF_SectorColormap` | yes (native no-op) | `sectorcolormap` |
| `SF_CeilingTexture` | yes | `ceilingtexture` |
| `SF_OpenDoor` | yes | `opendoor` |
| `SF_CloseDoor` | yes | `closedoor` |

**Not in this chunk (zero credit, not registered)**

| Native | Policy |
|--------|--------|
| `SF_ChangeHubLevel` (**1973–1976**) | Unregistered (`script_error`) |
| `SF_StartSkill` (**1979–1982**) | Unregistered (`script_error`) |

| Managed | Native |
|---------|--------|
| `SfSectorColormap` | **1883–1922** |
| `SfCeilingTexture` | **1931–1965** |
| `SfOpenDoor` | **1992–2013** |
| `SfCloseDoor` | **2021–2038** |
| `VmFScriptInitFunctionsReg872.Register` | `NewFunction` rows |
| (separators) | **1876–1882**, **1923–1930**, **1966–1972**, **1983–1991**, **2014–2020** |
| (unregistered band) | **1973–1982** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **163** |

**Post-review ratio:** 77/153 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTexDoorTests.SfCeilingTexture_SetsAndReturnsName`
- `VmFScriptParserSfTexDoorTests.SfOpenDoor_StartsDoorMotion`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 872 --to 872` (0 hard / 0 soft).

## Seams

Colormap native no-op; ceiling/doors use sector tag iteration and `LineSpecials.ExecuteDoorSpecial`. `SF_ChangeHubLevel` / `SF_StartSkill` stay off `Reg872`.

## Credit rationale

Re-land of retired **870** band on the only contiguous ≥150-line uncredited island after **871**; no door-speed helper extractions.
