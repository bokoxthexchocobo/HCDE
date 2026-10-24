# Chunk 343 — Automap togglemap/am_zoom and AMColor (trimmed re-audit) Retired (2026-10-23): out-of-body credit removed; honest **29** < **150**.

## Scope

Automap togglemap/am_zoom and AMColor (trimmed re-audit) Retired (2026-10-23): out-of-body credit removed; honest **29** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3c5a4003326fc149774c448f38bb54b75201e8de`.

## Padding review (2026-10-21)



### Out-of-body uncredit (343)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **36** |
| **Out-of-body removed** | **7** |
| **Honest** | **29** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 343 --to 343` (0 hard / 0 soft).

## Seams

Console `CCMD` registration and `primaryLevel->automap` callbacks stay native; managed code models argument parsing and ARGB color validity only.

## Credit rationale

Credits only `CCMD(togglemap)` idle guard, `CCMD(am_zoom)` argv parse, and `AMColor` RGB validity — not CVAR registration blocks.
