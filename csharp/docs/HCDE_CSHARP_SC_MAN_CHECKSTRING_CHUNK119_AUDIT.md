# Chunk 119 — `sc_man.cpp` `CheckString` / `UnGet`

`VmClassicScriptScanner` provides token lookahead with case-insensitive `CheckString` and pushback on mismatch (minimal tokenizer for tests).

Tests: `VmClassicScriptScannerTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
