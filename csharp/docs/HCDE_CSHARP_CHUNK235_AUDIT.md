# Chunk 235 — GameNames, border presets, GAMEINFO macro catalog from gi.cpp header.

## Scope

GameNames, border presets, GAMEINFO macro catalog from gi.cpp header.

## Native ranges

- `src/gamedata/gi.cpp` **gi.cpp file header** 1–33 (33 lines)
- `src/gamedata/gi.cpp` **GameNames and border tables** 73–111 (39 lines)
- `src/gamedata/gi.cpp` **GAMEINFOKEY macro block** 113–258 (146 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

(see conversion commit file list)
Managed LOC (tally): 85.
Conversion commit: `20d15060ccd8058244d9e6fca30fb9834482d81c`.

## Tests

- `VmGameInfoStaticSchemaTests.GameNames_IncludesDoom`
- `VmGameInfoStaticSchemaTests.ResolveBorderPreset_HereticUsesFourBySixteen`
- `VmGiNativeExportRecognitionTests.GameInfoMacroKeys_IncludesTitleMusic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 235 --to 235` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
