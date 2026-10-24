# Chunk 352 — Automap ColorNames and AMColorset indices

## Scope

Automap ColorNames and AMColorset indices

## Native ranges

- `src/am_map.cpp` **ColorNames AMColorset indices** 350–424 (75 lines)

**Credited native LOC:** 75 (tally `nativeLoc` 75).

## Managed

`VmAutomapColorsetIndex.cs`, `VmAutomapColorsetIndexTests.cs`
Managed LOC (tally): 28.
Conversion commit: `1e946392b9c02e0c95954a7c76dde10e42d00e8c`.

## Tests

- `VmAutomapColorsetIndexTests.ColorNames_MatchesNativeTableLength`
- `VmAutomapColorsetIndexTests.Indices_AlignWithFirstEntries`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 352 --to 352` (0 hard / 0 soft).

## Seams

Runtime `AMColorset` population from CVARs is not hosted; managed code mirrors the static name table and enum ordinals used when building custom color sets.

## Credit rationale

Continues fresh `am_map.cpp` after chunk 343: credits 350–424 covering `ColorNames[]` labels and the `AMColorset` slot indices consumed when overlay and classic automap palettes are assembled.
