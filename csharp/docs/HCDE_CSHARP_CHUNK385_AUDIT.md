# Chunk 385 — Ticker, scale save, serialize/show-all, GoBig CCMD, toggle map, AM_Stop Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-compat-callbacks-chunk393 src/am_map.cpp 578-593).

## Scope

Ticker, scale save, serialize/show-all, GoBig CCMD, toggle map, AM_Stop Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-compat-callbacks-chunk393 src/am_map.cpp 578-593).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `65ceae96887750d1db9d616978373b4c05210cd4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (385)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **0** |
| **Honest** | **151** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 385 --to 385` (0 hard / 0 soft).

## Seams

`FSerializer` I/O and live `primaryLevel->automap` callbacks are not hosted.

## Credit rationale

Credits `DAutomap::GetPosition`, `saveScaleAndLoc`, `Ticker`, `Serialize`, `UpdateShowAllLines`, `GoBig`, `ResetFollowLocation`, `AM_Stop`, `AM_ToggleMap`, `AM_Create`, and `CCMD(am_clearmarks)`/`CCMD(am_gobig)` from `am_map.cpp`.
