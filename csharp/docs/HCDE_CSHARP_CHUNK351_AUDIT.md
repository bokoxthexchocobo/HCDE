# Chunk 351 — D_Main timescale and rendermode policy (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **13** < **150**.

## Scope

D_Main timescale and rendermode policy (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **13** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `909708aa9211c76a80912050be61e19be175cf45`.

## Padding review (2026-10-21)



### Out-of-body uncredit (351)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **13** |
| **Out-of-body removed** | **0** |
| **Honest** | **13** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 351 --to 351` (0 hard / 0 soft).

## Seams

Engine CVAR registration and renderer mode switching are not re-hosted; policy functions mirror net-game timescale, wipe bounds, and HUD snapshot behavior.

## Credit rationale

Credits `i_timescale` net-game reset and `vid_rendermode` clamp/softpoly remap only.
