# Chunk 1342 — JIT register moves, numeric and string casts, and dynamic class casts from jit_move.cpp (1-312).

## Scope

JIT register moves, numeric and string casts, and dynamic class casts from jit_move.cpp (1-312).

## Native ranges

- `src/common/scripting/jit/jit_move.cpp` **EmitMOVE** 1–312 (312 lines)

**Credited native LOC:** 312 (tally `nativeLoc` 312).

## Managed

`VmJitMove1342Slice.cs`
Managed LOC (tally): 450.
Conversion commit: `b04e83c386f0f6b237c3c6e94772c2542a72a004`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Integer, float, string, and pointer moves (**30–49**) | yes |
| Vector moves (**51–70**) | yes |
| Cast helpers (**72–88**) | yes |
| Cast dispatch (**90–218**) | yes |
| Bool cast (**221–256**) | yes |
| Dynamic object and class casts (**258–312**) | yes |
| Band hull (**1–312**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| stb_sprintf | callee; five-digit floats, Inf, and NaN |
| strtoll and strtod | callees; base 0 and hex floats |
| FName, V_GetColor, sound, sprite, and texture | callees invoked by the cast |
| IsKindOf and IsDescendantOf | callees; a null receiver skips them |

Unsigned float conversion zero-extends the 32-bit register. Signed truncating conversion uses the x64 indefinite result when the value does not fit in 32 bits. Unsigned truncating conversion uses a 64-bit truncate and keeps the low 32 bits. A null pointer formats as the word null, and any other pointer is 16 lowercase hex digits. An empty string name is NAME_None and does not look the name up. An out-of-range name index formats as empty. A missing texture formats as (null). The default sprite cast prints the 32-bit pattern with %d. Bool casts of NaN are true, and negative zero is false. Unknown cast types raise the unknown-type error.

| Managed (`VmJitMove1342Slice.cs`) | Native (`jit_move.cpp`) |
|-----------------------------------|-------------------------|
| Moves, casts, and dynamic casts | **1–312** |
| Band hull (exclusive slice) | **1–312** |

### Chunk **1342** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **312** |
| **Net headline** | **+312** vs the prior chunk |


**Post-review ratio:** 450/312 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmJitMove1342SliceTests.MovesAndNumericCasts`
- `VmJitMove1342SliceTests.NamesColorsAndDynamicCasts`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1342 --to 1342` (0 hard / 0 soft).

## Seams

Moves copy registers in ascending order, so an overlapping vector move reads a lane that an earlier lane already replaced. Cast formatting is stb_sprintf. Integer parse is strtoll base 0 and the result is narrowed to 32 bits. Float parse is strtod, including a hex float. FName, V_GetColor, sound, sprite, and texture lookups stay callees. A null object skips IsKindOf and IsDescendantOf.

## Credit rationale

UNCredited jit_move.cpp 1-312 (+312). A full tally line scan shows no overlap. This whole file is the largest remaining hole of at most 400 lines, ahead of a_dynlightdata.cpp 1-304.
