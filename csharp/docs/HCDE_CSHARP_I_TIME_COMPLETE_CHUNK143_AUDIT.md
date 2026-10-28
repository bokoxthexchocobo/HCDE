# Chunk 143 — `i_time.cpp` clock

`VmGameClock` ports frame/tic timing, freeze, input/delta time, and physics step (injectable `GetTimePoint`). Static `MSToNS`/`NSToTic` helpers remain chunk 138.

**Not credited:** `DEFINE_ACTION_FUNCTION` exports, OS `timeBeginPeriod` notes.

Tests: `VmGameClockTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
