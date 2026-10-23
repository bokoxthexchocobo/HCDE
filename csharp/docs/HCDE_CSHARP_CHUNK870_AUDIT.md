# Chunk 870 — `FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); `changehublevel` / `startskill` not registered (same policy as **851** `movecamera`).

## Scope

`FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); `changehublevel` / `startskill` not registered (same policy as **851** `movecamera`).

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

`VmFScriptParserSfTexDoor.cs`, `VmFScriptInitFunctionsReg870.cs`
Managed LOC (tally): 77.
Conversion commit: `3d18f066fcfdda8a30405e112cbbf4e916db949f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Registered |
|---------------|-----------------|------------|
| `SF_SectorColormap` | yes (native no-op) | `sectorcolormap` |
| `SF_CeilingTexture` | yes | `ceilingtexture` |
| `SF_OpenDoor` / `SF_CloseDoor` | yes | `opendoor` / `closedoor` |

**Not in this chunk (zero credit, not registered)**

| Native | Policy |
|--------|--------|
| `SF_ChangeHubLevel` / `SF_StartSkill` (**1973–1982**) | Unregistered until real port — no `script_error` stub |

| Managed | Native |
|---------|--------|
| `SfSectorColormap` | **1883–1922** |
| `SfCeilingTexture` | **1931–1965** |
| `SfOpenDoor` | **1992–2013** |
| `SfCloseDoor` | **2021–2038** |
| `VmFScriptInitFunctionsReg870.Register` | `NewFunction` rows for ported names only |
| (separators) | **1876–1882**, **1923–1930**, **1966–1972**, **1983–1991**, **2014–2020** |
| (unregistered band) | **1973–1982** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **153** |

**Post-review ratio:** 77/153 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTexDoorTests.SfSectorColormap_IsNoOp`
- `VmFScriptParserSfTexDoorTests.SfCeilingTexture_SetsAndReturnsName`
- `VmFScriptParserSfTexDoorTests.SfOpenDoor_StartsDoorMotion`
- `VmFScriptParserSfTexDoorTests.SfCloseDoor_StartsClosingDoor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 870 --to 870` (0 hard / 0 soft).

## Seams

Colormap matches native no-op; ceiling flats set `LevelSector.CeilingPic`; doors via `LineSpecials.ExecuteDoorSpecial` (10/11/12). No `script_error` placeholder registrations.

## Credit rationale

Texture/door band after **869**; hub/skill `NewFunction` rows stay off `BuildTable` / `Reg870` until a later chunk ports real behavior.
