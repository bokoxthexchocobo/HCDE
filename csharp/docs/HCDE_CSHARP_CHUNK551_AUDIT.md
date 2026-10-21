# Chunk 551 — FxStackVariable ctor/emit and FxStructMember RequestAddress scope gates

## Scope

FxStackVariable ctor/emit and FxStructMember RequestAddress scope gates

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::FxStackVariable** 7517–7520 (4 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::~FxStackVariable** 7529–7533 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::RequestAddress** 7541–7546 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::Resolve** 7554–7559 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::Emit** 7561–7598 (38 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::RequestAddress** 7635–7666 (32 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::Resolve** 7674–7716 (43 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::Resolve** 7718–7743 (26 lines)
- `src/common/scripting/backend/codegen.cpp` **FxArrayElement::RequestAddress** 7914–7919 (6 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`ZccStackVariableEmit.cs`
Managed LOC (tally): 93.
Conversion commit: `5fc43544209cee523aaf8815d4d61ae4bf374bf6`.

## Padding review (2026-10-21)

Deepen pass (2026-10-21) added `StackEmitPlan` / `PlanStackVariableEmit` and duplicate wrappers (`ComputeStructMemberWritable`, `StructMemberMetaVariable`, color `NeedsShift`). Removed; see method map below.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `StackEmitOffsetReg`, `StackAddressRequestedUsesFramePointer`, `StackAddressRequestedUsesFrameAdd`, `StackEmitValueLoadUsesFramePointer`, `StackLoadOpForMember`, `StackUseBitLoad`, `StackBitMask`, `StackEmitBitPathAddsFrameOffset` | `FxStackVariable::Emit` 7561–7597 |
| `StructMemberBaseWritable` | `FxStructMember::RequestAddress` ctx/const-pointer gate 7646–7647 |
| `StackVariableWritable` | `FxStackVariable::RequestAddress` 7541–7545 |
| `StructMemberMetaReadOnly`, `StructMemberWritable` | `FxStructMember::RequestAddress` 7635–7664 |
| `VirtualScopeOuterFlags` | outerflags / virtual scope 7651–7656, 7687–7691 |
| `StructMemberInheritBarrierSide` | nested struct barrier 7702–7707 |
| `StructMemberResolveReadable` | scope readable gate 7694–7698 |
| `StaticReadonlyPromotesToGlobal` | static readonly promote 7710–7716 |
| `StructMemberPointerRequiresContainerType`, `StructMemberRequiresContainer` | pointer/container check 7718–7726 |
| `StructMemberColorChannelShift`, `ApplyColorChannelExtract` | color member resolve 7767–7777 |
| `ArrayElementRequestAddress` | `FxArrayElement::RequestAddress` 7914–7919 |

**Post-review ratio:** 93/166 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `ZccStackVariableEmitTests.StackOffsetRegister_ZeroMeansDefault`
- `ZccStackVariableEmitTests.StackVariableWritable_RequiresCtx`
- `ZccStackVariableEmitTests.StackUseBitLoad_WhenBitSet`
- `ZccStackVariableEmitTests.StackLoadOpForMember_RewritesLoToLp`
- `ZccStackVariableEmitTests.StructMemberWritable_RequiresScopeBarrier`
- `ZccStackVariableEmitTests.StructMemberResolve_RejectsUnreadable`
- `ZccStackVariableEmitTests.StructMemberResolve_StaticReadonlyPromotesGlobal`
- `ZccStackVariableEmitTests.ArrayElementRequestAddress_PassthroughWritable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 551 --to 551` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, and `FScopeBarrier` live instances are not hosted.

## Credit rationale

Credits native bands mirrored by `ZccStackVariableEmit` and tests — not full bytecode emission. Former chunk 554 retired: only `FxArrayElement::RequestAddress` merged here (whole function port).
