# Chunk 1379 — FxVMFunctionCall::Resolve argument placement, default fill, casts, and the empty-call check from codegen.cpp (9936-10139).

## Scope

FxVMFunctionCall::Resolve argument placement, default fill, casts, and the empty-call check from codegen.cpp (9936-10139).

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9936–10139 (204 lines)

**Credited native LOC:** 204 (tally `nativeLoc` 204).

## Managed

`VmCallResolveTail1379Slice.cs`
Managed LOC (tally): 290.
Conversion commit: `8a3dd3d1ed500871127d282a6c491f685413bfed`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Named-branch close (**9936–9938**) | yes |
| Slot placement and vararg push (**9939–9969**) | yes |
| Ordered-list move (**9971–9972**) | yes |
| Default fill and casts (**9974–10113**) | yes |
| Trailing insufficient check (**10115–10125**) | yes |
| Empty-call else (**10127–10139**) | yes |
| Band hull (**9936–10139**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Named-argument search **9882–9935** | below this band |
| `if (failed)` delete **10140–10144** | below this band |
| `Resolve`, `RequestAddress`, `NewPointer` | callees |

An empty argument list checks `ArgFlags` at the implicit index and rejects a required parameter. A non-empty list places each argument at the index already chosen above this band. A named node stores its value and clears that value. An index at or past `count` pushes the raw argument when the prototype is vararg, and otherwise reports too many arguments and deletes the call. Missing optional arguments become a one-register constant, or a vector of `float64` constants when the register count is not 1. A reference default forces `TypeNullPtr` after asserting the original type is a pointer, while the default cursor still advances by the original register count. Implicit parameters consume default words first. Varargs marked `VARF_VarArg` resolve in place, and `name` or `sound` values get a string cast and a second resolve. Other values are cast to the parameter type unless the parameter is `ref` or `out`. A real pointer to a dynamic array, map, or map iterator is dereferenced and `SAFE_RESOLVE` aborts the call when that resolve fails. A struct-pointer passed as the same struct-pointer skips `RequestAddress`. An `out` parameter that is not writable deletes the call. A `ref` parameter is rewrapped with `NewPointer`. A reference type mismatch records the error and continues with `failed` set. `hasStringArgs` is set only when the resolved value is `TypeString` and `failed` is still clear. The trailing check uses `Variants[0].ArgFlags` and deletes the call when another required parameter remains.

| Managed (`VmCallResolveTail1379Slice.cs`) | Native (`codegen.cpp`) |
|-------------------------------------------|------------------------|
| Placement, defaults, casts, and the empty call | **9936–10139** |
| Band hull (exclusive slice) | **9936–10139** |

### Chunk **1379** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **204** |
| **Net headline** | **+204** vs the prior chunk |


**Post-review ratio:** 290/204 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmCallResolveTail1379SliceTests.PlacementFillsDefaultsAndRejectsExtraArgs`
- `VmCallResolveTail1379SliceTests.RefsVarargsAndInsufficientCalls`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1379 --to 1379` (0 hard / 0 soft).

## Seams

The named-argument search at 9882-9935 stays outside this band. IndexAt is the running index after that search. Resolve, RequestAddress, and NewPointer are callees. VARF_Optional is 1<<0, VARF_Out is 1<<11, VARF_Ref is 1<<16, and VARF_VarArg is 1<<19. The failed-call delete at 10140 stays outside the band.

## Credit rationale

UNCredited codegen.cpp 9936-10139 (+204). After chunk 1378 this ties d_net.cpp 4992-5195. Path order takes this file. Gaps longer than 400 lines stay whole and are not sliced.
