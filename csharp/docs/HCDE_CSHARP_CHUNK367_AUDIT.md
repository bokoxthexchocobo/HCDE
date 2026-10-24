# Chunk 367 — LoadSectors defaults/fade policy and LoadNodes BSP child validation Retired (2026-10-24): thin managed at HEAD — surviving **0** < **83** (50% of honest **166**).

## Scope

LoadSectors defaults/fade policy and LoadNodes BSP child validation Retired (2026-10-24): thin managed at HEAD — surviving **0** < **83** (50% of honest **166**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `14012059e1173261eaf86d54b807ec27d18892ca`.

## Padding review (2026-10-21)



### Out-of-body uncredit (367)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **166** |
| **Out-of-body removed** | **0** |
| **Honest** | **166** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 367 --to 367` (0 hard / 0 soft).

## Seams

Sector allocation, `SetTexture`, and pointer wiring are not hosted; managed code mirrors seq-type defaults, behavior flags, fade selection, and BSP child checks.

## Credit rationale

Credits `MapLoader::LoadSectors` and template `MapLoader::LoadNodes` from `maploader.cpp`.
