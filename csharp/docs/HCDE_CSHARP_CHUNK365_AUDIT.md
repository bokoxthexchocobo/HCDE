# Chunk 365 — Automap drawGrid block-aligned lines and AM_FakeFlat heightsec routing Retired (2026-10-24): thin managed at HEAD — surviving **0** < **84** (50% of honest **168**).

## Scope

Automap drawGrid block-aligned lines and AM_FakeFlat heightsec routing Retired (2026-10-24): thin managed at HEAD — surviving **0** < **84** (50% of honest **168**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `46f822abc263d4bd20adcc626536a9d28fb06895`.

## Padding review (2026-10-21)



### Out-of-body uncredit (365)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **168** |
| **Out-of-body removed** | **0** |
| **Honest** | **168** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 365 --to 365` (0 hard / 0 soft).

## Seams

`rotatePoint`, `drawMline`, and live `sector_t` mutation are not hosted; managed code mirrors grid iteration math and fake-flat area/texture policy.

## Credit rationale

Credits `DAutomap::drawGrid` and free function `AM_FakeFlat` from `am_map.cpp`.
