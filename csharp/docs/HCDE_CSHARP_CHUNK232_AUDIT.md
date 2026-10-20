# Chunk 232 — FDecalGroup, FDecalLib clear/tree, decal keyword schema, animator structs header.

## Scope

FDecalGroup, FDecalLib clear/tree, decal keyword schema, animator structs header.

## Native ranges

- `src/gamedata/decallib.cpp` **FDecalGroup and FDecalLib header** 1–222 (222 lines)
- `src/gamedata/decallib.cpp` **FDecalBase stubs gap** 238–238 (1 lines)

**Credited native LOC:** 223 (tally `nativeLoc` 223).

## Managed

`VmDecalDocumentLoader.cs`, `VmDecalDocumentLoaderTests.cs`, `VmDecalKeywordSchema.cs`, `VmDecalKeywordSchemaTests.cs`, `VmDecalLibCore.cs`, `VmDecalLibCoreTests.cs`, `VmDecalLibrary.cs`, `VmDoomFontConstants.cs`, `VmDoomFontConstantsTests.cs`, `VmGameInfoParseGameInfo.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmGameInfoStaticSchema.cs`, `VmGameInfoStaticSchemaTests.cs`, `VmGiNativeExportRecognition.cs`, `VmGiNativeExportRecognitionTests.cs`
Managed LOC (tally): 180.
Conversion commit: `91499df55478b48ada6d426351cae028ebb4920e`.

## Tests

- `VmDecalLibCoreTests.DecalGroup_PickDecal_UsesWeights`
- `VmDecalKeywordSchemaTests.DecalKeywords_EndsWithTranslatable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 232 --to 232` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
