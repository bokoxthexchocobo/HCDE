# Chunk 369 — Automap scale restore, bounds, panning, and parchment scroll policy Retired (2026-10-24): `--band` hull overlaps existing credit (am-map-lifecycle-colors-chunk385 src/am_map.cpp 1129-1135).

## Scope

Automap scale restore, bounds, panning, and parchment scroll policy Retired (2026-10-24): `--band` hull overlaps existing credit (am-map-lifecycle-colors-chunk385 src/am_map.cpp 1129-1135).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7e716f0fe15254c8fb61a09c8bd5e38c748142ba`.

## Padding review (2026-10-21)



### Out-of-body uncredit (369)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **154** |
| **Out-of-body removed** | **0** |
| **Honest** | **154** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 369 --to 369` (0 hard / 0 soft).

## Seams

Live `DAutomap` fields, `rotate`, and texture-backed parchment tiling are not hosted; managed code mirrors numeric window/scale rules.

## Credit rationale

Credits `activateNewScale`, `restoreScaleAndLoc`, `addMark`, `findMinMaxBoundaries`, `calcMinMaxMtoF`, `ClipRotatedExtents`, `ScrollParchment`, and `changeWindowLoc` from `am_map.cpp`.
