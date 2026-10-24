# Chunk 389 — ProcessSideTextures specials and LoadBlockMap lump ingest Retired (2026-10-24): `--band` hull overlaps existing credit (maploader-create-blockmap-chunk390 src/maploader/maploader.cpp 2324-2397).

## Scope

ProcessSideTextures specials and LoadBlockMap lump ingest Retired (2026-10-24): `--band` hull overlaps existing credit (maploader-create-blockmap-chunk390 src/maploader/maploader.cpp 2324-2397).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `fa99e6d4b914d1728f21963ab6e7ed74b05e2fa6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (389)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 389 --to 389` (0 hard / 0 soft).

## Seams

`SetTexture`, `CreateBlockMap`, and `GPalette::DetermineTranslucency` are not hosted.

## Credit rationale

Credits `MapLoader::ProcessSideTextures` and `MapLoader::LoadBlockMap` from `maploader.cpp`.
