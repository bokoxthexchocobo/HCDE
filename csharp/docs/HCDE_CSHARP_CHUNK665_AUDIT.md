# Chunk 665 — Whole `P_CheckSight` and `SightCheck::P_SightTraverseIntercepts` (`p_sight.cpp` 534–605, 840–951). Retired (2026-10-24): `--band` hull overlaps existing credit (p-sight-path-traverse-chunk664 src/playsim/p_sight.cpp 619-825).

## Scope

Whole `P_CheckSight` and `SightCheck::P_SightTraverseIntercepts` (`p_sight.cpp` 534–605, 840–951). Retired (2026-10-24): `--band` hull overlaps existing credit (p-sight-path-traverse-chunk664 src/playsim/p_sight.cpp 619-825).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `60263268bf716c7a288d9469bb9c4356ad64e4fa`.

## Padding review (2026-10-21)

| Managed | Native (`p_sight.cpp`) |
|---------|----------------------|
| `CheckSight` / `CheckSightContext` | `P_CheckSight` 840–951 |
| `SightTraverseIntercepts` | `P_SightTraverseIntercepts` 534–605 |

### Out-of-body uncredit (665)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **184** |
| **Out-of-body removed** | **0** |
| **Honest** | **184** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 665 --to 665` (0 hard / 0 soft).

## Seams

`SightCheck::init`, `P_SightPathTraverse`, reject matrix, RNG, and `GetPortalTransition` remain native.

## Credit rationale

Completes sight orchestration after chunks 663–664.
