# Chunk 236 — ParseGameInfo GAMEINFOKEY macro expansion tail.

## Scope

ParseGameInfo GAMEINFOKEY macro expansion tail.

## Native ranges

- `src/gamedata/gi.cpp` **ParseGameInfo GAMEINFOKEY tail** 363–471 (109 lines)

**Credited native LOC:** 109 (tally `nativeLoc` 109).

## Managed

(see conversion commit file list)
Managed LOC (tally): 95.
Conversion commit: `20d15060ccd8058244d9e6fca30fb9834482d81c`.

## Tests

- `VmGameInfoParseGameInfoTests.TryParseBlock_ReadsDialogue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 236 --to 236` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
