# Chunk 394 — GroupLines sector tables and LoadReject matrix ingest Retired (2026-10-24): thin managed at HEAD — surviving **0** < **92** (50% of honest **184**).

## Scope

GroupLines sector tables and LoadReject matrix ingest Retired (2026-10-24): thin managed at HEAD — surviving **0** < **92** (50% of honest **184**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `88dfd7931e35b681c9a5819f3469087ded2c44e6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (394)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **184** |
| **Out-of-body removed** | **0** |
| **Honest** | **184** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 394 --to 394` (0 hard / 0 soft).

## Seams

`Level->tagManager`, `SetSlopes`, and `MapData` lump IO are not hosted.

## Credit rationale

Credits `MapLoader::GroupLines` and `MapLoader::LoadReject` from `maploader.cpp`.
