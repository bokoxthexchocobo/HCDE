# Chunk 295 — EMAPINFO [map] section header scan

## Scope

EMAPINFO [map] section header scan

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **HCDE_ParseEMapInfo** 825–922 (98 lines)

**Credited native LOC:** 98 (tally `nativeLoc` 14).

## Managed

`VmEMapInfoDocumentScanner.cs`, `VmEMapInfoDocumentScannerTests.cs`
Managed LOC (tally): 28.
Conversion commit: `b095675caf94ecc94dd28aefcd81252826d2996c`.

## Tests

- `VmEMapInfoDocumentScannerTests.TryParseSectionHeader_ReadsMap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 295 --to 295` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
