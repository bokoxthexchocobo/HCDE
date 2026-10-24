# Chunk 306 — G_ParseMapInfo COMPLVL compatibility presets

## Scope

G_ParseMapInfo COMPLVL compatibility presets

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **G_ParseMapInfo COMPLVL** 2968–3006 (39 lines)

**Credited native LOC:** 39 (tally `nativeLoc` 39).

## Managed

`VmMapInfoComplvlParser.cs`, `VmMapInfoComplvlParserTests.cs`
Managed LOC (tally): 46.
Conversion commit: `dd76fb49074e0d46ee2a05876b18ac5c0aeae7dc`.

## Tests

- `VmMapInfoComplvlParserTests.TryParseComplvl_VanillaPreset`
- `VmMapInfoComplvlParserTests.TryParseComplvl_BoomPreset`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 306 --to 306` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
