# Chunk 828 — `DPillar` lifecycle and `FLevelLocals::EV_DoPillar` from `mapthinkers/a_pillar.cpp` (47–259).

## Scope

`DPillar` lifecycle and `FLevelLocals::EV_DoPillar` from `mapthinkers/a_pillar.cpp` (47–259).

## Native ranges

- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::OnDestroy** 47–60 (14 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Serialize** 68–80 (13 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Tick** 88–123 (36 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **DPillar::Construct** 131–220 (90 lines)
- `src/playsim/mapthinkers/a_pillar.cpp` **FLevelLocals::EV_DoPillar** 228–259 (32 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Headline gross vs net

| Anchor | `convertedBehaviorNative` |
| --- | ---: |
| After chunk **827** tally (`a4645f5a`) | **96,839** |
| After chunk **828** tally (`c069dc43`) | **97,024** |
| **Net** | **+185** (= row gross **185**) |

## Managed

`VmAPillar.cs`
Managed LOC (tally): 223.
Conversion commit: `d1f3bac44a9f1c78cc76c99eff0326388f00dc61`.

## Padding review (2026-10-21)

| Managed | Native (`a_pillar.cpp`) |
|---------|----------------------|
| `OnDestroy` | **47–60** |
| `Serialize` | **68–80** |
| `Tick` | **88–123** |
| `Construct` | **131–220** |
| `EvDoPillar` | **228–259** |

**Post-review ratio:** 223/185 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmAPillarTests.OnDestroy_DelRefsInterpolators`
- `VmAPillarTests.Tick_BothPastDest_StopsAndDestroys`
- `VmAPillarTests.Construct_BuildType_SplitsSpeedByDistance`
- `VmAPillarTests.Construct_OpenType_UsesSurroundWhenDistZero`
- `VmAPillarTests.EvDoPillar_SkipsWhenPlaneMoving`
- `VmAPillarTests.EvDoPillar_BuildSkipsEqualFloorCeiling`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 828 --to 828` (0 hard / 0 soft).

## Seams

Sector plane moves, surround height queries, interpolation refs, and `SN_*` sequences are injected; no forwarder to `VmAFloorTickElevator`.

## Credit rationale

First whole-function credit for pillar build/open specials in `a_pillar.cpp`.
