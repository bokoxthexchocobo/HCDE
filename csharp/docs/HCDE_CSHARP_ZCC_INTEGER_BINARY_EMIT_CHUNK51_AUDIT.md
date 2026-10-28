# ZScript integer binary emit — chunk 51 audit

**Native:** `codegen.cpp` `FxAddSub::Emit` integer add/sub 3457–3463 (within 3405–3463); `FxMulDiv::Resolve` numeric 3606–3662; `FxMulDiv::Emit` integer multiply/divide/mod 3739–3777.

**Managed:** `ZccExpressionIntegerBinary.cs`, `ZccExpressionIntegerBinaryTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
