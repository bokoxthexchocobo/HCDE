# Chunk 419 — lookspring/mouse capture CVARs and System_* input routing

## Scope

lookspring/mouse capture CVARs and System_* input routing

## Native ranges

- `src/d_main.cpp` **input CVARs and System_*** 3318–3476 (159 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmDMainSystemInput.cs`, `VmDMainSystemInputTests.cs`
Managed LOC (tally): 139.
Conversion commit: `05b07ac9ed38b06454352da5b988dcdebffa0042`.

## Tests

- `VmDMainSystemInputTests.MouseCaptureMode_ClampsHigh`
- `VmDMainSystemInputTests.DirectMouseLook_RequiresMenuOff`
- `VmDMainSystemInputTests.CaptureMode_InGameOnlyForModeZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 419 --to 419` (0 hard / 0 soft).

## Seams

Live console/menu state, `debugServer`, and `G_AddView*` are not hosted.

## Credit rationale

Credits mouse-capture CVAR clamping, GUI capture policy, direct-mouse look routing, and capture-mode gamestate gates from `d_main.cpp`.
