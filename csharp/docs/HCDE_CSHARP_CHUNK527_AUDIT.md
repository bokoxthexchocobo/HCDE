# Chunk 527 — FxTypeCast::Resolve

## Scope

FxTypeCast::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1775–1801 (27 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1804–2016 (213 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`ZccTypeCastResolve.cs`
Managed LOC (tally): 154.
Conversion commit: `b0926b7dd98b0d6b7a96558130e1423174aaa547`.

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

### Out-of-body uncredit (527)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **240** |
| **Out-of-body removed** | **0** |
| **Honest** | **240** |


**Post-review ratio:** 154/240 managed/native ≈ 64% (src file LOC at HEAD; see tally).

## Tests

- `ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual`
- `ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 527 --to 527` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `convertRawFunctionToFunctionPointer`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits ZScript type-cast resolve dispatch (null pointer, ID types, numeric/string casts) and raw-function pointer conversion from `codegen.cpp` (1775–2017).
