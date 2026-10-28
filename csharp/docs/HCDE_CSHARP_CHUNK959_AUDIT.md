# Chunk 959 — `P_RecalculateLights` tail, `P_RecalculateAttachedLights`, `P_GetPlaneLight`, and `P_LineOpening_XFloors` head from `p_3dfloors.cpp` (**601–750**). Retired chunk **781** left this band UNCredited.

## Scope

`P_RecalculateLights` tail, `P_RecalculateAttachedLights`, `P_GetPlaneLight`, and `P_LineOpening_XFloors` head from `p_3dfloors.cpp` (**601–750**). Retired chunk **781** left this band UNCredited.

## Native ranges

- `src/playsim/p_3dfloors.cpp` **P_RecalculateLights** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmP3dFloorsLightOpening959Slice.cs`
Managed LOC (tally): 109.
Conversion commit: `28c3d3b6ba9216e4f25b782083e41851f7277589`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_RecalculateLights` loop body (**601–612**) | yes |
| `P_RecalculateAttachedLights` (**620–629**) | yes |
| `P_GetPlaneLight` (**637–650**) | yes |
| `P_LineOpening_XFloors` 3D-floor scan (**672–750**) | yes |
| `thing` null early out (**661–662**) | seam (host) |

| Managed (`VmP3dFloorsLightOpening959Slice.cs`) | Native (`p_3dfloors.cpp`) |
|------------------------------------------------|-------------------------|
| `RefreshLightListEntry` | **601–612** |
| `RecalculateAttachedSectorLights` | **624–628** |
| `PlaneHeightAtCenter` / `SelectPlaneLightIndex` | **642–649** |
| `SectorHas3dFloors` / `Solid3dFloorActive` | **672–698** |
| `MidpointDistanceDeltas` / ceiling vs floor picks | **703–712**, **714–727** |
| `ShouldTrackSteppableLowestFloor` | **729–733** |
| `ShouldRaiseOpeningBottom` / plane flip gates | **737–754** (hull ends **750**) |
| Band hull (exclusive slice) | **601–750** |

### Chunk **959** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**958** |


**Post-review ratio:** 109/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmP3dFloorsLightOpening959SliceTests.RefreshLightListEntry_UsesSourceBlendWhenPresent`
- `VmP3dFloorsLightOpening959SliceTests.RefreshLightListEntry_FallsBackToSectorColormap`
- `VmP3dFloorsLightOpening959SliceTests.RecalculateAttachedSectorLights_VisitsAttachedBeforeSelf`
- `VmP3dFloorsLightOpening959SliceTests.PlaneHeightAtCenter_SubtractsEpsilonOnUnderside`
- `VmP3dFloorsLightOpening959SliceTests.SelectPlaneLightIndex_PicksTierBelowPlane`
- `VmP3dFloorsLightOpening959SliceTests.Solid3dFloorActive_RequiresExistsAndSolid`
- `VmP3dFloorsLightOpening959SliceTests.ShouldPickLowestCeiling_WhenBottomCloserToThingFeet`
- `VmP3dFloorsLightOpening959SliceTests.FloorCandidateDominatesThing_RespectsRestrictFlag`
- `VmP3dFloorsLightOpening959SliceTests.ShouldTrackSteppableLowestFloor_WithinMaxStep`
- `VmP3dFloorsLightOpening959SliceTests.SecPlaneNeedsFlip_WhenCoeffNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 959 --to 959` (0 hard / 0 soft).

## Seams

`UpdateColormap` / `GetBlend` on `F3DFloor`, texture/terrain host lookups, and `FLineOpening` mutation remain injected seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmP3dFloorsLightOpening959Slice.cs` (no tally credit via **781** / **785** / **699** hulls).
