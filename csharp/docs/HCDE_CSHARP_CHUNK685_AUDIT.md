# Chunk 685 — Whole `BuildPortalBlockmap`, `FLinePortalTraverse::AddLineIntercepts`, `FindPortalDestination`, `SetPortalRotation`, `UpdatePortal`, `CollectLinkedPortals`, and `FinalizePortals` (`portal.cpp` 60–322).

## Scope

Whole `BuildPortalBlockmap`, `FLinePortalTraverse::AddLineIntercepts`, `FindPortalDestination`, `SetPortalRotation`, `UpdatePortal`, `CollectLinkedPortals`, and `FinalizePortals` (`portal.cpp` 60–322).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::BuildPortalBlockmap** 60–104 (45 lines)
- `src/playsim/portal.cpp` **FLinePortalTraverse::AddLineIntercepts** 131–170 (40 lines)
- `src/playsim/portal.cpp` **FLevelLocals::FindPortalDestination** 178–194 (17 lines)
- `src/playsim/portal.cpp` **SetPortalRotation** 203–232 (30 lines)
- `src/playsim/portal.cpp` **FLevelLocals::UpdatePortal** 240–284 (45 lines)
- `src/playsim/portal.cpp` **FLevelLocals::CollectLinkedPortals** 293–304 (12 lines)
- `src/playsim/portal.cpp` **FLevelLocals::FinalizePortals** 312–322 (11 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmPortalBuildBlockmap.cs`, `VmPortalUpdateFinalize.cs`
Managed LOC (tally): 336.
Conversion commit: `a3b8cfc7692bd0e9a681031a24b9bce0bd672688`.

## Padding review (2026-10-21)

| Managed | Native (`portal.cpp`) |
|---------|---------------------|
| `VmPortalBuildBlockmap` | `BuildPortalBlockmap` 60–104; `AddLineIntercepts` 131–170; `FindPortalDestination` 178–194; `SetPortalRotation` 203–232 |
| `VmPortalUpdateFinalize` | `UpdatePortal` 240–284; `CollectLinkedPortals` 293–304; `FinalizePortals` 312–322 |

### Out-of-body uncredit (685)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **200** |
| **Out-of-body removed** | **0** |
| **Honest** | **200** |


**Post-review ratio:** 336/200 managed/native ≈ 168% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalBuildBlockmapTests.AddLineIntercepts_PushesCrossingLine`
- `VmPortalBuildBlockmapTests.FindPortalDestination_SkipsSourceLine`
- `VmPortalBuildBlockmapTests.SetPortalRotation_LinkedUsesIdentity`
- `VmPortalUpdateFinalizeTests.UpdatePortal_ClearsFlagsWithoutDestination`
- `VmPortalUpdateFinalizeTests.FinalizePortals_InvokesBuildAndCreate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 685 --to 685` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_PointOnDivlineSide`, `CreateLinkedPortals`, and `Printf` remain native.

## Credit rationale

Portal blockmap and line-portal post-spawn update before clip/translate chunks.
