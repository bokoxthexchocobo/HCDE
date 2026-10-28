# Chunk 403 — BlockHash, BlockCompare, CreatePackedBlockmap, CreateBlockMap span walks Retired (2026-10-24): thin managed at HEAD — surviving **0** < **91** (50% of honest **182**).

## Scope

BlockHash, BlockCompare, CreatePackedBlockmap, CreateBlockMap span walks Retired (2026-10-24): thin managed at HEAD — surviving **0** < **91** (50% of honest **182**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d88b8bbfa3307e249d973f63f00362232e8a7eac`.

## Padding review (2026-10-21)



### Out-of-body uncredit (403)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **182** |
| **Out-of-body removed** | **0** |
| **Honest** | **182** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 403 --to 403` (0 hard / 0 soft).

## Seams

`TArray`, `memset`, and `Scale` fixed-point walks are not hosted.

## Credit rationale

Credits `BlockHash`, `BlockCompare`, `CreatePackedBlockmap`, and the line-span tail of `MapLoader::CreateBlockMap` from `maploader.cpp`.
