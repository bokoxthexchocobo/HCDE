# ZScript FxFunctionCall::Resolve math builtins — chunk 69 audit

**Native:** `codegen.cpp` `FxFunctionCall::Resolve` lines 8700–8899 (Random/Min/Max/Clamp/Abs/ATan2/New arity rules).

**Managed:** `ZccFunctionCallMathBuiltins.cs`, tests.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
