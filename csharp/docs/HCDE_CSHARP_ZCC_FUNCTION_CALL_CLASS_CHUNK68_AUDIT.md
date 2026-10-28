# ZScript FxFunctionCall::Resolve class/casts — chunk 68 audit

**Native:** `codegen.cpp` `FxFunctionCall::Resolve` lines 8503–8699 (class-member gate, scope checks, builtin type casts).

**Managed:** `ZccFunctionCallClassResolve.cs`, `ZccScopeBarrier.cs`, `ZccFunctionCallBuiltinCasts.cs`, `ZccFunctionCallArgSize.cs`, tests.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
