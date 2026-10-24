# Chunk 870 — Retired (2026-10-23): `t_func.cpp` **1876–2038** band had honest behavior ports in `VmFScriptParserSfTexDoor.cs`, but **72** surviving managed lines vs **153** native credit (**47%**) after removing coverage-padding helpers (`FraggleDoorSpeedArg`, `FraggleDoorWaitTics`, `FraggleDoorTagAllowed`). No further top-up; band uncredited until a re-land meets the **50%** floor without line-count padding.

## Scope

Retired (2026-10-23): `t_func.cpp` **1876–2038** band had honest behavior ports in `VmFScriptParserSfTexDoor.cs`, but **72** surviving managed lines vs **153** native credit (**47%**) after removing coverage-padding helpers (`FraggleDoorSpeedArg`, `FraggleDoorWaitTics`, `FraggleDoorTagAllowed`). No further top-up; band uncredited until a re-land meets the **50%** floor without line-count padding.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmFScriptInitFunctions.cs`
Managed LOC (tally): 0.
Conversion commit: `f11a337990e170413bf19a34e4ec5d566e914d46`.

## Padding review (2026-10-21)

### Padding incident (chunk **870**)

| Artifact | Native basis | Verdict |
|----------|--------------|--------|
| `FraggleDoorSpeedArg` | `2.*clamp(speed,1,127)` inline in `SF_OpenDoor` / `SF_CloseDoor` | **Padding** — extracted for managed LOC; reverted |
| `FraggleDoorWaitTics` | `(intvalue(argv[1])*TICRATE)/100` inline in `SF_OpenDoor` | **Padding** — reverted |
| `FraggleDoorTagAllowed` | `if (sectag==0) return` | **Padding** — reverted |
| Blank lines in `VmFScriptInitFunctions.cs` (uncommitted) | tally **283** vs **282** | **Padding** — discarded (tally follows HEAD) |

### Behavior ported (off-tally, not matrix-credited)

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SectorColormap` | yes (native no-op) |
| `SF_CeilingTexture` | yes |
| `SF_OpenDoor` / `SF_CloseDoor` | yes (inlined speed/wait/tag guards) |

| Managed (off-tally) | Native |
|---------|--------|
| `SfSectorColormap` | **1883–1922** |
| `SfCeilingTexture` | **1931–1965** |
| `SfOpenDoor` | **1992–2013** |
| `SfCloseDoor` | **2021–2038** |
| (separators) | **1876–1882**, **1923–1930**, **1966–1972**, **1983–1991**, **2014–2020** |
| (unregistered hub/skill) | **1973–1982** |

### Gross vs net

| | Value |
|--|------:|
| Prior active `nativeLoc` | **153** |
| Surviving managed (HEAD, no helpers) | **72** |
| Matrix credit after retire | **0** |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 870 --to 870` (0 hard / 0 soft).

## Seams

Off-tally implementations remain (`VmFScriptParserSfTexDoor.cs`, `VmFScriptInitFunctionsReg870.cs`); `Reg870` removed from `RegisterFraggleFunctions` like retired **852**.

## Credit rationale

Padding incident: commits **84faa281** / **50943a25** added door helpers solely to pass `check_port_coverage.sh`; reverted on **a674ab10** / **8a186f7a**. Ratio still below floor with inlined native door math — retire instead of dishonest LOC.
