# Chunk 956 — `CollectWithinRadius` portal tail, `LinkLight`, `UnlinkLight`, `AttachLight`, and `SetDynamicLights` head from `a_dynlight.cpp` (**601–750**). Adjacent to chunk **955** (**751–900**); chunk **823** tick band unchanged.

## Scope

`CollectWithinRadius` portal tail, `LinkLight`, `UnlinkLight`, `AttachLight`, and `SetDynamicLights` head from `a_dynlight.cpp` (**601–750**). Adjacent to chunk **955** (**751–900**); chunk **823** tick band unchanged.

## Native ranges

- `src/playsim/a_dynlight.cpp` **FDynamicLight::CollectWithinRadius** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmADynLightLink956Slice.cs`
Managed LOC (tally): 86.
Conversion commit: `ff35d0f43138cf9988447f12d3e8dbcb6daa869f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CollectWithinRadius` portal ceiling/floor (**601–630**) | yes |
| `shadowmapped` assignment (**632**) | yes |
| `LinkLight` (**641–654**) | yes |
| `UnlinkLight` (**662–691**) | yes |
| `AttachLight` (**699–718**) | yes |
| `SetDynamicLights` head (**726–750**) | yes |

| Managed (`VmADynLightLink956Slice.cs`) | Native (`a_dynlight.cpp`) |
|----------------------------------------|-------------------------|
| `PortalSightOpen` / `CeilingPortalAffectsLight` / `FloorPortalAffectsLight` | **601–620** |
| `ShouldCollectPortalSection` | **609–627** |
| `ShadowMappedAfterCollection` | **632** |
| `ShouldLinkDynamicLight` / `LinkLightRadiusSquared` | **644–651** |
| `UnlinkSkipsNullSide` / `UnlinkListIndexInRange` | **668–684** |
| `ReuseAttachedLightSlot` / `AbortAttachLightWhenEuthanizing` | **701–715** |
| `ShouldAbortSetDynamicLights` / association/state-light gates | **731–750** |
| `DistToSegClampParameter` / `LightInFrontOfSeg` / segment radius | **489–545**, **572–597** (predicates cited in band context) |
| Band hull (exclusive slice) | **601–750** |

### Chunk **956** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**955** |


**Post-review ratio:** 86/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightLink956SliceTests.CeilingPortalAffectsLight_WhenPlaneBelowTopOfRadius`
- `VmADynLightLink956SliceTests.FloorPortalAffectsLight_WhenPlaneAboveBottomOfRadius`
- `VmADynLightLink956SliceTests.ShadowMappedAfterCollection_RequiresBackHitAndShadowmapAllowed`
- `VmADynLightLink956SliceTests.LinkLightRadiusSquared_DoublesAsSquare`
- `VmADynLightLink956SliceTests.DistToSegClampParameter_ClampsToSegment`
- `VmADynLightLink956SliceTests.LightInFrontOfSeg_UsesCrossProductSign`
- `VmADynLightLink956SliceTests.AssociationTriggersAttach_DelegatesTo955Slice`
- `VmADynLightLink956SliceTests.ShouldAbortSetDynamicLights_WhenStateMissing`
- `VmADynLightLink956SliceTests.SegmentDistanceWithinRadius_ComparesSquaredDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 956 --to 956` (0 hard / 0 soft).

## Seams

`AddLightNode`, `GetLight`, `PointInRenderSubsector`, light list `Remove`, and `ApplyProperties` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmADynLightLink956Slice.cs` (reuses **955** association/euthanize predicates only by reference, no duplicate tally rows).
