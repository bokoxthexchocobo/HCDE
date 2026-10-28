# Chunk 966 — `P_DelPortalnode`, `P_AddPortalnode`, `AActor::UpdateRenderSectorList` head, and render-portal clear helpers from `p_secnodes.cpp` (**284–433**). Follows chunk **810** (**≤283**); chunk **881** deferred band split — this row credits **284–433** only.

## Scope

`P_DelPortalnode`, `P_AddPortalnode`, `AActor::UpdateRenderSectorList` head, and render-portal clear helpers from `p_secnodes.cpp` (**284–433**). Follows chunk **810** (**≤283**); chunk **881** deferred band split — this row credits **284–433** only.

## Native ranges

- `src/playsim/p_secnodes.cpp` **P_DelPortalnode** 284–433 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSecnodesPortalRender966Slice.cs`
Managed LOC (tally): 94.
Conversion commit: `1a194a937408cc53624f6fca3937b2156bb67242`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_DelPortalnode` (**293–329**) | yes |
| `P_AddPortalnode` (**338–367**) | yes |
| `AActor::UpdateRenderSectorList` gates/loops (**375–427**) | yes (pure planning; host runs geometry) |
| `AActor::ClearRenderSectorList` (**429–433**) | yes |
| Comment/separator hull (**284–292**, **331–337**, **369–374**, **428**) | layout only |

| Managed (`VmPSecnodesPortalRender966Slice.cs`) | Native (`p_secnodes.cpp`) |
|-----------------------------------------------|---------------------------|
| `DelPortalnode` | **293–329** |
| `AddPortalnode` | **338–367** |
| `ShouldRefreshRenderLists` / portal scan gates | **378–395** |
| `CeilingPortalLoopContinues` / `FloorPortalLoopContinues` | **404–425** |
| `ClearRenderSectorPortalList` | **429–433** |
| Band hull (exclusive slice) | **284–433** |

### Chunk **966** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**965** |


**Post-review ratio:** 94/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmPSecnodesPortalRender966SliceTests.DelPortalnode_UnlinksBothThreadsAndFreesNode`
- `VmPSecnodesPortalRender966SliceTests.AddPortalnode_PrependsPortalThread`
- `VmPSecnodesPortalRender966SliceTests.ShouldRefreshRenderLists_RequiresMoveWithoutNosector`
- `VmPSecnodesPortalRender966SliceTests.ShouldScanBlockForLinePortals_AllGatesRequired`
- `VmPSecnodesPortalRender966SliceTests.CeilingPortalLoopContinues_MatchesHeightGuards`
- `VmPSecnodesPortalRender966SliceTests.FloorPortalLoopContinues_MatchesHeightGuards`
- `VmPSecnodesPortalRender966SliceTests.ClearRenderSectorPortalList_EmptiesTouchingList`
- `VmPSecnodesPortalRender966SliceTests.RenderBoundingRadius_CapsAt128`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 966 --to 966` (0 hard / 0 soft).

## Seams

`I_FatalError`, `BoxOnLineSide`, `inRange`, `PointInSector`, portal displacement/plane queries, and playsim tick wiring remain host seams; freelist reuse calls chunk **810** `VmPSecnodes` without re-crediting **42–283**.

## Credit rationale

Honest **150**-line UNCredited band with zero conversion/recognition overlap; exclusive `VmPSecnodesPortalRender966Slice.cs` (does not duplicate chunk **810** secnode list credit).
