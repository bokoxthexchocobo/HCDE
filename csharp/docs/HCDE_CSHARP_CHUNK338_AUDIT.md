# Chunk 338 — EMAPINFO parse loop section properties and compat API tail Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Scope

EMAPINFO parse loop section properties and compat API tail Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `5ba237cb07158c59b108e6f249fa50744a0763c5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (338)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **56** |
| **Out-of-body removed** | **2** |
| **Honest** | **54** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 338 --to 338` (0 hard / 0 soft).

## Seams

`GetOrCreateLevel` and `ApplyEMapInfoProperty` dispatch remain injectable; this chunk models sky2 mirroring, registry lookup, and extradata lump resolution.

## Credit rationale

Credits `hcde_emapinfo.cpp` 847–921: section open property application with `-NOFLAT-` sky2 mirroring, unsupported-property outside map blocks, and `HCDE_EMapInfo_IsEternityMap` / `HCDE_EMapInfo_FindExtraDataLump` compatibility helpers.
