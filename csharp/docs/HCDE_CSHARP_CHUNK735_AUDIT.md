# Chunk 735 — Whole `AActor::FallAndSink` and `P_NightmareRespawn` (`p_mobj.cpp` 3355–3551). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-fakefloor-land-chunk747 src/playsim/p_mobj.cpp 3443-3454).

## Scope

Whole `AActor::FallAndSink` and `P_NightmareRespawn` (`p_mobj.cpp` 3355–3551). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-fakefloor-land-chunk747 src/playsim/p_mobj.cpp 3443-3454).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `1aee7eb4436a73d3a2450ce2d18ebb1e30397da2`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `FallAndSink` | `AActor::FallAndSink` 3355–3432 |
| `NightmareRespawn` | `P_NightmareRespawn` 3459–3551 |

### Out-of-body uncredit (735)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **171** |
| **Out-of-body removed** | **0** |
| **Honest** | **171** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 735 --to 735` (0 hard / 0 soft).

## Seams

`StaticSpawn`, `P_CheckPosition`, `P_FindFloorCeiling`, and teleport fog spawns are injected.

## Credit rationale

Water sink policy and nightmare respawn orchestrator as two whole functions in one TU.
