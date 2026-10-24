# ZScript FxUnaryNotBoolean — chunk 61 audit

**Native:** `codegen.cpp` `FxUnaryNotBoolean::Resolve` 2346–2351; `Emit` 2364–2385.

**Managed:** `ZccExpressionUnaryBoolean.cs`, `ZccExpressionUnaryBooleanTests`.

`EmitCompare` branch patching and bool-cast insertion are not converted.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
