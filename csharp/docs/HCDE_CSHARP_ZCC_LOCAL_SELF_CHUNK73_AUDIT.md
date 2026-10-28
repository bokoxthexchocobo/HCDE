# ZScript FxLocalVariable / FxSelf — chunk 73 audit

**Native:** `FxLocalVariable::Resolve/Emit` 7187–7226, `FxStaticArrayVariable` 7235–7259, `FxSelf::Resolve` 7280–7309.

**Managed:** `ZccLocalVariableAccess.cs`, `ZccExpressionSelf.cs`, tests.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
