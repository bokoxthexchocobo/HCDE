# Chunk 1344 — Member-call resolve tail, VM function-call construction, and the accessibility check from codegen.cpp (9535-9836).

## Scope

Member-call resolve tail, VM function-call construction, and the accessibility check from codegen.cpp (9535-9836).

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall** 9535–9836 (302 lines)

**Credited native LOC:** 302 (tally `nativeLoc` 302).

## Managed

`VmCodegen1344Slice.cs`
Managed LOC (tally): 736.
Conversion commit: `e24999d5faba9acc8fc76d428d3a2a5c6df9222f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Pointer dispatch (**9536–9560**) | yes |
| Struct address (**9561–9568**) | yes |
| Function pointer (**9570–9584**) | yes |
| Invalid left side (**9586–9590**) | yes |
| Find and readonly (**9596–9617**) | yes |
| Scope rewrite (**9622–9651**) | yes |
| Static gate (**9654–9673**) | yes |
| Method context (**9676–9701**) | yes |
| Ownership transfer (**9705–9709**) | yes |
| Constructor (**9719–9727**) | yes |
| Return prototype (**9745–9751**) | yes |
| Accessibility (**9754–9783**) | yes |
| Direct call (**9791–9806**) | yes |
| Resolve prefix (**9814–9836**) | yes |
| Band hull (**9535–9836**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| FindClassMemberFunction | callee; injected |
| CheckFunctionCompatiblity | callee; injected |
| FScopeBarrier | already chunk 1207; this slice only rewrites the flags it passes in |
| Lines before 9535 | earlier part of the member-call resolve |
| Lines after 9836 | the missing-self error and the rest of the VM resolve |

GetClass with arguments deletes this call. A non-container pointer is rejected. An already-readonly struct does not request an address. Call on a non-void function pointer uses the fake function and does not look up a class member. A lookup error deletes this call and adds no second message. Readonly without the readonly flag fails before the barrier. Virtual scope plus virtual on a play class calls a UI function from play; without that class the same flags stay data. A virtual-scope method on a UI class becomes a UI call. A plain struct member takes the barrier side. A UI member does not. Clear scope stays callable from data. A static method on a non-descendant or a struct cannot be called; a descendant is not implemented yet. Novirtual plus virtual skips that gate. A method with no caller function is a constant declaration. Self compatibility failure adds no message. A non-self call without the actor use flag names the symbol. A method moves the receiver; a static call deletes it. A function pointer forces the virtual call and reports a function-pointer self from the type. Accessibility uses ZScript text at 2.3.0 and DECORATE before that. Deprecated use warns, or stays quiet when the function version is still ahead. Direct calls skip accessibility when arguments, virtual, or a function pointer already disqualify them. The first explicit argument is the one past the implicit count, and action counts as three even when the method bit is also set. String arguments use the expression prototype; otherwise the variant prototype is returned. A null self resolve does not mark the caller script. A function pointer marks it before a failing accessibility check. The relaxed named-argument flag follows version 4.13.0. A passing check sets the continued flag and does not invent the following self-pointer error. The VM call does not delete its receiver.

| Managed (`VmCodegen1344Slice.cs`) | Native (`codegen.cpp`) |
|------------------------------------|------------------------|
| Dispatch, scope, VM call, accessibility, and the resolve prefix | **9535–9836** |
| Band hull (exclusive slice) | **9535–9836** |

### Chunk **1344** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **302** |
| **Net headline** | **+302** vs the prior chunk |


**Post-review ratio:** 736/302 managed/native ≈ 244% (src file LOC at HEAD; see tally).

## Tests

- `VmCodegen1344SliceTests.DispatchAndScope`
- `VmCodegen1344SliceTests.DirectCallsAndAccessibility`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1344 --to 1344` (0 hard / 0 soft).

## Seams

A class pointer named GetClass with no arguments returns the resolved class query and leaves this call alive. Any other container pointer falls through to the member lookup. A writable struct becomes a pointer and records whether the address request failed. Scope flags are rewritten before the existing barrier. delete this drops the receiver only when it was not moved into the VM call. The VM destructor body is empty, so that receiver stays. The resolve prefix stops after the accessibility check.

## Credit rationale

UNCredited codegen.cpp 9535-9836 (+302). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of codegen.cpp 2386-2684.
