# Chunk 124 — complete `sc_man.cpp` `FScanner`

`VmFScanner` replaces the minimal `VmClassicScriptScanner`: `OpenString`, `GetString`/`MustGet*`, `GetToken`/`CheckToken`, `GetNumber`/`GetFloat` and check/must variants, `UnGet`, `Compare`/`MatchString`, save/restore, symbols, and script errors. Chunk 119 credited only `CheckString`/`UnGet`; this chunk credits the remaining scanner logic.

Tests: `VmFScannerTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
