# Chunk 151 — `sc_man` C-mode punctuation

`VmFScanner`: C-mode single-character `TOKC` tokens (`=`, `;`, brackets) and semicolon no longer treated as end-of-line comment in C mode.

**Not credited:** full re2c tables, `StateMode`, decorate expression lexer.

Tests: `VmFScannerTests.CMode_TreatsEqualsAndSemicolonAsTokens`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
