# Chunk 419 — lookspring/mouse capture CVARs and System_* input routing Retired (2026-10-23): out-of-body credit removed; honest **141** < **150**.

## Scope

lookspring/mouse capture CVARs and System_* input routing Retired (2026-10-23): out-of-body credit removed; honest **141** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3f4254c963817f558d9ae183115e829214d9e896`.

## Padding review (2026-10-21)



### Out-of-body uncredit (419)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **18** |
| **Honest** | **141** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 419 --to 419` (0 hard / 0 soft).

## Seams

Live console/menu state, `debugServer`, and `G_AddView*` are not hosted.

## Credit rationale

Credits mouse-capture CVAR clamping, GUI capture policy, direct-mouse look routing, and capture-mode gamestate gates from `d_main.cpp`.
