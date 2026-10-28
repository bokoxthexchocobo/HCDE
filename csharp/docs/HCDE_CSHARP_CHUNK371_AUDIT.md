# Chunk 371 — AM_CheckSecret and drawWalls line classification Retired (2026-10-24): `--band` hull overlaps existing credit (am-map-seg-boundaries-chunk376 src/am_map.cpp 2346-2360).

## Scope

AM_CheckSecret and drawWalls line classification Retired (2026-10-24): `--band` hull overlaps existing credit (am-map-seg-boundaries-chunk376 src/am_map.cpp 2346-2360).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0cc44f133fbcbed7c3146278a8d47ddf24ba0372`.

## Padding review (2026-10-21)



### Out-of-body uncredit (371)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **177** |
| **Out-of-body removed** | **0** |
| **Honest** | **177** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 371 --to 371` (0 hard / 0 soft).

## Seams

`drawMline`, portal displacement, and boundary helpers are not hosted; managed code mirrors secret checks and wall style routing.

## Credit rationale

Credits `AM_CheckSecret` and `DAutomap::drawWalls` from `am_map.cpp`.
