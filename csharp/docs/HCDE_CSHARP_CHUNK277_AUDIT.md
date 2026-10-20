# Chunk 277 — PatchMisc key table (re-audit 265-286: tested ranges only)

## Scope

PatchMisc key table (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchMisc keys** 2537–2686 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 1).

## Managed

`VmDehackedPatchMisc.cs`, `VmDehackedPatchMiscTests.cs`
Managed LOC (tally): 48.
Conversion commit: `5c16aed1819608e92dff06c3597b3790b0d2a239`.

## Tests

- `VmDehackedPatchMiscTests.TryApplyKnownKey_SetsStartHealth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 277 --to 277` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
