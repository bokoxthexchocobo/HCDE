# Chunk 684 — Whole `FMultiBlockThingsIterator`, point/subsector helpers, and `BoxOnLineSide` (`p_maputl.cpp` 1038–1131, 1846–2067). `P_RoughMonsterSearch` is a thin `P_BlockmapSearch` forward (already credited via `RoughBlockCheck` on chunk 682 and `ActorEnemySearchActions`). Retired (2026-10-24): `--band` hull overlaps existing credit (p-maputl-path-traverse-init-chunk666 src/playsim/p_maputl.cpp 1397-1604).

## Scope

Whole `FMultiBlockThingsIterator`, point/subsector helpers, and `BoxOnLineSide` (`p_maputl.cpp` 1038–1131, 1846–2067). `P_RoughMonsterSearch` is a thin `P_BlockmapSearch` forward (already credited via `RoughBlockCheck` on chunk 682 and `ActorEnemySearchActions`). Retired (2026-10-24): `--band` hull overlaps existing credit (p-maputl-path-traverse-init-chunk666 src/playsim/p_maputl.cpp 1397-1604).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f7a11037ad79baef62d89d174fc7b3e1b76b5269`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `VmPMapUtlMultiBlockThingsIterator` | `FMultiBlockThingsIterator` 1038–1131 |
| `VmPMapUtlPointSubsector` | `R_PointOnSideSlow` 1846–1896; `P_VanillaPointOnLineSide` 1906–1938; `PointInSubsector` 1946–1962; `PointInSectorBuggy` 1971–1991; `PointInRenderSubsector` 1999–2018; `BoxOnLineSide` 2030–2067 |

### Out-of-body uncredit (684)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **253** |
| **Out-of-body removed** | **1** |
| **Honest** | **252** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 684 --to 684` (0 hard / 0 soft).

## Seams

`P_BlockmapSearch`, `R_PointOnSide`, `FloatToFixed`, and VM `DEFINE_ACTION_FUNCTION` wrappers remain native.

## Credit rationale

Finishes `p_maputl.cpp` after block iterators in chunk 683.
