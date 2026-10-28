# ZScript FxMemberFunctionCall vector/sprite builtins — chunk 75 audit

**Native:** `codegen.cpp` sprite-id zero-arg gates and compare constants (9062–9114), vector/quaternion unary builtins (9117–9163).

**Managed:** `ZccSpriteMemberBuiltinValidation.cs`, `ZccMemberVectorBuiltinValidation.cs`, tests.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
