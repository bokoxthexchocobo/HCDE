# Chunk 431 — Non-overlapping door, waggle, ceiling instant, and Thing_Destroy bands Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Non-overlapping door, waggle, ceiling instant, and Thing_Destroy bands Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `679c272dfbaa5712fc48ce46cf07215e4cb8eed9`.

## Padding review (2026-10-21)



### Out-of-body uncredit (431)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **150** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 431 --to 431` (0 hard / 0 soft).

## Seams

`EV_DoDoor`, `EV_StartWaggle`, `EV_DoCeiling`, and destroy iterators are not hosted.

## Credit rationale

Credits door open/raise/animated paths, floor/ceiling waggle args, ceiling instant movers, and destroy sector/TID branches from `p_lnspec.cpp` (bands not in legacy line-specials rows).
