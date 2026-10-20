# Chunk 237 — DEFINE_GLOBAL/DEFINE_FIELD gameinfo exports.

## Scope

DEFINE_GLOBAL/DEFINE_FIELD gameinfo exports.

## Native ranges

- `src/gamedata/gi.cpp` **DEFINE_GLOBAL gameinfo** 39–39 (1 lines)
- `src/gamedata/gi.cpp` **DEFINE_FIELD gameinfo_t** 40–71 (32 lines)
- `src/gamedata/gi.cpp` **GetFinalePage gap** 478–478 (1 lines)

**Credited native LOC:** 34 (tally `nativeLoc` 34).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: ``.

## Tests

- `VmGiNativeExportRecognitionTests.GameInfoVmFields_IncludesGibfactor`
- `VmGameInfoParserTests.GetFinalePage_WrapsBounds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 237 --to 237` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
