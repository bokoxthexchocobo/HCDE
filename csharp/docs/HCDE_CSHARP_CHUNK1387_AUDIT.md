# Chunk 1387 — Random-pick emit, floating random, and the random2 construct from codegen.cpp (6316-6508).

## Scope

Random-pick emit, floating random, and the random2 construct from codegen.cpp (6316-6508).

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxRandomPick::Emit** 6316–6508 (193 lines)

**Credited native LOC:** 193 (tally `nativeLoc` 193).

## Managed

`VmCodegenRandom1387Slice.cs`
Managed LOC (tally): 228.
Conversion commit: `a7cf163cbc037963a8cd93cc5da2e0b7f648892f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Pick resolve return (**6316–6317**) | yes |
| Pick sugar comment (**6318–6337**) | yes |
| `FxRandomPick::Emit` (**6339–6419**) | yes |
| `FxFRandom` construct (**6426–6433**) | yes |
| `NativeFRandom` and the action (**6441–6454**) | yes |
| `FxFRandom::Emit` (**6456–6473**) | yes |
| `FxRandom2` construct (**6481–6488**) | yes |
| `FxRandom2` destructor (**6496–6499**) | yes |
| Random2 resolve shell (**6507–6508**) | yes |
| Band hull (**6316–6508**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Pick resolve loop **6310–6315** | chunk 538 |
| `EmitLoad` and `EmitLoadInt` | callees |
| `FxRandom` base constructor | callee; stores `EFX_Random` and ignores the passed kind |
| Float and int cast constructors | callees |
| Random2 resolve body **6509–6512** | outside this band |

An empty choice list aborts before the call when asserts are on. The integer random result is freed, and a float pick allocates a new float register and frees that too before the jump table. Each jump-table slot is backpatched to its case. A constant case loads through the result register. A different case register is moved and both registers are freed. The result register is marked in use again, then every choice is deleted and the array shrinks. `FxFRandom` passes `EFX_FRandom` to the base, which stores `EFX_Random`, then wraps both bounds in float casts and sets the value type to float. `NativeFRandom` is `min * (1 - t) + max * t` where `t` is the next `uint` times `2^-32`. The action reads a pointer and two floats and returns 0 when no return slot is requested. `FxFRandom::Emit` emits both bounds before the parameter opcodes. A null-typed bound records the non-value error and still pushes it. `FxRandom2` stores an int cast when a mask is present and a resolved constant -1 otherwise. The destructor deletes the mask and clears it.

| Managed (`VmCodegenRandom1387Slice.cs`) | Native (`codegen.cpp`) |
|------------------------------------------------|--------------------------------|
| Pick emit, floating random, and random2 | **6316–6508** |
| Band hull (exclusive slice) | **6316–6508** |

### Chunk **1387** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **193** |
| **Net headline** | **+193** vs the prior chunk |


**Post-review ratio:** 228/193 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmCodegenRandom1387SliceTests.PickEmitLoadsConstantsBackpatchesAndMovesADifferentRegister`
- `VmCodegenRandom1387SliceTests.FRandomScalesUnitsAndRandom2DeletesTheMask`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1387 --to 1387` (0 hard / 0 soft).

## Seams

The pick resolve loop at 6310-6315 and the random2 resolve body at 6509-6510 stay chunk 538. EmitLoad, float and int casts, the constant pool, and the register allocator stay callees. Random2::Resolve opens at the end of the band and its body stays outside it.

## Credit rationale

UNCredited codegen.cpp 6316-6508 (+193). After chunk 1386 this is the unique largest hole. p_saveg.cpp 84-275 is 192. The band is shorter than 400 lines and is not sliced.
