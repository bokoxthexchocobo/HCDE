# Chunk 230 — TERRAIN header/enums and keyword tables (gaps not in prior terrain chunks).

## Scope

TERRAIN header/enums and keyword tables (gaps not in prior terrain chunks).

## Native ranges

- `src/gamedata/p_terrain.cpp` **Terrain enums and file header** 1–78 (78 lines)
- `src/gamedata/p_terrain.cpp` **Terrain prototypes and keyword arrays** 101–184 (84 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmTerrainInit.cs`, `VmTerrainKeywordSchema.cs`, `VmTerrainKeywordSchemaTests.cs`
Managed LOC (tally): 75.
Conversion commit: `797d52b69f89e166ce3586d434a59edd79dc13a7`.

## Tests

- `VmTerrainKeywordSchemaTests.OuterKeywords_MatchesNativeTable`
- `VmTerrainKeywordSchemaTests.LoadTerrainDocuments_ParsesSplashBlock`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 230 --to 230` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
