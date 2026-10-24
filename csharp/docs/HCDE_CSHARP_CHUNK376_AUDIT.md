# Chunk 376 — drawSeg/drawPolySeg rotation policy, AM_Check3DFloors, and automap special boundary helpers Retired (2026-10-24): thin managed at HEAD — surviving **0** < **130** (50% of honest **259**).

## Scope

drawSeg/drawPolySeg rotation policy, AM_Check3DFloors, and automap special boundary helpers Retired (2026-10-24): thin managed at HEAD — surviving **0** < **130** (50% of honest **259**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a9510094cfbcdee7f74a17dfbb5d6f823a29e9ac`.

## Padding review (2026-10-21)



### Out-of-body uncredit (376)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **264** |
| **Out-of-body removed** | **5** |
| **Honest** | **259** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 376 --to 376` (0 hard / 0 soft).

## Seams

`drawMline`, polyobject iterators, and `P_GetLineSpecialInfo` tables are not hosted.

## Credit rationale

Credits `DAutomap::drawSeg`, `drawPolySeg`, `showSS` seg path, `AM_Check3DFloors`, and `AM_checkSectorActions` through `AM_isLockBoundary` from `am_map.cpp`.
