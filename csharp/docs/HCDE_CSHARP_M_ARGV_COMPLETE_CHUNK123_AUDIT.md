# Chunk 123 — complete `m_argv.cpp` `FArgs` surface

Extends chunk 118 with `RemoveArg`, `RemoveArgs`, `AppendRawArg`, `AppendRawArgsString`, and multi-name `CheckParm`.

Tests: `VmCommandLineArgsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
