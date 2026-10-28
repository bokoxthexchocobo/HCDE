# Chunk 662 — Whole `FLevelLocals::CreateLinkedPortals` (`portal.cpp` 851–1039).

## Scope

Whole `FLevelLocals::CreateLinkedPortals` (`portal.cpp` 851–1039).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::CreateLinkedPortals** 851–1039 (189 lines)

**Credited native LOC:** 189 (tally `nativeLoc` 189).

## Managed

`VmPortalCreateLinkedPortals.cs`
Managed LOC (tally): 242.
Conversion commit: `b590f071da08915e8b5dd8760c0b9d6943fc81a3`.

## Padding review (2026-10-21)

| Managed | Native (`portal.cpp`) |
|---------|---------------------|
| `CreateLinkedPortals` / `CreateLinkedPortalsContext` | `CreateLinkedPortals` 851–1039 |
| `CollectFromLinkedOrgs` / `ProcessLineLinkedPortals` / `RunDisplacementConsistencyPass` / `FinalizeLinkedSectorPlanes` | org collect **884–907**, displacements **909–932**, mismatch **934–971**, plane finalize **983–1018** |

### Out-of-body uncredit (662)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **189** |
| **Out-of-body removed** | **0** |
| **Honest** | **189** |


**Post-review ratio:** 242/189 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalCreateLinkedPortalsTests.CreateLinkedPortals_CollectsWithLinkedIds`
- `VmPortalCreateLinkedPortalsTests.CreateLinkedPortals_FlagsDisplacementMismatch`
- `VmPortalCreateLinkedPortalsTests.ProcessLineLinkedPortals_DisablesUnconnected`
- `VmPortalCreateLinkedPortalsTests.ResolveCeilingBelowFloorClearPlane_PicksCeilingWhenPlaneDLower`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 662 --to 662` (0 hard / 0 soft).

## Seams

`CollectSectors`, `AddDisplacementForPortal`, and map mutation stay native.

## Credit rationale

Fresh portal TU; linked portal grouping hosted.
