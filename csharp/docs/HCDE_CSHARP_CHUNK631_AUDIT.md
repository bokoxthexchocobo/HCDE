# Chunk 631 — Start-game ack wait, abort send, and `HostGame` through net message (`i_net.cpp` 3366–3543)

## Scope

Start-game ack wait, abort send, and `HostGame` through net message (`i_net.cpp` 3366–3543)

## Native ranges

- `src/common/engine/i_net.cpp` **SendAbort** 3366–3431 (66 lines)
- `src/common/engine/i_net.cpp` **SendAbort** 3433–3460 (28 lines)
- `src/common/engine/i_net.cpp` **SendAbort** 3462–3543 (82 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmINetHostStartAck.cs`
Managed LOC (tally): 166.
Conversion commit: `855f194ae78c20ae806fcae6dc0f1fd3c149db2a`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HostCheckStartGameAcksNative` | `Host_CheckStartGameAcks` 3366–3431 |
| `SendAbortNative` | `SendAbort` 3433–3460 |
| `HostGameThroughNetMessageNative` | `HostGame` 3462–3543 |

### Out-of-body uncredit (631)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **0** |
| **Honest** | **176** |


**Post-review ratio:** 166/176 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmINetHostStartAckTests.SendAbortNative_AuthoritySendsTwoByteExit`
- `VmINetHostStartAckTests.HostCheckStartGameAcksNative_AllAcked`
- `VmINetHostStartAckTests.HostGameThroughNetMessageNative_SinglePlayerShortcut`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 631 --to 631` (0 hard / 0 soft).

## Seams

`GetConnection`, `RemoveClientConnection`, and per-client `SendPacket` are injected. Context DTOs are uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Whole `Host_CheckStartGameAcks`, `SendAbort`, and `HostGame` through `I_NetMessage`.
