# Chunk 628 — Reject, verification error, and add client (`i_net.cpp` 2525–2676)

## Scope

Reject, verification error, and add client (`i_net.cpp` 2525–2676)

## Native ranges

- `src/common/engine/i_net.cpp` **AddClientConnection** 2525–2532 (8 lines)
- `src/common/engine/i_net.cpp` **AddClientConnection** 2534–2592 (59 lines)
- `src/common/engine/i_net.cpp` **AddClientConnection** 2594–2676 (83 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmINetConnectReject.cs`
Managed LOC (tally): 203.
Conversion commit: `8db816e6fc4b332f8a2bae670c800d788c5f676d`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `RejectConnectionNative` | `RejectConnection` 2525–2532 |
| `SendVerificationErrorNative` | `SendVerificationError` 2534–2592 |
| `AddClientConnectionNative` | `AddClientConnection` 2594–2676 |

### Out-of-body uncredit (628)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


**Post-review ratio:** 203/150 managed/native ≈ 135% (src file LOC at HEAD; see tally).

## Tests

- `VmINetConnectRejectTests.RejectConnectionNative_SetsSetupPacket`
- `VmINetConnectRejectTests.SendVerificationErrorNative_EnginePayload`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 628 --to 628` (0 hard / 0 soft).

## Seams

`SendPacket`, `Net_ResetClientState`, `SET_PLAYER_STATE`, and `I_NetLog` are injected.

## Credit rationale

Whole `RejectConnection` through `AddClientConnection`.
