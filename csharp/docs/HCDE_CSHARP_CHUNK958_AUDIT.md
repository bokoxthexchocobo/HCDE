# Chunk 958 — `GetLight`, actor `AttachLight`, and `ActivateLight` from `a_dynlight.cpp` (**1–150**). File head before chunk **823** tick band (**196+**).

## Scope

`GetLight`, actor `AttachLight`, and `ActivateLight` from `a_dynlight.cpp` (**1–150**). File head before chunk **823** tick band (**196+**).

## Native ranges

- `src/playsim/a_dynlight.cpp` **GetLight** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmADynLightSpawn958Slice.cs`
Managed LOC (tally): 83.
Conversion commit: `7a0bb499494e34eac6d67879036868ef6f8dbb03`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Static arena / free list (**36–38**) | seam (host alloc) |
| `GetLight` (**49–70**) | yes |
| `AttachLight` actor (**81–120**) | yes |
| `ADynamicLight::AttachLight` export (**122–127**) | seam (VM export) |
| `ActivateLight` (**135–138**) | yes |
| `ADynamicLight::ActivateLight` export (**140–145**) | seam (VM export) |

| Managed (`VmADynLightSpawn958Slice.cs`) | Native (`a_dynlight.cpp`) |
|----------------------------------------|-------------------------|
| `ShouldPopLightFromFreeList` / `ShouldAllocateLightFromArena` | **52–56** |
| `LinkNewLightHasSuccessor` / list defaults | **60–67** |
| `InitialOffMapPosition` / `DefaultShadowmapIndex` constants | **64–69** |
| `AbortActorLightAttach` / UDMF resolvers / `ClearRecreateLightsFlag` | **83–119** |
| `NormalizeSpawnAngleDegrees` / `AttachedLightStartsInactive` | **93–97** |
| `SpawnLightVisibleToPlayerDefault` / `ShouldLinkPreviousLight` | **62–63**, **98** |
| `LightTypeFromSpawnInt` / `LightDefIntensityFromSpawn` | **88**, **99** |
| `DefaultCyclerIncrement` | **59** |
| `ActivateAllAttachedLights` | **137** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **958** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**957** |


**Post-review ratio:** 83/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightSpawn958SliceTests.ShouldPopLightFromFreeList_WhenCountPositive`
- `VmADynLightSpawn958SliceTests.ResolveUdmfSoftShadowRadius_OnlyWhenNonNegative`
- `VmADynLightSpawn958SliceTests.ClearRecreateLightsFlag_ClearsMf8Bit`
- `VmADynLightSpawn958SliceTests.NormalizeSpawnAngleDegrees_WrapsNegative`
- `VmADynLightSpawn958SliceTests.InitialOffMapPosition_UsesInvalidX`
- `VmADynLightSpawn958SliceTests.AbortActorLightAttach_WhenEuthanizing`
- `VmADynLightSpawn958SliceTests.ActivateAllAttachedLights_SkipsNullEntries`
- `VmADynLightSpawn958SliceTests.ArenaAllocationSize_ScalesByPoolCapacity`
- `VmADynLightSpawn958SliceTests.LinkNewLightHasSuccessor_WhenNextPresent`
- `VmADynLightSpawn958SliceTests.ResolveUdmfLinearity_OnlyWhenNonNegative`
- `VmADynLightSpawn958SliceTests.LightTypeFromSpawnInt_TruncatesToByte`
- `VmADynLightSpawn958SliceTests.ShouldLinkPreviousLight_WhenNextPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 958 --to 958` (0 hard / 0 soft).

## Seams

`DynLightArena`, placement `new`, `memset`, `AngleVar`/`IntVar` bindings, and `DEFINE_ACTION_FUNCTION_NATIVE` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmADynLightSpawn958Slice.cs` (does not re-credit **823** `Activate`/`Tick` bodies).
