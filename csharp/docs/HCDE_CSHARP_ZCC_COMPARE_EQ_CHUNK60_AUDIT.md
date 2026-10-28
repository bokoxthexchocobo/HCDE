# ZScript FxCompareEq integer fold/emit — chunk 60 audit

**Native:** `codegen.cpp` `FxCompareEq::Resolve` integer constants 4306–4308; `FxCompareEq::Emit` 4467–4469; `EmitCommon` integer path 4439–4462.

**Managed:** `ZccExpressionCompare` extensions; `ZccExpressionCompareTests`.

String/float/vector compare and `~==` remain outside this slice.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
