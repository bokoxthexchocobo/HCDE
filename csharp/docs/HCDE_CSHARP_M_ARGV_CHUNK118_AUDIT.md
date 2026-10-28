# Chunk 118 — `m_argv.cpp` `FArgs` lookup

Mirrors `CheckParm`, `CheckParmList`, `CheckValue`, `TakeValue`, and `NumArgs` for string argv (not `FArg` metadata).

Tests: `VmCommandLineArgsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
