# Chunk 383 — compatmode presets 8–9 and CalcFps frame accounting

## Scope

compatmode presets 8–9 and CalcFps frame accounting

## Native ranges

- `src/d_main.cpp` **CUSTOM_CVAR(compatmode)** 896–912 (17 lines)
- `src/d_main.cpp` **CalcFps** 1041–1060 (20 lines)

**Credited native LOC:** 37 (tally `nativeLoc` 37).

## Managed

`VmDMainCalcFps.cs`, `VmDMainCalcFpsTests.cs`, `VmDMainCompatMode.cs`, `VmDMainRenderDmflagsTests.cs`
Managed LOC (tally): 53.
Conversion commit: `dd30871754f13cb23215eb30d2fa62949624261a`.

## Tests

- `VmDMainRenderDmflagsTests.CompatMode_Mbf21Preset_IncludesReservedLineFlag`
- `VmDMainRenderDmflagsTests.CompatMode_StricterMbf21_AddsMikoPortals`
- `VmDMainCalcFpsTests.Tick_AdvancesFrameCountOncePerFrame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 383 --to 383` (0 hard / 0 soft).

## Seams

CVAR callback side effects and `screen->FrameTime` are not hosted.

## Credit rationale

Credits `CUSTOM_CVAR(compatmode)` cases 8–9 and `CalcFps` from `d_main.cpp`.
