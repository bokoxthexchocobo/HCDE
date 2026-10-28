# Chunk 337 — ParseMapDefinition close and ParseEpisodeInfo teaser head

## Scope

ParseMapDefinition close and ParseEpisodeInfo teaser head

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapDefinition tail comment** 2286–2294 (9 lines)
- `src/gamedata/g_mapinfo.cpp` **ParseEpisodeInfo map teaser** 2444–2475 (32 lines)

**Credited native LOC:** 41 (tally `nativeLoc` 41).

## Managed

(see conversion commit file list)
Managed LOC (tally): 12.
Conversion commit: `348c44a1ba407a6393d5c04f0198c68bd6832e35`.

## Tests

- `VmMapInfoEpisodeParserTests.TryResolveTeaserMap_UsesTeaserWhenShareware`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 337 --to 337` (0 hard / 0 soft).

## Seams

Scanner `CheckEndOfFile` and shareware `gameinfo` flags are modeled as pure string helpers on `VmMapInfoEpisodeParser`.

## Credit rationale

Fills the small `g_mapinfo.cpp` gaps 2286–2294 (post-`ParseMapDefinition` comment header before file-local `GetDefaultLevelNum`) and 2444–2475 (`ParseEpisodeInfo` prologue through shareware teaser map selection).
