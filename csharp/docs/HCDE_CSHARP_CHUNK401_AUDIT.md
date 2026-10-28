# Chunk 401 — SetTexture gaps, SetTextureNoErr, LoadSegs preamble and split-vertex snap Retired (2026-10-24): thin managed at HEAD — surviving **0** < **80** (50% of honest **159**).

## Scope

SetTexture gaps, SetTextureNoErr, LoadSegs preamble and split-vertex snap Retired (2026-10-24): thin managed at HEAD — surviving **0** < **80** (50% of honest **159**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6eb04ee65a97edc539297e51d3e9235097ec8de0`.

## Padding review (2026-10-21)



### Out-of-body uncredit (401)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **207** |
| **Out-of-body removed** | **48** |
| **Honest** | **159** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 401 --to 401` (0 hard / 0 soft).

## Seams

`TexMan`, seg iteration, and vertex mutation are not hosted.

## Credit rationale

Credits non-overlapping `SetTexture`/`SetTextureNoErr` bands plus `LoadSegs` preamble and split-vertex angle correction from `maploader.cpp`.
