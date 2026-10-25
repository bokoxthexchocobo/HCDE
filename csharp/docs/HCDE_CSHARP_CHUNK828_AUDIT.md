# Chunk 828 — `DPillar` lifecycle and `FLevelLocals::EV_DoPillar` from `mapthinkers/a_pillar.cpp` (47–259).

## Scope

`DPillar` lifecycle and `FLevelLocals::EV_DoPillar` from `mapthinkers/a_pillar.cpp` (47–259).

## Native ranges

- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::OnDestroy** 47–60 (14 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Serialize** 68–80 (13 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Tick** 88–123 (36 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Construct** 131–220 (90 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmAPillar.cs`
Managed LOC (tally): 223.
Conversion commit: `3d3510d1d8b25ffff9359d26d59b384b0a467be8`.

## Padding review (2026-10-21)

| Managed | Native (`a_pillar.cpp`) |
|---------|----------------------|
| `OnDestroy` | **47–60** |
| `Serialize` | **68–80** |
| `Tick` | **88–123** |
| `Construct` | **131–220** |
| `EvDoPillar` | **228–259** |

### Out-of-body uncredit (828)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **32** |
| **Honest** | **153** |


**Post-review ratio:** 223/153 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmAPillarTests.OnDestroy_DelRefsInterpolators`
- `VmAPillarTests.Tick_BothPastDest_StopsAndDestroys`
- `VmAPillarTests.Construct_BuildType_SplitsSpeedByDistance`
- `VmAPillarTests.Construct_OpenType_UsesSurroundWhenDistZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 828 --to 828` (0 hard / 0 soft).

## Seams

Sector plane moves, surround height queries, interpolation refs, and `SN_*` sequences are injected; no forwarder to `VmAFloorTickElevator`.

## Credit rationale

First whole-function credit for pillar build/open specials in `a_pillar.cpp`.
