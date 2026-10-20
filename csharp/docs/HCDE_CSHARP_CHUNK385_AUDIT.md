# Chunk 385 — AM_initColors, Ticker, Serialize, show-all-lines, GoBig, Drawer timing

## Scope

AM_initColors, Ticker, Serialize, show-all-lines, GoBig, Drawer timing

## Native ranges

- `src/am_map.cpp` **AM_initColors** 705–759 (55 lines)
- `src/am_map.cpp` **DAutomap::Ticker** 1603–1609 (7 lines)
- `src/am_map.cpp` **DAutomap::Serialize** 3442–3465 (24 lines)
- `src/am_map.cpp` **DAutomap::UpdateShowAllLines** 3474–3505 (32 lines)
- `src/am_map.cpp` **DAutomap::GoBig** 3507–3522 (16 lines)
- `src/am_map.cpp` **DAutomap::Drawer** 3348–3363 (16 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAutomapInitColors.cs`, `VmAutomapLifecycle.cs`, `VmAutomapLifecycleTests.cs`
Managed LOC (tally): 135.
Conversion commit: `542ddec31fb82d38476d079c6f40d84ee39189d0`.

## Tests

- `VmAutomapInitColorsTests.ResolveAutoColorset_PicksDoomForDoomChex`
- `VmAutomapLifecycleTests.ToggleBigState_Flips`
- `VmAutomapLifecycleTests.ComputeShowAllEnabled_UsesPercentageThreshold`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 385 --to 385` (0 hard / 0 soft).

## Seams

`FSerializer` I/O and `saveScaleAndLoc` engine state are not hosted.

## Credit rationale

Credits `AM_initColors`, `DAutomap::Ticker`, `Serialize`, `UpdateShowAllLines`, `GoBig`, `ResetFollowLocation`, and Drawer frame delta from `am_map.cpp`.
