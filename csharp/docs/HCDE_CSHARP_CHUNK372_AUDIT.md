# Chunk 372 — startDisplay, LevelInit, scale, Responder, changeWindowScale, doFollowPlayer policy

## Scope

startDisplay, LevelInit, scale, Responder, changeWindowScale, doFollowPlayer policy

## Native ranges

- `src/am_map.cpp` **DAutomap::startDisplay** 1337–1367 (31 lines)
- `src/am_map.cpp` **DAutomap::clearMarks** 1375–1381 (7 lines)
- `src/am_map.cpp` **DAutomap::LevelInit** 1389–1409 (21 lines)
- `src/am_map.cpp` **DAutomap::minOutWindowScale** 1417–1421 (5 lines)
- `src/am_map.cpp` **DAutomap::maxOutWindowScale** 1429–1433 (5 lines)
- `src/am_map.cpp` **DAutomap::NewUIScale** 1441–1456 (16 lines)
- `src/am_map.cpp` **DAutomap::NewResolution** 1464–1483 (20 lines)
- `src/am_map.cpp` **DAutomap::Responder** 1492–1514 (23 lines)
- `src/am_map.cpp` **DAutomap::changeWindowScale** 1523–1558 (36 lines)
- `src/am_map.cpp` **DAutomap::doFollowPlayer** 1566–1600 (35 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmAutomapDrawSubsectorsTests.cs`
Managed LOC (tally): 0.
Conversion commit: `0e616b0575b4fa50c387ae51e5d68767cb2c0240`.

## Tests

- `VmAutomapSessionTests.ResolveInitialPlayer_FallsBackToFirstInGame`
- `VmAutomapSessionTests.CenterOnCamera_OffsetsByHalfWindow`
- `VmAutomapSessionTests.ClearMarkArray_FillsEmptySentinel`
- `VmAutomapSessionTests.PickInitialScale_CapsAtMax`
- `VmAutomapSessionTests.ClampScale_ClampsToBounds`
- `VmAutomapSessionTests.ScaleLineThickness_MinimumOne`
- `VmAutomapSessionTests.RescaleAfterResolution_ZeroOldMin_ReturnsInput`
- `VmAutomapSessionTests.ShouldIgnorePanBindInFollow_MatchesPrefix`
- `VmAutomapSessionTests.ShouldReleaseMainBinding_RejectsHoldBinds`
- `VmAutomapSessionTests.ResolveZoomMultiplier_UsesZoomDirWhenSet`
- `VmAutomapSessionTests.FollowPlayerMapCenter_AppliesRotation`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 372 --to 372` (0 hard / 0 soft).

## Seams

Engine object graph (`players`, `StatusBar`, `twod`) is not hosted; managed code mirrors numeric/session rules only.

## Credit rationale

Credits `startDisplay`, `clearMarks`, `LevelInit`, `minOutWindowScale`, `maxOutWindowScale`, `NewUIScale`, `NewResolution`, `Responder`, `changeWindowScale`, and `doFollowPlayer` from `am_map.cpp`.
