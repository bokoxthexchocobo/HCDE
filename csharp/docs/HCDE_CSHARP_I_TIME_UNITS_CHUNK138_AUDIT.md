# Chunk 138 — `i_time.cpp` ns/tic conversions

`VmTimeUnitConversions` ports `MSToNS`, `NSToMS`, `NSToTic`, and `TicToNS` static helpers (not frame clock state).

**Not credited:** `I_InitTime`, `I_SetFrameTime`, `I_WaitVBL`, `I_GetTime*`.

Tests: `VmTimeUnitConversionsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
