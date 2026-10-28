# Chunk 386 — D_Display startup gates, wipe selection, level HUD branch, present path Retired (2026-10-24): thin managed at HEAD — surviving **0** < **102** (50% of honest **203**).

## Scope

D_Display startup gates, wipe selection, level HUD branch, present path Retired (2026-10-24): thin managed at HEAD — surviving **0** < **102** (50% of honest **203**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `88edc7807ba71cb20eef5161c3e9bb8fd9999bf2`.

## Padding review (2026-10-21)



### Out-of-body uncredit (386)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **203** |
| **Out-of-body removed** | **0** |
| **Honest** | **203** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 386 --to 386` (0 hard / 0 soft).

## Seams

`RenderView`, `PerformWipe`, and `StatusBar` drawing are not hosted.

## Credit rationale

Credits `D_Display` from `d_main.cpp`.
