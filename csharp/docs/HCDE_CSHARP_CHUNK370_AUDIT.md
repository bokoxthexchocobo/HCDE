# Chunk 370 — drawSubsectors skip/textured/fade/triangulation policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **123** (50% of honest **246**).

## Scope

drawSubsectors skip/textured/fade/triangulation policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **123** (50% of honest **246**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f3b5195ee5366c80cbf3b1d7c1a237e179d4a8d3`.

## Padding review (2026-10-21)



### Out-of-body uncredit (370)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **246** |
| **Out-of-body removed** | **0** |
| **Honest** | **246** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 370 --to 370` (0 hard / 0 soft).

## Seams

`twod->AddPoly`, `AM_FakeFlat`, and earcut triangulation are not hosted; managed code mirrors visibility gates and lighting/fade math.

## Credit rationale

Credits `DAutomap::drawSubsectors` from `am_map.cpp`.
