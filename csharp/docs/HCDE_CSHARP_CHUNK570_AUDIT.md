# Chunk 570 — HGetPacket tail, ClientConnecting, runtime slot/join helpers (5196–5347)

## Scope

HGetPacket tail, ClientConnecting, runtime slot/join helpers (5196–5347)

## Native ranges

- `src/d_net.cpp` **HGetPacket** 5196–5201 (6 lines)
- `src/d_net.cpp` **ClientConnecting** 5202–5307 (106 lines)
- `src/d_net.cpp` **Net_EnsureRuntimeClientSlot** 5309–5329 (21 lines)
- `src/d_net.cpp` **Net_ClearRuntimeClientJoinState** 5331–5347 (17 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDNetRuntimeClientConnect.cs`
Managed LOC (tally): 91.
Conversion commit: `455109c388e0dc415bd3f2057d3005c9249f9f43`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `HGetPacketRejectsLengthMismatch` | `HGetPacket` mismatch/return tail 5196–5201 |
| `ClientConnectingShouldReturnEarly` / `PrepareLateJoinRetransmit` / `ResendFromTargets` / `AppliedSequenceAfterSeed` / `ShouldSetPlayerEnter` | `ClientConnecting` 5202–5307 |
| `EnsureRuntimeSlotAddsClient` / `ShouldEnterOnSlotActivation` | `Net_EnsureRuntimeClientSlot` 5309–5329 |
| `ClearRuntimeJoinStateNative` | `Net_ClearRuntimeClientJoinState` 5331–5347 |

**Post-review ratio:** 91/150 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetRuntimeClientConnectTests.HGetPacketRejectsLengthMismatch_WhenSizesDiffer`
- `VmDNetRuntimeClientConnectTests.ClientConnectingShouldReturnEarly_WhenSetupInProgress`
- `VmDNetRuntimeClientConnectTests.AppliedSequenceAfterSeed_SetsBehindCurrent`
- `VmDNetRuntimeClientConnectTests.EnsureRuntimeSlotAddsClient_AppendsOnce`
- `VmDNetRuntimeClientConnectTests.ClearRuntimeJoinStateNative_ClearsWhenPending`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 570 --to 570` (0 hard / 0 soft).

## Seams

`SET_PLAYER_STATE`, `Net_SetLateJoinSyncPending`, `I_NetCmd`, and `memset` are injected at seams.

## Credit rationale

Ports late-join admission and join-state clear; `Net_ResetClientState` body remains on chunk 25 (no line overlap).
