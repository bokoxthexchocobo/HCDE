# Chunk 351 — D_Main CVAR clamps and HUD toggle capture

## Scope

D_Main CVAR clamps and HUD toggle capture

## Native ranges

- `src/d_main.cpp` **CUSTOM_CVAR policy D_ToggleHud** 422–620 (199 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmDMainCvarPolicy.cs`, `VmDMainCvarPolicyTests.cs`
Managed LOC (tally): 34.
Conversion commit: `18e6c38b056a4787ee8c046efdb2f28b24ce69c1`.

## Tests

- `VmDMainCvarPolicyTests.ClampTimeScaleForNetGame_ForcesOne`
- `VmDMainCvarPolicyTests.ClampVidRenderMode_RemapsSoftpoly`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 351 --to 351` (0 hard / 0 soft).

## Seams

Engine CVAR registration and renderer mode switching are not re-hosted; policy functions mirror net-game timescale, wipe bounds, and HUD snapshot behavior.

## Credit rationale

First behavioral slice of untouched `d_main.cpp` 422–620: `i_timescale` net-game guard, `vid_rendermode` clamp/softpoly remap, `wipetype` bounds, fraglimit exit hook policy, and `D_ToggleHud` saved-state capture.
