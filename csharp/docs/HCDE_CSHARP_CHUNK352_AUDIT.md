# Chunk 352 — Automap ColorNames table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Automap ColorNames table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `623288c17a174b387f38cbe1f815118370373066`.

## Padding review (2026-10-21)



### Out-of-body uncredit (352)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **26** |
| **Out-of-body removed** | **26** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 352 --to 352` (0 hard / 0 soft).

## Seams

Runtime `AMColorset` population from CVARs is not hosted; managed code mirrors the static name table and enum ordinals used when building custom color sets.

## Credit rationale

Credits static `ColorNames[]` labels mirrored by `VmAutomapColorsetIndex.ColorNames`.
