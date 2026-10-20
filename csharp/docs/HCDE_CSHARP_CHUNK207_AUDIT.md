# Chunk 207 — KEYCONF lump comment strip, addkeysection/addmenukey, LoadKeys, player class KEYCONF helpers.

## Scope

KEYCONF lump comment strip, addkeysection/addmenukey, LoadKeys, player class KEYCONF helpers.

## Native ranges

- `src/gamedata/keysections.cpp` **LoadKeys** 39–55 (17 lines)
- `src/gamedata/keysections.cpp` **CCMD addkeysection** 81–115 (35 lines)
- `src/gamedata/keysections.cpp` **CCMD addmenukey** 117–138 (22 lines)
- `src/gamedata/keysections.cpp` **D_LoadWadSettings** 150–237 (88 lines)
- `src/gamedata/keysections.cpp` **ClearIWADPlayerClasses** 243–256 (14 lines)
- `src/gamedata/keysections.cpp` **CCMD clearplayerclasses** 258–279 (22 lines)
- `src/gamedata/keysections.cpp` **CCMD addplayerclass** 283–323 (41 lines)

**Credited native LOC:** 239 (tally `nativeLoc` 239).

## Managed

`VmKeyConfIniLoader.cs`, `VmKeyConfIniLoaderTests.cs`, `VmKeyConfParser.cs`, `VmKeyConfParserTests.cs`, `VmKeyConfPlayerParser.cs`, `VmKeyConfPlayerParserTests.cs`
Managed LOC (tally): 168.
Conversion commit: `b1dd52030f1960b14410142eff9a8ad2718f1e9e`.

## Tests

- `VmKeyConfIniLoaderTests.LoadKeys_BindsFromGameConfigSection`
- `VmKeyConfIniLoaderTests.LoadKeysForSection_LoadsSingleAndDouble`
- `VmKeyConfParserTests.TryAddKeySection_DeduplicatesByIniNameAgainstTitle`
- `VmKeyConfParserTests.TryAddMenuKey_RequiresSection`
- `VmKeyConfParserTests.ParseKeyConfLumpCommands_StripsCommentsAcrossLines`
- `VmKeyConfParserTests.StripKeyConfComments_RemovesLineAndBlockComments`
- `VmKeyConfPlayerParserTests.ClearPlayerClasses_NonStrictRemovesIwadNames`
- `VmKeyConfPlayerParserTests.ClearPlayerClasses_StrictClearsAll`
- `VmKeyConfPlayerParserTests.TryAddPlayerClass_DeduplicatesAndParsesNomenu`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 207 --to 207` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
