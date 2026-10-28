# Chunk 570 — HGetPacket tail, ClientConnecting, runtime slot/join helpers (5196–5347) (merged chunk 571) (merged chunk 571) (merged chunk 571) Merged: [569].

## Scope

HGetPacket tail, ClientConnecting, runtime slot/join helpers (5196–5347) (merged chunk 571) (merged chunk 571) (merged chunk 571) Merged: [569].

## Native ranges

- `src/d_net.cpp` **HGetPacket** 5196–5201 (6 lines)
- `src/d_net.cpp` **HGetPacket** 5202–5307 (106 lines)
- `src/d_net.cpp` **HGetPacket** 5309–5329 (21 lines)
- `src/d_net.cpp` **HGetPacket** 5331–5347 (17 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDNetRuntimeClientConnect.cs`, `VmDehackedMbfWeaponPointerEmitters.cs`, `ZccUnarySignCodegen.cs`
Managed LOC (tally): 275.
Conversion commit: `4b6ece1cebea48ad459da23e8730811c9aa7d35d`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `HGetPacketRejectsLengthMismatch` | `HGetPacket` mismatch/return tail 5196–5201 |
| `ClientConnectingShouldReturnEarly` / `PrepareLateJoinRetransmit` / `ResendFromTargets` / `AppliedSequenceAfterSeed` / `ShouldSetPlayerEnter` | `ClientConnecting` 5202–5307 |
| `EnsureRuntimeSlotAddsClient` / `ShouldEnterOnSlotActivation` | `Net_EnsureRuntimeClientSlot` 5309–5329 |
| `ClearRuntimeJoinStateNative` | `Net_ClearRuntimeClientJoinState` 5331–5347 |

<!-- merged from chunk569 -->
| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `TypeCastEmitIsUnreachable` | `FxTypeCast::Emit` assert-only body 2025–2030 (unreachable in hosted VM) |
| `PlusSignEmitForwardsOperand` | `FxPlusSign::Emit` returns operand unchanged 2081–2084 |
| `PlusSignResolveAllowed` | `FxPlusSign::Resolve` numeric/vector/quaternion guard 2061–2078 |
| `ResolveMinusSign` | `FxMinusSign::Resolve` 2115–2146 |
| `EmitMinusSign` | `FxMinusSign::Emit` 2154–2210 |

### Out-of-body uncredit (570)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


**Post-review ratio:** 275/150 managed/native ≈ 183% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetRuntimeClientConnectTests.HGetPacketRejectsLengthMismatch_WhenSizesDiffer`
- `VmDNetRuntimeClientConnectTests.ClientConnectingShouldReturnEarly_WhenSetupInProgress`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 570 --to 570` (0 hard / 0 soft).

## Seams

`SET_PLAYER_STATE`, `Net_SetLateJoinSyncPending`, `I_NetCmd`, and `memset` are injected at seams.

## Credit rationale

Ports late-join admission and join-state clear; `Net_ResetClientState` body remains on chunk 25 (no line overlap).
