# Chunk 383 — compatmode presets 8–9 and CalcFps frame accounting Retired (2026-10-23): out-of-body credit removed; honest **37** < **150**.

## Scope

compatmode presets 8–9 and CalcFps frame accounting Retired (2026-10-23): out-of-body credit removed; honest **37** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a4bf031c524cd0ce03503f69878a8278e329cb12`.

## Padding review (2026-10-21)



### Out-of-body uncredit (383)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **37** |
| **Out-of-body removed** | **0** |
| **Honest** | **37** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 383 --to 383` (0 hard / 0 soft).

## Seams

CVAR callback side effects and `screen->FrameTime` are not hosted.

## Credit rationale

Credits `CUSTOM_CVAR(compatmode)` cases 8–9 and `CalcFps` from `d_main.cpp`.
