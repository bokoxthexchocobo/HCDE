# Chunk 221 — UMAPINFO extended properties, map lump ingest, CommitUMapinfo.

## Scope

UMAPINFO extended properties, map lump ingest, CommitUMapinfo.

## Native ranges

- `src/gamedata/umapinfo.cpp` **ParseStandardProperty intertext through freeaim** 270–394 (125 lines)
- `src/gamedata/umapinfo.cpp` **ParseMapEntry** 402–414 (13 lines)
- `src/gamedata/umapinfo.cpp` **ParseUMapInfo** 422–471 (50 lines)
- `src/gamedata/umapinfo.cpp` **CommitUMapinfo** 475–625 (151 lines)

**Credited native LOC:** 339 (tally `nativeLoc` 339).

## Managed

`VmUMapInfoEpisodeParser.cs`, `VmUMapInfoParser.cs`, `VmUMapInfoParserTests.cs`, `VmUMapInfoStandardPropertyParser.cs`
Managed LOC (tally): 220.
Conversion commit: `b299de9f9f6a4002d0a0235eaad04f685337cec4`.

## Tests

- `VmUMapInfoParserTests.CommitToLevelInfo_AppliesInterText`
- `VmUMapInfoParserTests.TryParseMapBlock_ReadsBossAction`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 221 --to 221` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
