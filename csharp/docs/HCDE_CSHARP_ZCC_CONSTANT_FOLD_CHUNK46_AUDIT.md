# ZScript FxAddSub constant fold — chunk 46 audit

**Native:** `codegen.cpp` `FxAddSub::Resolve` integer constant fold 3361–3388.

**Managed:** `ZccExpressionConstantFolder.cs`, `ZccExpressionConstantFolderTests` (compile-and-run via `VmExec`).

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
