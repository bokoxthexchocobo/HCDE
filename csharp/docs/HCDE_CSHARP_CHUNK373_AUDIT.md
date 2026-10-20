# Chunk 373 — D_ToggleHud, D_Render, dmflags/dmflags2, compatmode presets

## Scope

D_ToggleHud, D_Render, dmflags/dmflags2, compatmode presets

## Native ranges

- `src/d_main.cpp` **D_ToggleHud** 594–612 (19 lines)
- `src/d_main.cpp` **D_Render** 629–649 (21 lines)
- `src/d_main.cpp` **CUSTOM_CVAR dmflags** 657–684 (28 lines)
- `src/d_main.cpp` **CUSTOM_CVAR dmflags2** 733–767 (35 lines)
- `src/d_main.cpp` **CUSTOM_CVAR compatmode** 841–893 (53 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmDMainCompatMode.cs`, `VmDMainRenderDmflags.cs`, `VmDMainRenderDmflagsTests.cs`
Managed LOC (tally): 164.
Conversion commit: `d165e7f4b3d93fab08fad3309644d80594756147`.

## Tests

- `VmDMainRenderDmflagsTests.ApplyHudHidden_ForcesFullscreenHud`
- `VmDMainRenderDmflagsTests.LevelHasDynamicLights_RequiresPolicyAndSources`
- `VmDMainRenderDmflagsTests.ShouldBroadcastFov_OnlyAuthorityWithFlag`
- `VmDMainRenderDmflagsTests.ResolveBroadcastFov_DefaultsToNinety`
- `VmDMainRenderDmflagsTests.ShouldStopAutomap_WhenFlagAndActive`
- `VmDMainRenderDmflagsTests.CompatMode_BoomPreset_IncludesTrace`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 373 --to 373` (0 hard / 0 soft).

## Seams

Network writes, `AM_Stop`, and renderer interpolation are not hosted; managed code mirrors gating predicates only.

## Credit rationale

Credits `D_ToggleHud`, `D_Render`, `CUSTOM_CVAR(dmflags)`, `CUSTOM_CVAR(dmflags2)`, and `CUSTOM_CVAR(compatmode)` presets from `d_main.cpp`.
