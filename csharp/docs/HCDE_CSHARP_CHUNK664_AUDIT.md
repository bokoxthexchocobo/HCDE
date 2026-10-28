# Chunk 664 — Whole `SightCheck::P_SightPathTraverse` (`p_sight.cpp` 619–825).

## Scope

Whole `SightCheck::P_SightPathTraverse` (`p_sight.cpp` 619–825).

## Native ranges

- `src/playsim/p_sight.cpp` **SightCheck::P_SightPathTraverse** 619–825 (207 lines)

**Credited native LOC:** 207 (tally `nativeLoc` 207).

## Managed

`VmPSightPathTraverse.cs`
Managed LOC (tally): 217.
Conversion commit: `6b594387947709cc7a66420929d9eefacbaa1094`.

## Padding review (2026-10-21)

| Managed | Native (`p_sight.cpp`) |
|---------|----------------------|
| `SightPathTraverse` / `SightPathTraverseContext` | `P_SightPathTraverse` 619–825 |

### Out-of-body uncredit (664)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **207** |
| **Out-of-body removed** | **0** |
| **Honest** | **207** |


**Post-review ratio:** 217/207 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmPSightPathTraverseTests.SightPathTraverse_EarlyOutWhenBlockIteratorFails`
- `VmPSightPathTraverseTests.SightPathTraverse_QueuesCeilingPortalWhenOpen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 664 --to 664` (0 hard / 0 soft).

## Seams

`P_SightBlockLinesIterator`, `P_SightTraverseIntercepts`, blockmap storage, and sector/ffloor host data are injected.

## Credit rationale

Fresh sight TU band after chunk 663 `PTR_SightTraverse`; blockmap stepping and portal queue setup.
