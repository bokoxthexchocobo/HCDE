# Chunk 269 — PatchFrame property dispatch and MBF21 bits. (re-audit 265-286: tested ranges only)

## Scope

PatchFrame property dispatch and MBF21 bits. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchFrame** 1965–2118 (154 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 12).

## Managed

`VmDehackedPatchFrame.cs`, `VmDehackedPatchFrameTests.cs`
Managed LOC (tally): 118.
Conversion commit: `1a62e8d6ecbd4d08ce3a8b3f8e9387103cfe3e69`.

## Tests

- `VmDehackedPatchFrameTests.TryApplyProperty_SetsDuration`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 269 --to 269` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
