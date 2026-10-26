# Chunk 872 — `FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); hub/skill natives unregistered. Retired (2026-10-23): out-of-body credit removed; honest **115** < **150**.

## Scope

`FParser::SF_SectorColormap` … `SF_CloseDoor` from `fragglescript/t_func.cpp` (1876–2038); hub/skill natives unregistered. Retired (2026-10-23): out-of-body credit removed; honest **115** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c05e36985143988febe415ea3d6a8ba479262acd`.

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

### Out-of-body uncredit (872)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **38** |
| **Honest** | **115** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 872 --to 872` (0 hard / 0 soft).

## Seams

Colormap native no-op; ceiling/doors use sector tag iteration and `LineSpecials.ExecuteDoorSpecial`. `SF_ChangeHubLevel` / `SF_StartSkill` stay off `Reg872`.

## Credit rationale

Re-land of retired **870** band on the only contiguous ≥150-line uncredited island after **871**; no door-speed helper extractions.
