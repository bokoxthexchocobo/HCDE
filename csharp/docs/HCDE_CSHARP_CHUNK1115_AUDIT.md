# Chunk 1115 — `UpdateWaterLevel` dive gate and `ConstructActor` spawn setup through the float-rand space test from `p_mobj.cpp` (**5427–5576**); follows **1114**.

## Scope

`UpdateWaterLevel` dive gate and `ConstructActor` spawn setup through the float-rand space test from `p_mobj.cpp` (**5427–5576**); follows **1114**.

## Native ranges

- `src/playsim/p_mobj.cpp` **UpdateWaterLevel tail and ConstructActor** 5427–5576 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjConstructActor1115Slice.cs`
Managed LOC (tally): 142.
Conversion commit: `85085801a0411aa9e2729f457b8a411fc217f782`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dive sound only for players (**5432–5435**) | yes |
| `UpdateWaterLevel` returns false (**5437**) | yes |
| Conversation root gate (**5462–5468**) | yes |
| Zero gravity flag (**5477**) | yes |
| Instant reaction / last-look (**5481–5489**) | yes |
| Client vs server tics + fullbright (**5496–5500**) | yes |
| Fast speed, spawn Z, floor/ceiling copy (**5503–5556**) | yes |
| Spawn XY only + float-rand space (**5559–5576**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **5423–5424** | splash/depth calls (chunk **1114**) |
| **5440–5444** | VM thunk |
| **5510–5515** | host link / plane sample |
| **5577+** | float-rand write outside hull |

| Managed (`VmPMobjConstructActor1115Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------------|---------------------|
| Water-level tail + construct policy | **5427–5576** |
| Band hull (exclusive slice) | **5427–5576** |

### Chunk **1115** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1114** |


**Post-review ratio:** 142/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjConstructActor1115SliceTests.RunUpdateWaterLevel_PlaysDiveOnlyForPlayersAndReturnsFalse`
- `VmPMobjConstructActor1115SliceTests.RunConstructActor_ZerosReactionForAlwaysFastMonsters`
- `VmPMobjConstructActor1115SliceTests.RunConstructActor_PlacesOnCeilingAndCopiesMapFloor`
- `VmPMobjConstructActor1115SliceTests.RunConstructActor_FloatRandomOnlyWhenSpaceExceeds48`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1115 --to 1115` (0 hard / 0 soft).

## Seams

`PlayDiveOrSurfaceSounds`, `LinkToWorld`, `P_FindFloorCeiling`, and conversation lookup remain host-injected; float-rand Z write continues at **5577**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **5427–5576** (+150); follows **1114** (**5277–5426**).
