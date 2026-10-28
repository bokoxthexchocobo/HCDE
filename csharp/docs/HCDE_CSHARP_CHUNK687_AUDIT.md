# Chunk 687 — Whole sector portal factories, `GetPortalOffsetPosition`, `CollectSectors`, both `AddDisplacementForPortal`, and `ConnectPortalGroups` (`portal.cpp` 535–835).

## Scope

Whole sector portal factories, `GetPortalOffsetPosition`, `CollectSectors`, both `AddDisplacementForPortal`, and `ConnectPortalGroups` (`portal.cpp` 535–835).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::GetSkyboxPortal** 535–548 (14 lines)
- `src/playsim/portal.cpp` **FLevelLocals::GetPortal** 564–574 (11 lines)
- `src/playsim/portal.cpp` **FLevelLocals::GetStackPortal** 584–594 (11 lines)
- `src/playsim/portal.cpp` **FLevelLocals::GetPortalOffsetPosition** 606–665 (60 lines)
- `src/playsim/portal.cpp` **CollectSectors** 676–699 (24 lines)
- `src/playsim/portal.cpp` **FLevelLocals::AddDisplacementForPortal** 714–746 (33 lines)
- `src/playsim/portal.cpp` **FLevelLocals::AddDisplacementForPortal** 749–781 (33 lines)
- `src/playsim/portal.cpp` **FLevelLocals::ConnectPortalGroups** 791–835 (45 lines)

**Credited native LOC:** 231 (tally `nativeLoc` 231).

## Managed

`VmPortalDisplacementConnect.cs`
Managed LOC (tally): 251.
Conversion commit: `bb1a7f88f2a3676e951e61d81265161fa97d06f0`.

## Padding review (2026-10-21)

| Managed | Native (`portal.cpp`) |
|---------|---------------------|
| `VmPortalDisplacementConnect` | `GetSkyboxPortal` 535–548; `GetPortal` 564–574; `GetStackPortal` 584–594; `GetPortalOffsetPosition` 606–665; `CollectSectors` 676–699; `AddDisplacementForPortal` 714–746; `AddDisplacementForPortal` 749–781; `ConnectPortalGroups` 791–835 |

### Out-of-body uncredit (687)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **231** |
| **Out-of-body removed** | **0** |
| **Honest** | **231** |


**Post-review ratio:** 251/231 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalDisplacementConnectTests.CollectSectors_FloodFillsGroup`
- `VmPortalDisplacementConnectTests.GetPortalOffsetPosition_ReturnsDeltaWhenNoNeighborPortalLines`
- `VmPortalDisplacementConnectTests.ConnectPortalGroups_FillsIndirectDisplacement`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 687 --to 687` (0 hard / 0 soft).

## Seams

`FLinePortalTraverse`, `Printf`, and `CreateLinkedPortals` remain native.

## Credit rationale

Sector portal allocation through displacement graph after chunk 686 translation.
