# Chunk 392 — D_Render, D_DoomLoop timing, page drawer/ticker

## Scope

D_Render, D_DoomLoop timing, page drawer/ticker

## Native ranges

- `src/d_main.cpp` **D_DoomLoop** 1668–1726 (59 lines)
- `src/d_main.cpp` **D_DoomLoop** 1727–1759 (33 lines)
- `src/d_main.cpp` **D_PageTicker** 1769–1773 (5 lines)
- `src/d_main.cpp` **D_PageDrawer** 1781–1801 (21 lines)
- `src/d_main.cpp` **CCMD togglehud** 614–617 (4 lines)
- `src/d_main.cpp` **D_AdvanceDemo** 1811–1814 (4 lines)
- `src/d_main.cpp` **CCMD endgame** 2070–2078 (9 lines)
- `src/d_main.cpp` **D_DoStrifeAdvanceDemo** 1822–1879 (58 lines)

**Credited native LOC:** 193 (tally `nativeLoc` 193).

## Managed

`VmDMainDoomLoop.cs`, `VmDMainDoomLoopTests.cs`, `VmDMainPageDrawer.cs`, `VmDMainStrifeDemo.cs`
Managed LOC (tally): 92.
Conversion commit: `7ae4e94ad3f335eea181cc18cd9cbbca475e8379`.

## Tests

- `VmDMainDoomLoopTests.ShouldIgnoreRestartDuringNetgame`
- `VmDMainPageDrawerTests.ShouldAdvanceDemoOnPageTick`
- `VmDMainDoomLoopTests.ShouldStartFrame_WhenGameticAdvances`
- `VmDMainDoomLoopTests.ShouldExitEndgame_OnlyWhenNotNetgame`
- `VmDMainStrifeDemoTests.ResolveTitlePageTicDuration`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 392 --to 392` (0 hard / 0 soft).

## Seams

`TryRunTics`, `D_Display`, and texture draw calls are not hosted.

## Credit rationale

Credits `D_DoomLoop` (including recoverable error paths), `D_PageTicker`, and `D_PageDrawer` from `d_main.cpp`.
