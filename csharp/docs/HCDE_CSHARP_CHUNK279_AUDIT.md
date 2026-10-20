# Chunk 279 — ParseMapDefinition flag handler switch (re-audit 265-286: tested ranges only)

## Scope

ParseMapDefinition flag handler switch (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapDefinition flags** 2121–2270 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 12).

## Managed

`VmMapInfoFlagApply.cs`, `VmMapInfoFlagApplyTests.cs`
Managed LOC (tally): 56.
Conversion commit: `f22b1b5f1b994326bd6527c55647f2bd6f2a88d3`.

## Tests

- `VmMapInfoFlagApplyTests.ApplySetFlag_TogglesWithAssign`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 279 --to 279` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
