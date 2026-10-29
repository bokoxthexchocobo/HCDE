# Chunk 1203 — Steam libraryfolders.vdf paths and installed-game directories from d_steam.cpp (1–164).

## Scope

Steam libraryfolders.vdf paths and installed-game directories from d_steam.cpp (1–164).

## Native ranges

- `src/d_steam.cpp` **D_GetSteamGamePaths** 1–164 (164 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmSteamPaths1203Slice.cs`
Managed LOC (tally): 238.
Conversion commit: `52d5d93770d17665d055411f18c40a47f904f26d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SteamAppInfoList (**32–56**) | yes |
| PSR_FindEndBlock (**58–71**) | yes |
| D_ParseSteamRegistry (**73–112**) | yes |
| D_GetSteamGamePaths (**114–164**) | yes |
| Band hull (**1–164**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

D_ParseSteamRegistry returns an empty list when the file cannot be opened. Otherwise it reads the root string and the opening brace, then each library. A path key, matched without case, appends /steamapps/common and skips the rest of that library. Any other key that opens a brace is skipped as a block. Any other key consumes one string value. D_GetSteamGamePaths returns empty when the Steam root is empty or missing. A parse error returns empty and does not add the canon folder. Each library folder replaces backslashes with slashes one for one, then gains a trailing slash. The canon folder SteamPath/steamapps/common/ is always added after a successful parse. Each app base path that exists is pushed.

| Managed (`VmSteamPaths1203Slice.cs`) | Native (`d_steam.cpp`) |
|--------------------------------------|------------------------|
| Steam paths | **1–164** |
| Band hull (exclusive slice) | **1–164** |

### Chunk **1203** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **164** |
| **Net headline** | **+164** vs post-**1202** |


**Post-review ratio:** 238/164 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmSteamPaths1203SliceTests.ParseRegistry_ReadsLibraryPathsAndInstalledGames`
- `VmSteamPaths1203SliceTests.MissingRoot_AndParseErrors_SkipTheCanonPath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1203 --to 1203` (0 hard / 0 soft).

## Seams

The registry text stands in for OpenFile of libraryfolders.vdf. Null text is a missing file and yields no libraries. A C-mode parse error is the recoverable catch and returns no games, including the canon folder. String tokens keep backslashes and turn only a backslash-quote pair into a quote. // and block comments are skipped. #region lines are not special.

## Credit rationale

UNCredited d_steam.cpp 1–164 (+164). The file ends at 164. A full tally line scan shows no overlap.
