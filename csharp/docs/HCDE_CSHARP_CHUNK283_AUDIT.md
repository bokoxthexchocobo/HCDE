# Chunk 283 — EMAPINFO parse primitives (re-audit 265-286: tested ranges only)

## Scope

EMAPINFO parse primitives (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **EMAPINFO parse primitives** 225–374 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 8).

## Managed

`VmEMapInfoParsePrimitives.cs`, `VmEMapInfoParsePrimitivesTests.cs`
Managed LOC (tally): 65.
Conversion commit: `a6477fafbe10b87266ad4be59d0116c728c826cf`.

## Tests

- `VmEMapInfoParsePrimitivesTests.GetDefaultLevelNum_ParsesMap01`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 283 --to 283` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
