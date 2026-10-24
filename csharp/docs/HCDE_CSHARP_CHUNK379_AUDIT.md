# Chunk 379 — drawMarks, author markers, crosshair, and Drawer frame orchestration

## Scope

drawMarks, author markers, crosshair, and Drawer frame orchestration

## Native ranges

- `src/am_map.cpp` **DAutomap::drawMarks** 3215–3250 (36 lines)
- `src/am_map.cpp` **DAutomap::drawAuthorMarkers** 3258–3327 (70 lines)
- `src/am_map.cpp` **DAutomap::drawCrosshair** 3335–3338 (4 lines)
- `src/am_map.cpp` **DAutomap::Drawer** 3346–3434 (89 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmAutomapDrawThingsTests.cs`, `VmAutomapDrawerCompose.cs`, `VmAutomapDrawerComposeTests.cs`
Managed LOC (tally): 86.
Conversion commit: `2983ca25ce2bdfdcde18831e170e14bcb83a4f6a`.

## Tests

- `VmAutomapDrawerComposeTests.CrosshairPixel_CentersHorizontally`
- `VmAutomapDrawerComposeTests.ShouldDrawThingsLayer_MatchesCheatGate`
- `VmAutomapDrawerComposeTests.ComputeKeyboardPanDelta_AccumulatesDirections`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 379 --to 379` (0 hard / 0 soft).

## Seams

`twod` drawing, `doFollowPlayer`, and `changeWindowScale` engine hooks are not hosted.

## Credit rationale

Credits `DAutomap::drawMarks`, `drawAuthorMarkers`, `drawCrosshair`, and `Drawer` from `am_map.cpp`.
