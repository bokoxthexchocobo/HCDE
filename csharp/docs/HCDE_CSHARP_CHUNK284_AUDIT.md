# Chunk 284 — EMAPINFO level num helpers (re-audit 265-286: tested ranges only)

## Scope

EMAPINFO level num helpers (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **EMAPINFO level helpers** 375–524 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 9).

## Managed

`LocTallyIntegrityTests.cs`, `VmEMapInfoApplyHelpers.cs`, `VmEMapInfoApplyHelpersTests.cs`
Managed LOC (tally): 90.
Conversion commit: `fd9b8a11f0052856983c6f2d8b2c69990e44fe55`.

## Tests

- `VmEMapInfoApplyHelpersTests.ApplyBoolProperty_RecordsInvalidOnBadToken`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 284 --to 284` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
