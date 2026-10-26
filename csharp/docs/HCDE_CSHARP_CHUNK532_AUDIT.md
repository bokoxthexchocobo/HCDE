# Chunk 532 — FxAssign::Resolve

## Scope

FxAssign::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxAssign::Resolve** 2685–2860 (176 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`ZccAssignResolve.cs`
Managed LOC (tally): 209.
Conversion commit: `d664ca16de77308ce03ff9c58b21efc9280c9896`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `FxAssign::Resolve` **2685–2860** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `BoolModifyAssignInvalid` | **2694–2700** |
| `NeedsNumericPromotion` | **2705–2722** |
| `NativeStructToMatchingPointer` | **2724–2729** |
| `SameTypeOrRealPointerAlias` / `SkipVectorQuaternionPath` | **2731–2742** |
| `DynArrayAssignPolicy` | **2757–2783** |
| `MapAssignPolicy` | **2785–2811** |
| `MapIteratorAssignPolicy` | **2813–2817** |
| `StructAssignPolicy` | **2819–2833** |
| `FallsThroughToTypeCast` | **2839–2843** |
| `RequiresWritableAddress` / `ModifiableValueFails` | **2846–2850** |
| `NormalizeBitWriteIndex` / `UsesDeprecatedFlagHandler` | **2854–2858** |
| `ClassifyContainerAssign` | composite **2736–2834** |
| `RunAssignResolvePipeline` / `AssignResolveInput` | full resolve orchestration **2685–2860** |

### Out-of-body uncredit (532)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **0** |
| **Honest** | **176** |


**Post-review ratio:** 209/176 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `ZccAssignResolveTests.BoolModifyAssignInvalid_BlocksBoolBase`
- `ZccAssignResolveTests.DynArrayAssignPolicy_RewritesAt4111`
- `ZccAssignResolveTests.MapAssignPolicy_WarnsOnPointerSwapBelow4111`
- `ZccAssignResolveTests.ClassifyContainerAssign_MapIteratorFails`
- `ZccAssignResolveTests.NormalizeBitWriteIndex_StripsInternalFlag`
- `ZccAssignResolveTests.RunAssignResolvePipeline_BlocksArrayAssign`
- `ZccAssignResolveTests.RunAssignResolvePipeline_NormalizesBitWrite`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 532 --to 532` (0 hard / 0 soft).

## Seams

`FxBoolCast`, `FxTypeCast`, `FxMemberFunctionCall`, `RequestAddress`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript assignment resolve guards (bool modify-assign, container copy vs pointer-swap policy, writable lvalue, bitfield write index) from `codegen.cpp` (2685–2861).
