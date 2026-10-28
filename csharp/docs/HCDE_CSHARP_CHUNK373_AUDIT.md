# Chunk 373 — D_ToggleHud, D_Render, dmflags/dmflags2, compatmode presets Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-doomloop-page-chunk392 src/d_main.cpp 614-617).

## Scope

D_ToggleHud, D_Render, dmflags/dmflags2, compatmode presets Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-doomloop-page-chunk392 src/d_main.cpp 614-617).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `26f3cc8bfda0a2f8842885a3f327966ba764936d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (373)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **156** |
| **Out-of-body removed** | **0** |
| **Honest** | **156** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 373 --to 373` (0 hard / 0 soft).

## Seams

Network writes, `AM_Stop`, and renderer interpolation are not hosted; managed code mirrors gating predicates only.

## Credit rationale

Credits `D_ToggleHud`, `D_Render`, `CUSTOM_CVAR(dmflags)`, `CUSTOM_CVAR(dmflags2)`, and `CUSTOM_CVAR(compatmode)` presets from `d_main.cpp`.
