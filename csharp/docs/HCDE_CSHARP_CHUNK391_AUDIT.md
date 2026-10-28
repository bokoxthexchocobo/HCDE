# Chunk 391 — LoadSideDefs2 sector fixups and VerifyBlockMap validation Retired (2026-10-23): out-of-body credit removed; honest **114** < **150**.

## Scope

LoadSideDefs2 sector fixups and VerifyBlockMap validation Retired (2026-10-23): out-of-body credit removed; honest **114** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `69213d7f89d11aace82ee8c37fd4f74180f0bdca`.

## Padding review (2026-10-21)



### Out-of-body uncredit (391)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **114** |
| **Out-of-body removed** | **0** |
| **Honest** | **114** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 391 --to 391` (0 hard / 0 soft).

## Seams

`mapsidedef_t` IO and blockmap lump memory are not hosted.

## Credit rationale

Credits `MapLoader::LoadSideDefs2` and `FBlockmap::VerifyBlockMap` from `maploader.cpp`.
