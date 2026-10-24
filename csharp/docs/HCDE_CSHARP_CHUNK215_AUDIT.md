# Chunk 215 — UMapInfo multi-string, lump name, player action option.

## Scope

UMapInfo multi-string, lump name, player action option.

## Native ranges

- `src/gamedata/umapinfo.cpp` **ParseMultiString** 84–111 (28 lines)
- `src/gamedata/umapinfo.cpp` **ParseLumpName** 119–129 (11 lines)
- `src/gamedata/umapinfo.cpp` **ParsePlayerActionOption** 137–155 (19 lines)

**Credited native LOC:** 58 (tally `nativeLoc` 58).

## Managed

`VmUMapInfoTokenParser.cs`
Managed LOC (tally): 56.
Conversion commit: `be741dc66b574fb0abbe9db7bf57d7ea85af22d5`.

## Tests

- `VmUMapInfoTokenParserTests.TryParseMultiString_JoinsQuotedParts`
- `VmUMapInfoTokenParserTests.TryParsePlayerActionOption_ParsesAllow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 215 --to 215` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
