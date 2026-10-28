# Chunk 390 — Generated blockmap extents and line bucketing Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Scope

Generated blockmap extents and line bucketing Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `eb26a6ac379d61961e83e1124e3849fe2d45dca7`.

## Padding review (2026-10-21)



### Out-of-body uncredit (390)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **74** |
| **Out-of-body removed** | **0** |
| **Honest** | **74** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 390 --to 390` (0 hard / 0 soft).

## Seams

`CreatePackedBlockmap`, `TArray`, and `Scale` fixed-point walks are not hosted.

## Credit rationale

Credits `MapLoader::CreateBlockMap` from `maploader.cpp`.
