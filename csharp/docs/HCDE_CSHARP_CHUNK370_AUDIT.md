# Chunk 370 — drawSubsectors skip/textured/fade/triangulation policy

## Scope

drawSubsectors skip/textured/fade/triangulation policy

## Native ranges

- `src/am_map.cpp` **DAutomap::drawSubsectors** 2062–2307 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmAutomapDrawSubsectors.cs`, `VmAutomapDrawSubsectorsTests.cs`
Managed LOC (tally): 93.
Conversion commit: `5a9af690afbd067307c542225a9af830721eb75f`.

## Tests

- `VmAutomapDrawSubsectorsTests.ShouldSkipSubsector_HiddenWithoutCheat_Skips`
- `VmAutomapDrawSubsectorsTests.ShouldSkipSubsector_PortalOverlay_SkipsForeignGroup`
- `VmAutomapDrawSubsectorsTests.ApplyUndrawnCheatTint_WashesOutColors`
- `VmAutomapDrawSubsectorsTests.ResolveFadeLevel_SoftRamp_UsesDoomFormula`
- `VmAutomapDrawSubsectorsTests.NeedsHoleTriangulation_OnlyForHoleWithManyLines`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 370 --to 370` (0 hard / 0 soft).

## Seams

`twod->AddPoly`, `AM_FakeFlat`, and earcut triangulation are not hosted; managed code mirrors visibility gates and lighting/fade math.

## Credit rationale

Credits `DAutomap::drawSubsectors` from `am_map.cpp`.
