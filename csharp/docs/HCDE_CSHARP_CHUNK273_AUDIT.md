# Chunk 273 — EMAPINFO trim/quote/comment/property helpers. (re-audit 265-286: tested ranges only)

## Scope

EMAPINFO trim/quote/comment/property helpers. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **EMAPINFO trim/quote** 55–112 (58 lines)
- `src/gamedata/hcde_emapinfo.cpp` **EMAPINFO comments** 124–148 (25 lines)
- `src/gamedata/hcde_emapinfo.cpp` **EMAPINFO property split** 150–223 (74 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 43).

## Managed

`VmEMapInfoLineParser.cs`, `VmEMapInfoLineParserTests.cs`
Managed LOC (tally): 115.
Conversion commit: `d388fde494d98a5a82a7d7e0b1b6ce6061355316`.

## Tests

- `VmEMapInfoLineParserTests.SplitPropertyLine_ParsesEqualsForm`
- `VmEMapInfoLineParserTests.StripLineComment_HonorsQuotes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 273 --to 273` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
