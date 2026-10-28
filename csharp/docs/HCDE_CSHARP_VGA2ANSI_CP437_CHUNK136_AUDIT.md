# Chunk 136 — `vga2ansi.cpp` CP437 + terminal probe

`VmVgaCp437` ports `cp437_to_ascii` and non-Windows `TERM`/`COLORTERM` capability classification.

**Not credited:** `vga_to_ansi` screen loop, `ibm437_to_utf8`, Windows `WriteFile` output.

Tests: `VmVgaCp437Tests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
