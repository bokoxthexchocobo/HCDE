# Chunk 363 — am_toggletexture and am_setmark CCMD guards Retired (2026-10-23): out-of-body credit removed; honest **10** < **150**.

## Scope

am_toggletexture and am_setmark CCMD guards Retired (2026-10-23): out-of-body credit removed; honest **10** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2a6ccce1de13a2d7ed98d3cd135d6a112c77aa93`.

## Padding review (2026-10-21)



### Out-of-body uncredit (363)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **10** |
| **Out-of-body removed** | **0** |
| **Honest** | **10** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 363 --to 363` (0 hard / 0 soft).

## Seams

Engine automap instance methods are not hosted; CCMDs reduce to toggles and null guards.

## Credit rationale

Credits `CCMD(am_toggletexture)` and the `am_setmark` automap presence gate from `am_map.cpp`.
