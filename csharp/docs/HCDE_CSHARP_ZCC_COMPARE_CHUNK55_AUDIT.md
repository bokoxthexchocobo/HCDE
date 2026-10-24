# ZScript compare expressions — chunk 55 audit

**Native:** `codegen.cpp` `FxCompareRel::Resolve` 3934–4046, `FxCompareRel::EmitCommon` integer branch 4096–4143.

**Managed:** `ZccExpressionCompare.cs`, `ZccExpressionCompareTests`.

String and float compare paths remain unconverted.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
