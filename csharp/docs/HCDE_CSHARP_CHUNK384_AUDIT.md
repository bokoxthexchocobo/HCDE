# Chunk 384 — SetMapThingUserData, SpawnThings, AllocateSideDefs, LoopSidedefs vertex linking Retired (2026-10-24): thin managed at HEAD — surviving **0** < **100** (50% of honest **200**).

## Scope

SetMapThingUserData, SpawnThings, AllocateSideDefs, LoopSidedefs vertex linking Retired (2026-10-24): thin managed at HEAD — surviving **0** < **100** (50% of honest **200**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `1359c71bc46ff54bd87d543afe03b880ba159d2e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (384)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **200** |
| **Out-of-body removed** | **0** |
| **Honest** | **200** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 384 --to 384` (0 hard / 0 soft).

## Seams

`PField` reflection, `SpawnMapThing`, and full angle-based right-side search are not hosted.

## Credit rationale

Credits `SetMapThingUserData`, `SpawnThings`, `AllocateSideDefs`, and the opening of `LoopSidedefs` from `maploader.cpp`.
