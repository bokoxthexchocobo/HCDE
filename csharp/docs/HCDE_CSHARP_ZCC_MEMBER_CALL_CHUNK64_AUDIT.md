# ZScript member type names and call validation — chunk 64 audit

**Native:** `codegen.cpp` `FxMemberIdentifier::Resolve` basic types 7037–7071; `FxFunctionCall::Resolve` empty-arg check 8495–8502.

**Managed:** `ZccMemberTypeNames.cs`, `ZccFunctionCallValidation.cs`, tests.

Full member identifier resolution and VM function emission are not in this slice.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
