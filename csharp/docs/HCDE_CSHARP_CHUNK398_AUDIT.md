# Chunk 398 — LoadBehavior, GetPolySpots, CalcIndices, LoadLevel translator preamble Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Scope

LoadBehavior, GetPolySpots, CalcIndices, LoadLevel translator preamble Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2a687196ed646a2b463a2166a23a6a047e0171e5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (398)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 398 --to 398` (0 hard / 0 soft).

## Seams

`MapData` IO, `P_LoadTranslator`, and node build orchestration are not hosted.

## Credit rationale

Credits `MapLoader::LoadBehavior`, `GetPolySpots`, `CalcIndices`, and the behavior/translator/maptype preamble of `LoadLevel` from `maploader.cpp`.
