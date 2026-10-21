# Chunk 554 — FxStructMember::Emit, FxClassMember ctor, FxArrayElement ctor/resolve through static index fold

## Scope

FxStructMember::Emit, FxClassMember ctor, FxArrayElement ctor/resolve through static index fold

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxStructMember::Emit** 7796–7865 (70 lines)
- `src/common/scripting/backend/codegen.cpp` **FxClassMember::FxClassMember** 7874–7878 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::FxArrayElement** 7886–7894 (9 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::~FxArrayElement** 7902–7906 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::RequestAddress** 7914–7919 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::Resolve** 7927–7960 (34 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::Resolve** 7961–8010 (50 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::Resolve** 8011–8044 (34 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`ZccStackVariableEmit.cs`, `ZccStackVariableEmitTests.cs`, `ZccStructMemberArrayEmit.cs`, `ZccSuperGlobalCvarResolve.cs`, `ZccSuperGlobalCvarResolveTests.cs`
Managed LOC (tally): 125.
Conversion commit: `656a6cc4585201d331eca8d7634100df46ed2852`.

## Tests

- `ZccStructMemberArrayEmitTests.PlanStructMemberEmit_DeprecatedFlagsPath`
- `ZccStructMemberArrayEmitTests.StructMemberAddressReturnsObjectDirectly_WhenZeroOffset`
- `ZccStructMemberArrayEmitTests.ClassMemberSetsExprType_MatchesClassMember`
- `ZccStructMemberArrayEmitTests.ArrayElementRequestAddress_PassthroughWritable`
- `ZccStructMemberArrayEmitTests.ArrayIndexNeedsIntCast_WhenFloatReg`
- `ZccStructMemberArrayEmitTests.ArrayResolveAcceptsDynamicArray_PointerDynArray`
- `ZccStructMemberArrayEmitTests.ArrayResolveFailsNonArray_WhenNeither`
- `ZccStructMemberArrayEmitTests.ResizableStaticArrayMember_AllowsStackVariable`
- `ZccStructMemberArrayEmitTests.StaticArrayConstantIndexInBounds_RejectsEqualCount`
- `ZccStructMemberArrayEmitTests.FoldedStaticArrayFieldOffset_MultipliesElementSize`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 554 --to 554` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, `FunctionCallEmitter`, and live `PField` mutation are not hosted.

## Credit rationale

Credits struct-member emit and array-element resolve bands mirrored by `ZccStructMemberArrayEmit` and tests.
