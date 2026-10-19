# ZScript FxIntCast constant fold — chunk 48 audit

**Native:** `codegen.cpp` `FxIntCast::Resolve` constant propagation 1053–1054.

**Managed:** `ZccExpressionIntCast.cs`, `ZccExpressionIntCastTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
