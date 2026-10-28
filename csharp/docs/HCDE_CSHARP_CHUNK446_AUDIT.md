# Chunk 446 — Debug server listen, D_DoomLoop, restart cleanup

## Scope

Debug server listen, D_DoomLoop, restart cleanup

## Native ranges

- `src/d_main.cpp` **debug listen and D_DoomLoop cycle** 4763–4792 (30 lines)

**Credited native LOC:** 30 (tally `nativeLoc` 30).

## Managed

`VmDDoomMainRunLoop.cs`, `VmDDoomMainRunLoopTests.cs`
Managed LOC (tally): 29.
Conversion commit: `1add3871bbfb9aa77ec6cae80053639fc2842c30`.

## Tests

- `VmDDoomMainRunLoopTests.DebugPortInRange_Valid`
- `VmDDoomMainRunLoopTests.DebugPortInRange_RejectsHigh`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 446 --to 446` (0 hard / 0 soft).

## Seams

`DebugServer::Listen`, `D_DoomLoop`, and `D_Cleanup` bodies are not hosted.

## Credit rationale

Credits anon stats/title hooks, debug port validation, doom loop entry, post-loop cleanup, and GS_STARTUP reset from `d_main.cpp`.
