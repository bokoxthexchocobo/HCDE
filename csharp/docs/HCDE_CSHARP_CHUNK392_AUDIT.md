# Chunk 392 — D_Render, D_DoomLoop timing, page drawer/ticker Retired (2026-10-23): out-of-body credit removed; honest **102** < **150**.

## Scope

D_Render, D_DoomLoop timing, page drawer/ticker Retired (2026-10-23): out-of-body credit removed; honest **102** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `21e1f66f4f14781aa56d22c87266ea3897ff60b1`.

## Padding review (2026-10-21)



### Out-of-body uncredit (392)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **102** |
| **Out-of-body removed** | **0** |
| **Honest** | **102** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 392 --to 392` (0 hard / 0 soft).

## Seams

`TryRunTics`, `D_Display`, and texture draw calls are not hosted.

## Credit rationale

Credits `D_DoomLoop` (including recoverable error paths), `D_PageTicker`, and `D_PageDrawer` from `d_main.cpp`.
