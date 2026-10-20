# Chunk 286 — ResolveSiblingLump and ApplyEMapInfoProperty head (re-audit 265-286: tested ranges only)

## Scope

ResolveSiblingLump and ApplyEMapInfoProperty head (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ResolveSiblingLump and property dispatch** 525–674 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 6).

## Managed

`VmEMapInfoLumpResolve.cs`, `VmEMapInfoLumpResolveTests.cs`
Managed LOC (tally): 40.
Conversion commit: `6c5a53fb132ef931a4a8886a6a1a2bd3209f9fdc`.

## Tests

- `VmEMapInfoLumpResolveTests.TryBuildSiblingRelativePath_JoinsDirectory`
- `VmEMapInfoLumpResolveTests.ScaleFixedGravity_ZeroIsMax`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 286 --to 286` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
