# Chunk 343 — Automap CCMD toggles and AMColor packing

## Scope

Automap CCMD toggles and AMColor packing

## Native ranges

- `src/am_map.cpp` **Automap CCMDs and AMColor** 184–349 (166 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmAutomapColorArgb.cs`, `VmAutomapColorArgbTests.cs`, `VmAutomapCommands.cs`, `VmAutomapCommandsTests.cs`
Managed LOC (tally): 48.
Conversion commit: `f7667961b082bd7f47df5dcde6476ae988a43683`.

## Tests

- `VmAutomapCommandsTests.ToggleBool_FlipsValue`
- `VmAutomapCommandsTests.ShouldRunToggleMap_OnlyWhenIdle`
- `VmAutomapColorArgbTests.FromRgb_PacksAlpha`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 343 --to 343` (0 hard / 0 soft).

## Seams

Console `CCMD` registration and `primaryLevel->automap` callbacks stay native; managed code models argument parsing and ARGB color validity only.

## Credit rationale

Continues the fresh `am_map.cpp` port from line 184: follow/grid/texture/mark CCMD behavior, `togglemap` idle guard, zoom-dir parsing, and `AMColor` RGB packing used by the automap palette tables.
