# Chunk 420 — Sector link, texture/wall scroll, floor/ceiling scroll, damage/gravity/color/fade/panning Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Sector link, texture/wall scroll, floor/ceiling scroll, damage/gravity/color/fade/panning Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6aa8f7ba605ffd13ad4f5b5b9158bf30b2ad2bce`.

## Padding review (2026-10-21)



### Out-of-body uncredit (420)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **283** |
| **Out-of-body removed** | **283** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 420 --to 420` (0 hard / 0 soft).

## Seams

`P_AddSectorLinks`, `SetScroller`, and live `sector_t` mutation are not hosted.

## Credit rationale

Credits scroll id normalization, floor carry modes, legacy damage leakiness, gravity clamping, and panning offsets from `p_lnspec.cpp`.
