# Chunk 380 — MakeSkill, LoadThings doom conversion, and LoadThings2 hexen read defaults Retired (2026-10-23): out-of-body credit removed; honest **134** < **150**.

## Scope

MakeSkill, LoadThings doom conversion, and LoadThings2 hexen read defaults Retired (2026-10-23): out-of-body credit removed; honest **134** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `885197d0945adfbb0cacb95e318c43ca34311f2a`.

## Padding review (2026-10-21)



### Out-of-body uncredit (380)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **134** |
| **Out-of-body removed** | **0** |
| **Honest** | **134** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 380 --to 380` (0 hard / 0 soft).

## Seams

`DoomEdMap`, `ProcessEDMapthing`, and `SpawnMapThing` are not hosted.

## Credit rationale

Credits `MakeSkill`, `MapLoader::LoadThings`, and `MapLoader::LoadThings2` from `maploader.cpp`.
