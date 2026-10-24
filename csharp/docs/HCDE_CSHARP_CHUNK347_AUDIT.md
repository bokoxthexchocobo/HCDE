# Chunk 347 — MAPINFO SkipToNext CheckEndOfFile ParseLookupName

## Scope

MAPINFO SkipToNext CheckEndOfFile ParseLookupName

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **Parser helpers through ParseExitText** 525–775 (251 lines)

**Credited native LOC:** 251 (tally `nativeLoc` 251).

## Managed

`VmMapInfoParseLookupName.cs`, `VmMapInfoParseLookupNameTests.cs`
Managed LOC (tally): 42.
Conversion commit: `2b1afe4047fcd37b58c91efe6f965ede42ff76b0`.

## Tests

- `VmMapInfoParserFormatTests.EnsureNotUnexpectedEof_ThrowsForNewFormat`
- `VmMapInfoParseLookupNameTests.TryParseLookupToken_StripsDollarPrefix`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 347 --to 347` (0 hard / 0 soft).

## Seams

Scanner `sc` token stream is abstracted; HCDE mod-compat trailing-comma tolerance is not re-hosted in full.

## Credit rationale

Credits `g_mapinfo.cpp` 525–775: assign/float helpers, `SkipToNext`, unexpected-EOF checks, and the `ParseLookupName`/`ParseLumpOrTextureName`/`ParseExitText` entry points that populate per-map exit text tables.
