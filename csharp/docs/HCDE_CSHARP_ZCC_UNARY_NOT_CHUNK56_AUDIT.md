# ZScript unary bitwise not — chunk 56 audit

**Native:** `codegen.cpp` `FxUnaryNotBitwise::Resolve` 2241–2276, `FxUnaryNotBitwise::Emit` 2284–2300.

**Managed:** `ZccExpressionUnaryNot.cs`, `ZccExpressionUnaryNotTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
