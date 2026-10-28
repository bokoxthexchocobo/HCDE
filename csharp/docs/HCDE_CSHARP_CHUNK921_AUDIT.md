# Chunk 921 — `EV_DoDonut` (**796–863**) and `DElevator` head through `OnDestroy` (**864–940**) from `mapthinkers/a_floor.cpp`. Hull **791–940** sits above chunk **919** (**≤788**) and below chunk **6** `DElevator::Tick` (**941+**).

## Scope

`EV_DoDonut` (**796–863**) and `DElevator` head through `OnDestroy` (**864–940**) from `mapthinkers/a_floor.cpp`. Hull **791–940** sits above chunk **919** (**≤788**) and below chunk **6** `DElevator::Tick` (**941+**).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoDonut** 791–863 (73 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator** 864–940 (77 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAFloorEvDonut921Slice.cs`, `VmAFloorElevatorHead921Slice.cs`
Managed LOC (tally): 151.
Conversion commit: `534b1518af0f972b4a9a8980efd8685009b84c43`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `EV_DoDonut` (**791–863**) | yes |
| `DElevator` class head (**864–940**) | yes (**878–924** behavior; **925–940** comment seam before `Tick`) |

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `VmAFloorEvDonut921Slice.EvDoDonut` | **791–863** |
| `VmAFloorElevatorHead921Slice.ConstructElevator` | **878–885** |
| `VmAFloorElevatorHead921Slice.CaptureSerializeFields` / `ApplySerializeFields` | **887–897** |
| `VmAFloorElevatorHead921Slice.DefinedFieldNames` | **899–903** |
| `VmAFloorElevatorHead921Slice.OnDestroyElevator` | **911–924** |
| (IMPLEMENT / pointer macros) | **871–876** |

### Chunk **921** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**920** |


**Post-review ratio:** 151/150 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorEvTests.EvDoDonut_CreatesSlimeAndPillarThinkers`
- `VmAFloorEvTests.EvDoDonut_SkipsWhenFloorPlaneMoving`
- `VmAFloorElevatorHead921SliceTests.ConstructElevator_WiresFloorAndCeilingData`
- `VmAFloorElevatorHead921SliceTests.SerializeBundle_RoundTripsElevatorFields`
- `VmAFloorElevatorHead921SliceTests.OnDestroyElevator_ReleasesInterpRefs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 921 --to 921` (0 hard / 0 soft).

## Seams

Tag iterators, `getNextSector`, thinker spawn, height finders, interpolation refs, and `Super::Construct` / `OnDestroy` are injected.

## Credit rationale

Honest **150**-line UNCredited band; exclusive donut + elevator-head slices (chunk **920** keeps `EvDoChange` only on `VmAFloorEvDonutChange920Slice.cs`).
