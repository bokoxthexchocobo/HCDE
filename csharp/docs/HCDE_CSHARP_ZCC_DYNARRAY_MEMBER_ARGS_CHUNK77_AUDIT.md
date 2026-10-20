# ZScript FxMemberFunctionCall dynarray args — chunk 77 audit

**Native:** `codegen.cpp` object-element `Push`/`Insert` argument compatibility (9220–9233).

**Managed:** `ZccDynArrayMemberArgumentValidation.cs`, tests.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
