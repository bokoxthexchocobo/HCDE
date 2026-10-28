# Chunk 379 — drawMarks, author markers, crosshair, and Drawer frame orchestration Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **162**).

## Scope

drawMarks, author markers, crosshair, and Drawer frame orchestration Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **162**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `21b7a111e3302b2995cdc161ba503d4c2633c358`.

## Padding review (2026-10-21)



### Out-of-body uncredit (379)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 379 --to 379` (0 hard / 0 soft).

## Seams

`twod` drawing, `doFollowPlayer`, and `changeWindowScale` engine hooks are not hosted.

## Credit rationale

Credits `DAutomap::drawMarks`, `drawAuthorMarkers`, `drawCrosshair`, and `Drawer` from `am_map.cpp`.
