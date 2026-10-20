# Chunk 268 — PatchThing post-loop finalize only (aed28a93 commit). (re-audit 265-286: tested ranges only)

## Scope

PatchThing post-loop finalize only (aed28a93 commit). (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing finalize** 1831–1905 (75 lines)

**Credited native LOC:** 75 (tally `nativeLoc` 12).

## Managed

`VmDehackedPatchThingFinalize.cs`, `VmDehackedPatchThingFinalizeTests.cs`
Managed LOC (tally): 66.
Conversion commit: `aed28a9349028f764dea7c31a8850849f5eb7c08`.

## Tests

- `VmDehackedPatchThingFinalizeTests.NormalizeSpeed_ScalesLargeFixed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 268 --to 268` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
