# Chunk 366 — LoadVertexes, LoadZSegs, LoadGLZSegs, LoadZNodes binary layout and validation Retired (2026-10-24): thin managed at HEAD — surviving **0** < **120** (50% of honest **239**).

## Scope

LoadVertexes, LoadZSegs, LoadGLZSegs, LoadZNodes binary layout and validation Retired (2026-10-24): thin managed at HEAD — surviving **0** < **120** (50% of honest **239**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `42c048f34725aea85feabc9ae102e0c898c6898b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (366)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **239** |
| **Out-of-body removed** | **0** |
| **Honest** | **239** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 366 --to 366` (0 hard / 0 soft).

## Seams

`Level` allocation, `FileReader`, and seg pointer wiring are not hosted; managed code mirrors lump sizing, record parsing, and GL/Z node validation gates.

## Credit rationale

Credits `MapLoader::LoadVertexes`, `LoadZSegs`, `LoadGLZSegs`, and `LoadZNodes` through the classic node child read loop in `maploader.cpp`.
