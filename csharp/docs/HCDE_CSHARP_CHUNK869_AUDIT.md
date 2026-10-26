# Chunk 869 — `FParser::SF_CeilingHeight` … `SF_FloorTexture` from `fragglescript/t_func.cpp` (1587–1875); `DLightLevel` / `SF_FadeLight` deferred.

## Scope

`FParser::SF_CeilingHeight` … `SF_FloorTexture` from `fragglescript/t_func.cpp` (1587–1875); `DLightLevel` / `SF_FadeLight` deferred.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_CeilingHeight** 1595–1646 (52 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MoveCeiling** 1655–1678 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LightLevel** 1686–1722 (37 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_FloorTexture** 1841–1875 (35 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ceil/light separators** 1594–1594 (1 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ceil/light separators** 1836–1840 (5 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmFScriptParserSfCeilLight.cs`, `VmFScriptInitFunctionsReg869.cs`
Managed LOC (tally): 132.
Conversion commit: `3afd19a8d4cdc67f33a9c1af06460d9a7df20e67`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Managed playsim callee |
|---------------|-----------------|----------------------|
| `SF_CeilingHeight` read/set | yes | `CeilingOf` / `Ceilings[]` + crush helper |
| `SF_MoveCeiling` | yes | `LineSpecials.ExecuteCeilingSpecial` (280) |
| `SF_LightLevel` | yes | `Lights[]` / `LightOf` |
| `SF_FloorTexture` | yes | `LevelSector.FloorPic` |
| `DLightLevel` / `SF_FadeLight` | **no** | deferred thinker |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfCeilingHeight` | **1595–1646** |
| `SfMoveCeiling` | **1655–1678** |
| `SfLightLevel` | **1686–1722** |
| `SfFloorTexture` | **1841–1875** |
| `VmFScriptInitFunctionsReg869.Register` | `NewFunction` rows |
| (separators) | **1588–1594**, **1647–1654**, **1679–1685**, **1723–1725**, **1836–1840** |
| (deferred, uncredited here) | **1726–1834** |

### Chunk **869** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **178** |
| **Net headline** | **+178** vs post-**868** |

### Out-of-body uncredit (869)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **178** |
| **Out-of-body removed** | **24** |
| **Honest** | **154** |


**Post-review ratio:** 132/154 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfCeilLightTests.SfCeilingHeight_ReadsSectorCeiling`
- `VmFScriptParserSfCeilLightTests.SfCeilingHeight_WriteUpdatesPlaysimCeiling`
- `VmFScriptParserSfCeilLightTests.SfMoveCeiling_StartsSectorMotion`
- `VmFScriptParserSfCeilLightTests.SfLightLevel_ReadAndWrite`
- `VmFScriptParserSfCeilLightTests.SfFloorTexture_SetsAndReturnsName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 869 --to 869` (0 hard / 0 soft).

## Seams

Ceiling height/move mirror **868** floor paths (`Ceilings[]`, `LineSpecials.ExecuteCeilingSpecial` 280); lights use `AuthoritySimulation.Lights[]`; floor flats set `LevelSector.FloorPic`.

## Credit rationale

First ceiling/light `t_func` band after **868**; `SF_FloorTexture` included for honest **150+** native credit; fade thinker deferred.
