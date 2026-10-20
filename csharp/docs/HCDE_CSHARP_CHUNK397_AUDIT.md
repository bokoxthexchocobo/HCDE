# Chunk 397 — Automap CUSTOM_CVAR callbacks, color tables, colorset helpers

## Scope

Automap CUSTOM_CVAR callbacks, color tables, colorset helpers

## Native ranges

- `src/am_map.cpp` **CUSTOM_CVAR am_showalllines** 126–130 (5 lines)
- `src/am_map.cpp` **am visibility CVAR defaults** 153–166 (14 lines)
- `src/am_map.cpp` **CUSTOM_CVAR am_overlay** 144–152 (9 lines)
- `src/am_map.cpp` **am_emptyspacemargin NewResolution** 179–183 (5 lines)
- `src/am_map.cpp` **standard color CVAR defaults** 269–294 (26 lines)
- `src/am_map.cpp` **overlay color CVAR defaults** 296–318 (23 lines)
- `src/am_map.cpp` **AMColorset setWhite** 472–479 (8 lines)
- `src/am_map.cpp` **AMColorset operator[]** 481–489 (9 lines)
- `src/am_map.cpp` **cv_standard table** 522–548 (27 lines)
- `src/am_map.cpp` **cv_overlay table** 550–576 (27 lines)
- `src/am_map.cpp` **am_followplayer CVAR block** 191–198 (8 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmAutomapCustomCvarCallbacks.cs`, `VmDMainParseCVarInfoTests.cs`
Managed LOC (tally): 57.
Conversion commit: `96a75c8678eee077cf9c8152180562184596435e`.

## Tests

- `VmAutomapCustomCvarCallbacksTests.ShouldUpdateShowAllLines_WhenAutomapPresent`
- `VmAutomapCustomCvarCallbacksTests.DefaultShowSecrets_IsTrue`
- `VmAutomapCustomCvarCallbacksTests.ShouldDeactivateOverlay_WhenOverlayZero`
- `VmAutomapCustomCvarCallbacksTests.ShouldRefreshResolution_WhenStatusBarAndAutomap`
- `VmAutomapCustomCvarCallbacksTests.DefaultOverlaySectorFillAlpha_MatchesNative`
- `VmAutomapCustomCvarCallbacksTests.ShouldInitColorFromCvar_SkipsAlmostBackground`
- `VmAutomapCustomCvarCallbacksTests.StandardCvarTableLength_MatchesNative`
- `VmAutomapCustomCvarCallbacksTests.OverlayCvarTableLength_MatchesNative`
- `VmAutomapCustomCvarCallbacksTests.ClampEmptySpaceMargin_CapsAtNinety`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 397 --to 397` (0 hard / 0 soft).

## Seams

Engine CVAR registration and `primaryLevel->automap` method calls are not hosted.

## Credit rationale

Credits automap CUSTOM_CVAR callbacks, standard/overlay CVAR defaults, `cv_standard`/`cv_overlay`, and `AMColorset` white/lookup helpers from `am_map.cpp` (no overlap with chunks 340/352/354/379/385/393).
