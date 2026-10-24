# Chunk 387 — GetCaps, DrawPaletteTester, DrawRateStuff, lag HUD overlays

## Scope

GetCaps, DrawPaletteTester, DrawRateStuff, lag HUD overlays

## Native ranges

- `src/d_main.cpp` **GetCaps** 974–1000 (27 lines)
- `src/d_main.cpp` **DrawPaletteTester** 1008–1028 (21 lines)
- `src/d_main.cpp` **DrawRateStuff** 1068–1111 (44 lines)
- `src/d_main.cpp` **DrawHCDELagHUD** 1113–1164 (52 lines)
- `src/d_main.cpp` **DrawOverlays** 1166–1174 (9 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmDMainDisplayTests.cs`, `VmDMainDrawRateOverlay.cs`, `VmDMainLagHudOverlay.cs`, `VmDMainPaletteTester.cs`, `VmDMainRendererCaps.cs`
Managed LOC (tally): 117.
Conversion commit: `d0739c6ba50ebdd8f859398be68c07e174430aba`.

## Tests

- `VmDMainRendererCapsTests.BuildHardwareCaps_AddsVoxelsWhenEnabled`
- `VmDMainPaletteTesterTests.ResolveCellCoordinates_MapsLinearIndex`
- `VmDMainDrawRateOverlayTests.ClampTicDelta_CapsAtTwenty`
- `VmDMainLagHudOverlayTests.ResolveLagHudColor_StallIsRed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 387 --to 387` (0 hard / 0 soft).

## Seams

`ClearRect`/`DrawText` and hardware renderer detection are not hosted.

## Credit rationale

Credits `GetCaps`, `DrawPaletteTester`, `DrawRateStuff`, `DrawHCDELagHUD`, and `DrawOverlays` from `d_main.cpp`.
