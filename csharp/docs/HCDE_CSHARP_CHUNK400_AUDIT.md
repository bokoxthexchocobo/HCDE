# Chunk 400 — SetSubsectorLightmap, SetSideLightmap, LoadLightmap ingest Retired (2026-10-24): thin managed at HEAD — surviving **0** < **123** (50% of honest **245**).

## Scope

SetSubsectorLightmap, SetSideLightmap, LoadLightmap ingest Retired (2026-10-24): thin managed at HEAD — surviving **0** < **123** (50% of honest **245**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `10bb8f0d63fa6c760374e5a5ee63a95b5c02ca5e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (400)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **245** |
| **Out-of-body removed** | **0** |
| **Honest** | **245** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 400 --to 400` (0 hard / 0 soft).

## Seams

`OpenDecompressor`, sidedef/subsector mutation, and texture upload are not hosted.

## Credit rationale

Credits `MapLoader::SetSubsectorLightmap`, `SetSideLightmap`, and `LoadLightmap` from `maploader.cpp`.
