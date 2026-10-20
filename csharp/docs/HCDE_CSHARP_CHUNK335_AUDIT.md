# Chunk 335 — EMAPINFO ReportStats and HCDE_ParseEMapInfo scan head

## Scope

EMAPINFO ReportStats and HCDE_ParseEMapInfo scan head

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ReportStats HCDE_ParseEMapInfo head** 779–832 (54 lines)

**Credited native LOC:** 54 (tally `nativeLoc` 54).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 30.
Conversion commit: `9a0d96615886ade71afa3c10599424f8ae704a40`.

## Tests

- `VmEMapInfoPropertyTailTests.FormatReportSummary_IncludesCounts`
- `VmEMapInfoDocumentScannerTests.TryParseSectionHeader_ReadsMap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 335 --to 335` (0 hard / 0 soft).

## Seams

`Printf` and `fileSystem.ReadFile` are not re-hosted; the scan loop models line iteration and section headers only.

## Credit rationale

Covers `hcde_emapinfo.cpp` 779–832: `ReportStats` summary formatting/sample notes and the opening of `HCDE_ParseEMapInfo` through the first section-header branch, complementing chunk 295's later document rows.
