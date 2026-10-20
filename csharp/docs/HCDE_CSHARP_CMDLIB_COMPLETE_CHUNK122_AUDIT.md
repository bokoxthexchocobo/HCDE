# Chunk 122 — complete `cmdlib.cpp` (remainder)

Ports `strbin`, `IsNum`, `CheckWildcards`, `StripExtension`, `IsAbsPath`, `NormalizeFileName`, `uppercopy`, and filesystem probes not covered by chunk 117 path helpers.

Tests: `VmCmdLibCompleteTests`, existing `VmCmdLibPathsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
