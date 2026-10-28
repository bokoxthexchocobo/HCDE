# Chunk 378 — drawThings visibility, sprite rotation index, and thing color classification Retired (2026-10-24): thin managed at HEAD — surviving **0** < **87** (50% of honest **174**).

## Scope

drawThings visibility, sprite rotation index, and thing color classification Retired (2026-10-24): thin managed at HEAD — surviving **0** < **87** (50% of honest **174**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `162e221a77541c1ed8ebd40ccd91114a6af77c61`.

## Padding review (2026-10-21)



### Out-of-body uncredit (378)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **174** |
| **Out-of-body removed** | **0** |
| **Honest** | **174** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 378 --to 378` (0 hard / 0 soft).

## Seams

`DrawMarker`, sector thing iterators, and texture lookup are not hosted.

## Credit rationale

Credits `DAutomap::drawKeys` and `DAutomap::drawThings` from `am_map.cpp`.
