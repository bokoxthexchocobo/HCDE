# Chunk 957 — `AddLightNode` wall path, `DistToSeg`, and `CollectWithinRadius` head through side loops from `a_dynlight.cpp` (**451–600**). Adjacent to chunk **956** (**601–750**); chunk **823** tick band unchanged.

## Scope

`AddLightNode` wall path, `DistToSeg`, and `CollectWithinRadius` head through side loops from `a_dynlight.cpp` (**451–600**). Adjacent to chunk **956** (**601–750**); chunk **823** tick band unchanged.

## Native ranges

- `src/playsim/a_dynlight.cpp` **FDynamicLight::CollectWithinRadius** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmADynLightCollect957Slice.cs`
Managed LOC (tally): 88.
Conversion commit: `6c85546e0d3c1201a4745c73c7941ca4b2ad1c7c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AddLightNode` wall branch (**451–468**) | yes |
| `DistToSeg` (**480–499**) | yes |
| `CollectWithinRadius` init + segment/side loops (**515–600**) | yes |

| Managed (`VmADynLightCollect957Slice.cs`) | Native (`a_dynlight.cpp`) |
|-------------------------------------------|-------------------------|
| `AddLightNodeTargetsWall` / `EvaluateLightListCapacity` | **451–467** |
| `DistToSegSquared` / `SegmentWithinLightRadius` | **480–499**, **572–597** |
| `ShouldSkipCollectWithinRadius` / validcount init | **517–520** |
| `ProcessSideWhenLineFresh` / `ShouldAddWallLightNode` / backface | **534–546** |
| `LinkedPortalEligible` / portal traverse gates | **550–562** |
| `PartnerSectionEligible` / segment partner push | **580–588** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **957** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**956** |


**Post-review ratio:** 88/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightCollect957SliceTests.DistToSegSquared_OnSegmentEndpoint`
- `VmADynLightCollect957SliceTests.DistToSegSquared_PerpendicularOffset`
- `VmADynLightCollect957SliceTests.SegmentWithinLightRadius_UsesSquaredCompare`
- `VmADynLightCollect957SliceTests.LightNodeListNeedsResize_WhenIndexAtSize`
- `VmADynLightCollect957SliceTests.EvaluateLightListCapacity_ResizesWhenNeeded`
- `VmADynLightCollect957SliceTests.LinkedPortalEligible_OnlyLinkedType`
- `VmADynLightCollect957SliceTests.ShouldAddWallLightNode_DelegatesCrossProduct`
- `VmADynLightCollect957SliceTests.ShouldSkipCollectWithinRadius_WhenSectionNull`
- `VmADynLightCollect957SliceTests.AddLightNodeTargetsWall_WhenOnlySidedef`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 957 --to 957` (0 hard / 0 soft).

## Seams

`FLightNode` allocation, `TryEmplace`, `SortedAddUnique`, and `PointInRenderSubsector` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmADynLightCollect957Slice.cs` (delegates segment/front checks to **956**, no duplicate native rows).
