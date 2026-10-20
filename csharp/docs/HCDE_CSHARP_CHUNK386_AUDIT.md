# Chunk 386 — D_Display startup gates, wipe selection, level HUD branch, present path

## Scope

D_Display startup gates, wipe selection, level HUD branch, present path

## Native ranges

- `src/d_main.cpp` **D_Display** 1192–1291 (100 lines)
- `src/d_main.cpp` **D_Display** 1351–1428 (78 lines)
- `src/d_main.cpp` **D_Display** 1583–1607 (25 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmDMainDisplay.cs`, `VmDMainDisplayTests.cs`
Managed LOC (tally): 119.
Conversion commit: `ebc067e6474911e230a575de7492bd4ba1ff7078`.

## Tests

- `VmDMainDisplayTests.ShouldSkipDisplay_WhenDedicated`
- `VmDMainDisplayTests.ShouldEnterLevelRenderBranch_RequiresGametic`
- `VmDMainDisplayTests.ResolveWipeType_ForceFadeSelectsFade`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 386 --to 386` (0 hard / 0 soft).

## Seams

`RenderView`, `PerformWipe`, and `StatusBar` drawing are not hosted.

## Credit rationale

Credits `D_Display` from `d_main.cpp`.
