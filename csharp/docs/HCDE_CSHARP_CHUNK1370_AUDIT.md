# Chunk 1370 — FxTypeCast::Emit, FxPlusSign, FxMinusSign, and the FxUnaryNotBitwise constructor from codegen.cpp (2017-2240).

## Scope

FxTypeCast::Emit, FxPlusSign, FxMinusSign, and the FxUnaryNotBitwise constructor from codegen.cpp (2017-2240).

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **UnarySign** 2017–2240 (224 lines)

**Credited native LOC:** 224 (tally `nativeLoc` 224).

## Managed

`VmCodegenUnary1370Slice.cs`
Managed LOC (tally): 224.
Conversion commit: `f9ec292954acabf7a408c55486ec51356c179486`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Closing brace of the previous resolver (**2017**) | yes |
| `FxTypeCast::Emit` (**2019–2029**) | yes |
| `FxPlusSign` construct, destroy, resolve, and emit (**2031–2084**) | yes |
| `FxMinusSign` construct, destroy, resolve, and emit (**2086–2210**) | yes |
| `FxUnaryNotBitwise` construct and destroy (**2212–2233**) | yes |
| Banner and `Resolve` declaration (**2235–2240**) | yes |
| Band hull (**2017–2240**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| codegen.cpp **1804–2016** | `FxTypeCast::Resolve`, chunk 527 |
| codegen.cpp **2241–2276** and **2284–2300** | bitwise-not resolve and emit, chunk 56 |
| Constant pool and register allocator | helpers outside this band stay injected |

`FxTypeCast::Emit` hits `assert(false)`. With asserts enabled it does not return. With asserts compiled out it returns a default emit: register 0, type `REGT_NIL`, count 1, and the konst, fixed, final, and target flags clear.

Unary plus stores its operand. A null operand is rejected at construction. Resolve returns the same node when it is already resolved. Otherwise it resolves the operand, aborts when that result is null, and on a numeric, vector, or quaternion detaches that operand before deleting the plus node. Any other type reports `Numeric type expected` and deletes both. Emit forwards to the operand.

Unary minus uses the same resolve gate. A constant folds to a new constant, negating the int when the register type is `REGT_INT` and the float otherwise, and deletes the original operand. A non-constant bool becomes an `FxIntCast` with `nowarn` set, then that cast is resolved. A null cast result hits `assert`. Other numeric values copy the operand type onto the minus node and return it. Emit of a konst int returns `GetConstantInt` of the negated pooled value and does not emit an opcode. Any other konst, or a register-count mismatch, hits `assert` before allocation. A fixed source allocates a new emit and calls `Free`, which does not return a fixed register. Int emits `NEG`. Float emits `FLOP` with `FLOP_NEG`, or `NEGV2`, `NEGV3`, or `NEGV4`. Any other float count emits nothing.

The bitwise-not constructor stores the operand and the destructor deletes it. Its resolve body is not in this band.

| Managed (`VmCodegenUnary1370Slice.cs`) | Native (`codegen.cpp`) |
|----------------------------------------|------------------------|
| Type-cast emit, unary plus, unary minus, bitwise-not constructor | **2017–2240** |
| Band hull (exclusive slice) | **2017–2240** |

### Chunk **1370** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **224** |
| **Net headline** | **+224** vs the prior chunk |


**Post-review ratio:** 224/224 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmCodegenUnary1370SliceTests.PlusTypeCastAndBitwiseNot`
- `VmCodegenUnary1370SliceTests.MinusResolveAndEmit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1370 --to 1370` (0 hard / 0 soft).

## Seams

Child Resolve and Emit, constant-pool lookup, and register allocation stay injected. FxUnaryNotBitwise::Resolve at 2241 stays chunk 56. FxUnaryNotBitwise::Emit at 2284 stays chunk 56.

## Credit rationale

UNCredited codegen.cpp 2017-2240 (+224). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines after findfile.cpp 165-391. It is not a tie. decallib.cpp 1-222 is 222. Gaps longer than 400 lines stay whole and are not sliced. The bitwise-not resolve body is already chunk 56, so this band stops on its declaration.
