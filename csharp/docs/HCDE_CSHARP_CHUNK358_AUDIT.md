# Chunk 358 — MapLoader SetTexture colormap and hex blend path Retired (2026-10-23): out-of-body credit removed; honest **27** < **150**.

## Scope

MapLoader SetTexture colormap and hex blend path Retired (2026-10-23): out-of-body credit removed; honest **27** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `378cd8bff52ac5d8cd760485d162f0d2a80fdb7d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (358)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **27** |
| **Out-of-body removed** | **0** |
| **Honest** | **27** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 358 --to 358` (0 hard / 0 soft).

## Seams

`R_ColormapNumForName` and sidedef mutation are injected; blend resolution returns flags only.

## Credit rationale

Credits deep-water sidedef `SetTexture` that prefers colormap numbers, wall textures, then 8-char hex blends.
