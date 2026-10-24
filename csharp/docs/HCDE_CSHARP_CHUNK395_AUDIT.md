# Chunk 395 — CVARINFO scanner and deh/bex patch discovery Retired (2026-10-24): thin managed at HEAD — surviving **0** < **87** (50% of honest **173**).

## Scope

CVARINFO scanner and deh/bex patch discovery Retired (2026-10-24): thin managed at HEAD — surviving **0** < **87** (50% of honest **173**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6086b48a0e0832f85c17e6b1e52183d39d5c51e3`.

## Padding review (2026-10-21)



### Out-of-body uncredit (395)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **173** |
| **Out-of-body removed** | **0** |
| **Honest** | **173** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 395 --to 395` (0 hard / 0 soft).

## Seams

`FScanner`, `C_CreateCVar`, and `D_LoadDehFile` are not hosted.

## Credit rationale

Credits `ParseCVarInfo` and `ConsiderPatches` from `d_main.cpp`.
