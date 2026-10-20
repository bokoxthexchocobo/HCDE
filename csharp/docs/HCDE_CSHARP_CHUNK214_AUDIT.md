# Chunk 214 — ParseSpawnMap and InitClassMap.

## Scope

ParseSpawnMap and InitClassMap.

## Native ranges

- `src/gamedata/g_doomedmap.cpp` **ParseSpawnMap** 361–405 (45 lines)
- `src/gamedata/g_doomedmap.cpp` **InitClassMap** 420–451 (32 lines)

**Credited native LOC:** 77 (tally `nativeLoc` 77).

## Managed

`VmDoomEdSpawnMapParser.cs`, `VmDoomEdSpawnMapParserTests.cs`
Managed LOC (tally): 77.
Conversion commit: `769f4fb5c53a9b8838f40dce58efc7ff2bb528ec`.

## Tests

- `VmDoomEdSpawnMapParserTests.TryParseSpawnMap_ReadsPairs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 214 --to 214` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
