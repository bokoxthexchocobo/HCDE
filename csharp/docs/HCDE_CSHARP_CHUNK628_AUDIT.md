# Chunk 628 — Reject, verification error, and add client (`i_net.cpp` 2525–2676)

## Scope

Reject, verification error, and add client (`i_net.cpp` 2525–2676)

## Native ranges

- `src/common/engine/i_net.cpp` **AddClientConnection** 2525–2676 (152 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmINetConnectReject.cs`
Managed LOC (tally): 125.
Conversion commit: `846e71dab75b7c735d6f33f94a6e04c3589d3cfe`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `RejectConnectionNative` | `RejectConnection` 2525–2532 |
| `SendVerificationErrorNative` | `SendVerificationError` 2534–2592 |
| `AddClientConnectionNative` | `AddClientConnection` 2594–2676 |

**Post-review ratio:** 125/152 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmINetConnectRejectTests.RejectConnectionNative_SetsSetupPacket`
- `VmINetConnectRejectTests.SendVerificationErrorNative_EnginePayload`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 628 --to 628` (0 hard / 0 soft).

## Seams

`SendPacket`, `Net_ResetClientState`, `SET_PLAYER_STATE`, and `I_NetLog` are injected.

## Credit rationale

Whole `RejectConnection` through `AddClientConnection`.
