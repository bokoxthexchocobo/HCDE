# Chunk 274 — MAPINFO MapFlagHandlers catalog head. (re-audit 265-286: tested ranges only)

## Scope

MAPINFO MapFlagHandlers catalog head. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MapFlagHandlers table** 1946–2095 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 1).

## Managed

`VmMapInfoFlagHandlers.cs`, `VmMapInfoFlagHandlersTests.cs`
Managed LOC (tally): 86.
Conversion commit: `68329244e51a38f8c47177b65f5f464d29f272bc`.

## Tests

- `VmMapInfoFlagHandlersTests.TryGetHandler_FindsNoJump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 274 --to 274` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
