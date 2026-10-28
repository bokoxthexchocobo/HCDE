# Chunk 432 — Ceiling raise-by-texture, generic ceiling/crusher, plat movers, generic lift (legacy plat row migration) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Ceiling raise-by-texture, generic ceiling/crusher, plat movers, generic lift (legacy plat row migration) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `31c6214b10ceac0119dbcc9e6a4b6e4eabc5d201`.

## Padding review (2026-10-21)



### Out-of-body uncredit (432)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **194** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 432 --to 432` (0 hard / 0 soft).

## Seams

`EV_DoCeiling`, `EV_DoPlat`, and `EV_StopPlat` thinkers are not hosted.

## Credit rationale

Credits generic ceiling target switches, Doom/Hexen crusher modes, plat stop remove policy, raise-and-stay gametype defaults, and generic lift dispatch from `p_lnspec.cpp` (bands migrated off `line-specials-plat-generic-elevator`).
