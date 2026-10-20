# Chunk 124 — `sc_man.cpp` `FScanner` (managed subset)

`VmFScanner` is a **string-backed, simplified** port: classic Hexen/C-mode token splitting (not the re2c `sc_man_scanner` state machine or full ZScript keyword tables). Conversion credit covers only:

- `OpenString` + `VmScriptText.Prepare` buffer rules
- `ScanString` dispatch / `GetString` / `MustGetString` / `MustGetStringName`
- `GetNumber` / `MustGetNumber` / `CheckNumber`, `GetFloat` / `MustGetFloat` / `CheckFloat` (via `VmScriptNumber`)
- `Compare`, `MatchString`, `SavePos` / `RestorePos`, symbol table for numeric evaluate, `ScriptError`

**Not credited:** `GetToken` keyword lexing, state mode, lump/file `Open*`, `CheckToken`/`TokenMustBe`, `SkipToEndOfBlock`, `ParseHex` (separate chunk).

Chunk 119 credits `CheckString` / `UnGet` only.

Tests: `VmFScannerTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
