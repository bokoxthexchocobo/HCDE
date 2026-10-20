# Chunk 443 — D_DoomMain_Internal buttons, syscallbacks, basewad, help/version exit

## Scope

D_DoomMain_Internal buttons, syscallbacks, basewad, help/version exit

## Native ranges

- `src/d_main.cpp` **D_DoomMain_Internal entry** 4507–4563 (57 lines)
- `src/d_main.cpp` **forceswnodes through D_DoomInit** 4565–4576 (12 lines)
- `src/d_main.cpp` **basewad fatal and version banner** 4578–4612 (35 lines)
- `src/d_main.cpp` **logfile batch and iwad manager** 4614–4700 (87 lines)

**Credited native LOC:** 191 (tally `nativeLoc` 191).

## Managed

`VmDDoomMainInternalBoot.cs`, `VmDDoomMainInternalBootTests.cs`
Managed LOC (tally): 111.
Conversion commit: `f2f941d9673cb0f3005e50599f27bc5c90e86bb0`.

## Tests

- `VmDDoomMainInternalBootTests.NetIdStart_Offset`
- `VmDDoomMainInternalBootTests.ForceSwNodesFromArg`
- `VmDDoomMainInternalBootTests.FatalWhenBasewadMissing`
- `VmDDoomMainInternalBootTests.EarlyExitVersionHelp`
- `VmDDoomMainInternalBootTests.BatchRunFromErrorLog`
- `VmDDoomMainInternalBootTests.ShouldDebug_FromArg`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 443 --to 443` (0 hard / 0 soft).

## Seams

`D_DoomInit`, `FIWadManager`, and `PClass::StaticInit` bodies are not hosted.

## Credit rationale

Credits NetIDStart, button release locks, sys callback table wiring, forceswnodes, batch logfile policy, version/help early exit, and debug-port gating from `d_main.cpp`.
