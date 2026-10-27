# Chunk 918 — `DFloor::SetFloorChangeType`, `DFloor::StartFloorSound` / `Construct`, and `FLevelLocals::CreateFloor` from `mapthinkers/a_floor.cpp` (**233–514**). Stops before `DEFINE_ACTION_FUNCTION(CreateFloor)` **516+** and chunk **6** waggle **941+**.

## Scope

`DFloor::SetFloorChangeType`, `DFloor::StartFloorSound` / `Construct`, and `FLevelLocals::CreateFloor` from `mapthinkers/a_floor.cpp` (**233–514**). Stops before `DEFINE_ACTION_FUNCTION(CreateFloor)` **516+** and chunk **6** waggle **941+**.

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::SetFloorChangeType** 233–251 (19 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::StartFloorSound** 259–262 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::Construct** 270–273 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::CreateFloor** 281–514 (234 lines)

**Credited native LOC:** 261 (tally `nativeLoc` 261).

## Managed

`VmAFloorDfloorMethods918Slice.cs`, `VmAFloorCreate.cs`
Managed LOC (tally): 322.
Conversion commit: `a7b028bbe3467d21d56c1e2507a90159945d9e12`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DFloor::SetFloorChangeType` (**233–251**) | yes |
| `DFloor::StartFloorSound` (**259–262**) | yes (forwards to static seam) |
| `DFloor::Construct` (**270–273**) | yes (`Super::Construct` seam) |
| `FLevelLocals::CreateFloor` (**281–514**) | yes (full `floortype` switch + instant/silent + change model) |

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `VmAFloorDfloorMethods918Slice.SetFloorChangeType` | **233–251** |
| `VmAFloorDfloorMethods918Slice.DfloorStartFloorSound` | **259–262** |
| `VmAFloorDfloorMethods918Slice.ConstructThinker` | **270–273** |
| `VmAFloorCreate.CreateFloor` | **281–514** |

### Chunk **918** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **261** |
| **Net headline** | **+261** vs post-**917** |


**Post-review ratio:** 322/261 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorDfloorMethods918SliceTests.SetFloorChangeType_Case3_CopiesSpecialAndType`
- `VmAFloorCreateTests.CreateFloor_ModelChange_InvokesSetFloorChangeType`
- `VmAFloorDfloorMethods918SliceTests.DfloorStartFloorSound_ForwardsToStaticHelper`
- `VmAFloorDfloorMethods918SliceTests.ConstructThinker_InvokesSuperConstruct`
- `VmAFloorCreateTests.CreateFloor_RaiseByValue_SetsUpDirection`
- `VmAFloorCreateTests.CreateFloor_LowerByValue_SetsDownDirection`
- `VmAFloorCreateTests.CreateFloor_SkipsWhenPlaneMoving`
- `VmAFloorCreateTests.CreateFloor_LowerAndChange_UsesModelSectorTexture`
- `VmAFloorCreateTests.CreateFloor_InstantMove_SetsSilentWhenCompatAllows`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 918 --to 918` (0 hard / 0 soft).

## Seams

Height finders, model sectors, thinker spawn (`CreateThinker`), `Super::Construct`, static `StartFloorSound`, and `StopInterpolation` are injected.

## Credit rationale

Honest UNCredited band after chunk **917** head; re-attributes existing `VmAFloorCreate.cs` (never tallied) plus exclusive `VmAFloorDfloorMethods918Slice.cs`. Retired **799** row stays **0** matrix credit.
