# Chunk 448 — GameMain display backend, CVAR init, D_DoomMain_Internal dispatch

## Scope

GameMain display backend, CVAR init, D_DoomMain_Internal dispatch

## Native ranges

- `src/d_main.cpp` **GameMain** 4809–4880 (72 lines)

**Credited native LOC:** 72 (tally `nativeLoc` 72).

## Managed

`VmDMainGameMain.cs`, `VmDMainGameMainTests.cs`
Managed LOC (tally): 50.
Conversion commit: `9165fd6e7dc93c71131e7d958b66558e8b3b6210`.

## Tests

- `VmDMainGameMainTests.CreateDisplayWhenInteractive`
- `VmDMainGameMainTests.SkipCloseWidgetWhenNorun`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 448 --to 448` (0 hard / 0 soft).

## Seams

`DisplayBackend`, `D_DoomMain_Internal`, and shutdown I/O are not hosted.

## Credit rationale

Credits zwidget backend selection, ticrate init, console callback gate, try/catch exit reasons, and norun widget skip from `d_main.cpp`.
