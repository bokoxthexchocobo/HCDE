# Chunk 631 — Start-game ack wait, abort send, and `HostGame` through net message (`i_net.cpp` 3366–3543)

## Scope

Start-game ack wait, abort send, and `HostGame` through net message (`i_net.cpp` 3366–3543)

## Native ranges

- `src/common/engine/i_net.cpp` **SendAbort** 3366–3543 (178 lines)

**Credited native LOC:** 178 (tally `nativeLoc` 178).

## Managed

`VmINetHostStartAck.cs`
Managed LOC (tally): 240.
Conversion commit: `0b46afa74c65b27c3f2d532e83934b0c25c856af`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HostCheckStartGameAcksNative` | `Host_CheckStartGameAcks` 3366–3431 |
| `SendAbortNative` | `SendAbort` 3433–3460 |
| `HostGameThroughNetMessageNative` | `HostGame` 3462–3543 |

**Post-review ratio:** 240/178 managed/native ≈ 135% (src file LOC at HEAD; see tally).

## Tests

- `VmINetHostStartAckTests.SendAbortNative_AuthoritySendsTwoByteExit`
- `VmINetHostStartAckTests.HostCheckStartGameAcksNative_AllAcked`
- `VmINetHostStartAckTests.HostGameThroughNetMessageNative_SinglePlayerShortcut`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 631 --to 631` (0 hard / 0 soft).

## Seams

`GetConnection`, `RemoveClientConnection`, and per-client `SendPacket` are injected.

## Credit rationale

Whole `Host_CheckStartGameAcks`, `SendAbort`, and `HostGame` through `I_NetMessage`.
