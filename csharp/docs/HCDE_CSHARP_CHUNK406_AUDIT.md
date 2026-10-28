# Chunk 406 — Classic node fallback and LoadLevel finalize/subsector bounds Retired (2026-10-24): thin managed at HEAD — surviving **0** < **98** (50% of honest **196**).

## Scope

Classic node fallback and LoadLevel finalize/subsector bounds Retired (2026-10-24): thin managed at HEAD — surviving **0** < **98** (50% of honest **196**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7f81aa6db991b410c2de26d8f4ce7bf98dc4fc7d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (406)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **196** |
| **Out-of-body removed** | **0** |
| **Honest** | **196** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 406 --to 406` (0 hard / 0 soft).

## Seams

`LoadGLNodes`, `CreateVBO`, and `SpawnThings` are not hosted.

## Credit rationale

Credits classic node-load fallback in `LoadLevel` and the post-spawn finalize/subsector bbox loop from `maploader.cpp`.
