# Chunk 527 — FxTypeCast::Resolve

## Scope

FxTypeCast::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1775–1801 (27 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1804–2016 (213 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`ZccTypeCastResolve.cs`
Managed LOC (tally): 356.
Conversion commit: `00529874e2d32a0640055966955590cadd16055f`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `convertRawFunctionToFunctionPointer` **1775–1801** and `FxTypeCast::Resolve` early branches **1804–2016** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `PlanRawFunctionPointer` | `convertRawFunctionToFunctionPointer` **1775–1801** |
| `InvalidCastTypes` | **1832–1844** |
| `NullPtrToPointer` | **1851–1853** |
| `NullPtrToIdTypes` / `VersionAtLeast` / `MakeVersion` | **1855–1859** |
| `ClassDescendantAllowsCast` | **1957–1962** |
| `NativeStructToWritablePointer` | **1965–1979** |
| `CompatiblePointerTypes` | `AreCompatiblePointerTypes` **1982–1984** (helper at **267–308**) |
| `CompatibleVectors` | **1994–1999** |
| `ClassifyResolveCast` | composite of **1831–2000** branches |
| `RunTypeCastResolvePipeline` / `SelectSecondaryCastDelegate` / `ShouldConvertRawToFunctionPointer` / `ShouldConvertRawToVmFunction` / `NativeStructReadonlyBlocksWritable` / `DescribeObjectPointer` | `Resolve` dispatch **1815–2016** (raw fn **1815–1829**, secondary Fx* casts **1861–1993**, native struct **1965–1979**) |

### Out-of-body uncredit (527)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **240** |
| **Out-of-body removed** | **0** |
| **Honest** | **240** |


**Post-review ratio:** 356/240 managed/native ≈ 148% (src file LOC at HEAD; see tally).

## Tests

- `ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual`
- `ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip`
- `ZccTypeCastResolveTests.RunTypeCastResolvePipeline_DelegatesFloatCast`
- `ZccTypeCastResolveTests.RunTypeCastResolvePipeline_RawFunctionRejectVirtual`
- `ZccTypeCastResolveTests.NativeStructReadonlyBlocksWritable_At412`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 527 --to 527` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `convertRawFunctionToFunctionPointer`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits ZScript type-cast resolve dispatch (null pointer, ID types, numeric/string casts) and raw-function pointer conversion from `codegen.cpp` (1775–2017).
