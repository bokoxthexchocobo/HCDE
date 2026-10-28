# Chunk 353 — AMColorset init and line-color table Retired (2026-10-23): out-of-body credit removed; honest **44** < **150**.

## Scope

AMColorset init and line-color table Retired (2026-10-23): out-of-body credit removed; honest **44** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `38ce1c54467345ab6e29d3d8fe6e3288f6e9c12f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (353)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **89** |
| **Out-of-body removed** | **45** |
| **Honest** | **44** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 353 --to 353` (0 hard / 0 soft).

## Seams

CVAR pointer tables and `CCMD(am_restorecolors)` stay native; managed code mirrors `initFromColors` unused triplets, almost-background RGB math, and `AUTOMAP_LINE_COLORS` slot mapping.

## Credit rationale

Credits `am_map.cpp` 426–451 (`initFromCVars` almost-background), 453–470 (`initFromColors` unused marker), 498–514 (`AUTOMAP_LINE_COLORS`), and Doom palette seed 599–626 used by predefined colorsets.
