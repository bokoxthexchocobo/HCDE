# Chunk 348 — Cluster exit remap ParseNextMap shadow cap (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Scope

Cluster exit remap ParseNextMap shadow cap (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `74780e8c4548efe6df692908d45fe9885a02fb3a`.

## Padding review (2026-10-21)



### Out-of-body uncredit (348)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **54** |
| **Out-of-body removed** | **0** |
| **Honest** | **54** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 348 --to 348` (0 hard / 0 soft).

## Seams

Cutscene parsing and live `fileSystem` lump probes remain injectable dependencies.

## Credit rationale

Credits Hexen `CLUS?MSG` remap, `ParseNextMap` numeric/end-sequence formatting, and shadow-quality quantization only.
