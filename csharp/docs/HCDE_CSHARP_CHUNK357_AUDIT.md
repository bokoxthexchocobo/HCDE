# Chunk 357 — Sector SetTexture missing flat warnings and SummarizeMissingTextures

## Scope

Sector SetTexture missing flat warnings and SummarizeMissingTextures

## Native ranges

- `src/maploader/maploader.cpp` **SetTexture sector flat** 183–209 (27 lines)
- `src/maploader/maploader.cpp` **SummarizeMissingTextures** 220–235 (16 lines)

**Credited native LOC:** 43 (tally `nativeLoc` 43).

## Managed

`VmMapLoaderMissingTexturePolicy.cs`, `VmMapLoaderMissingTexturePolicyTests.cs`
Managed LOC (tally): 11.
Conversion commit: `bd4f2ca4d24dac20e0e40f67d1be66221b120868`.

## Tests

- `VmMapLoaderMissingTexturePolicyTests.TruncateFlatName_EightChars`
- `VmMapLoaderMissingTexturePolicyTests.FormatSectorMissingMessage_IncludesIndex`
- `VmMapLoaderMissingTexturePolicyTests.ShouldSummarizeExtraUsage_AfterLimit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 357 --to 357` (0 hard / 0 soft).

## Seams

`TexMan` and `Printf` are not hosted; messages and warn caps mirror native policy.

## Credit rationale

Credits sector `SetTexture` truncate/warn/default path and the missing-texture summary tail loop.
