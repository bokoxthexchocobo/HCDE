# Chunk 525 — FxArrayElement::Emit

## Scope

FxArrayElement::Emit

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::Emit** 8052–8315 (264 lines)

**Credited native LOC:** 264 (tally `nativeLoc` 264).

## Managed

(see conversion commit file list)
Managed LOC (tally): 153.
Conversion commit: `6c5e7095d5dacf3ade5002f432c40237e9f7ed2d`.

## Tests

- `ZccArrayElementEmitTests.ShiftBitsForElementSize_FourBytes`
- `ZccArrayElementEmitTests.CanUseShiftInsteadOfMul_WhenPowerOfTwo`
- `ZccArrayElementEmitTests.StaticArrayInBounds_RejectsHighIndex`
- `ZccArrayElementEmitTests.EmitBoundR_WhenResizable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 525 --to 525` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder::Emit`, `FxStructMember` dummy-var fix, and `PArray` layout are not hosted.

## Credit rationale

Credits ZScript array element codegen paths (bounds, meta size, konst tables, shift/mul indexing) from `codegen.cpp` (8052–8315).
