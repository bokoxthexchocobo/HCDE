# Chunk 353 — AMColorset init and line-color table

## Scope

AMColorset init and line-color table

## Native ranges

- `src/am_map.cpp` **AMColorset initFromCVars almost background** 426–451 (26 lines)
- `src/am_map.cpp` **AMColorset initFromColors** 453–470 (18 lines)
- `src/am_map.cpp` **AUTOMAP_LINE_COLORS** 498–514 (17 lines)
- `src/am_map.cpp` **DoomColors predefined table head** 599–626 (28 lines)

**Credited native LOC:** 89 (tally `nativeLoc` 89).

## Managed

`VmAutomapColorsetLoader.cs`, `VmAutomapColorsetLoaderTests.cs`
Managed LOC (tally): 42.
Conversion commit: `e5a76186f6ff86258bda665e8a15b6ce5b58e95e`.

## Tests

- `VmAutomapColorsetLoaderTests.ComputeAlmostBackgroundRgb_WrapsNegativeChannels`
- `VmAutomapColorsetLoaderTests.DecodeTriplet_MarksUnusedAsInvalid`
- `VmAutomapColorsetLoaderTests.LineStyleToColorIndex_MapsOneSidedWall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 353 --to 353` (0 hard / 0 soft).

## Seams

CVAR pointer tables and `CCMD(am_restorecolors)` stay native; managed code mirrors `initFromColors` unused triplets, almost-background RGB math, and `AUTOMAP_LINE_COLORS` slot mapping.

## Credit rationale

Credits `am_map.cpp` 426–451 (`initFromCVars` almost-background), 453–470 (`initFromColors` unused marker), 498–514 (`AUTOMAP_LINE_COLORS`), and Doom palette seed 599–626 used by predefined colorsets.
