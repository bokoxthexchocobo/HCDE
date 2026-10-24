# Chunk 374 — FloodZones sound zones and extended/ZDoom node lump routing Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **154**).

## Scope

FloodZones sound zones and extended/ZDoom node lump routing Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **154**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `cab3a6f11a815d5f1c5583aaac6a13f4ce71e9c8`.

## Padding review (2026-10-21)



### Out-of-body uncredit (374)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **154** |
| **Out-of-body removed** | **0** |
| **Honest** | **154** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 374 --to 374` (0 hard / 0 soft).

## Seams

Recursive `FloodZone`, decompressors, and `LoadZNodes` IO are not hosted; managed code mirrors zone assignment and node id routing.

## Credit rationale

Credits `FloodZone`, `FloodZones`, `LoadExtendedNodes`, and `P_CheckV4Nodes` from `maploader.cpp`.
