# Chunk 561 — FxArrayElement ctor/dtor and Resolve (7886–7913, 7920–8044, 153 native); 7914–7919 in chunk 551

## Scope

FxArrayElement ctor/dtor and Resolve (7886–7913, 7920–8044, 153 native); 7914–7919 in chunk 551

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::FxArrayElement** 7886–7894 (9 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::~FxArrayElement** 7902–7913 (12 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::Resolve** 7920–8048 (129 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`ZccArrayElementResolve.cs`
Managed LOC (tally): 78.
Conversion commit: `ca48e9020a43bc90587c2a61ddbe68ed36cede5d`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|----------------------------------|
| `ZccArrayElementResolve` ctor/dtor gates | `FxArrayElement` 7886–7906 |
| `ZccArrayElementResolve` resolve helpers | `FxArrayElement::Resolve` 7927–8044 |

**Post-review ratio:** 78/150 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `ZccArrayElementResolveTests.CanFoldConstantField_WhenStaticMember`
- `ZccArrayElementResolveTests.InvalidArraySubscript_WhenNotPointerToArray`
- `ZccArrayElementResolveTests.ConstantIndexOutOfBounds_DetectsHighIndex`
- `ZccArrayElementResolveTests.StaticArraySizeAddr_AddsPointerSize`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 561 --to 561` (0 hard / 0 soft).

## Seams

`SAFE_RESOLVE`, `FxIntCast`, `PField` creation, and `RequestAddress` host are not fully hosted.

## Credit rationale

Credits non-overlapping `FxArrayElement` lifecycle + `Resolve` bands; `RequestAddress` remains chunk 551.
