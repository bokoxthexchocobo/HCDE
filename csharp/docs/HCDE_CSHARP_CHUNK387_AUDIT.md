# Chunk 387 — GetCaps, DrawPaletteTester, DrawRateStuff, lag HUD overlays Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **153**).

## Scope

GetCaps, DrawPaletteTester, DrawRateStuff, lag HUD overlays Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **153**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7a59de0ec8edd5060e0fa88cc0f6753ee9dbc980`.

## Padding review (2026-10-21)



### Out-of-body uncredit (387)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **0** |
| **Honest** | **153** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 387 --to 387` (0 hard / 0 soft).

## Seams

`ClearRect`/`DrawText` and hardware renderer detection are not hosted.

## Credit rationale

Credits `GetCaps`, `DrawPaletteTester`, `DrawRateStuff`, `DrawHCDELagHUD`, and `DrawOverlays` from `d_main.cpp`.
