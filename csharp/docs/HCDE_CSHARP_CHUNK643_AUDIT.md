# Chunk 643 — NMS1 packet I/O and wait-for-ack loops (`sv_master.cpp` 284–495) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

NMS1 packet I/O and wait-for-ack loops (`sv_master.cpp` 284–495) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4f53f4e9685df29d9e764b3d6dfb33b56e0e74ae`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `SendNms1PacketNative` | 284–299 |
| `ReceiveNms1ReplyNative` | 301–354 |
| `PrintNms1ErrorNative` | 356–368 |
| `IsStaleNms1TokenErrorNative` | 370–374 |
| `WaitForNms1ChallengeNative` | 376–404 |
| `WaitForNms1RegisterAckNative` | 406–434 |
| `WaitForNms1HeartbeatAckNative` | 436–464 |
| `WaitForNms1UnregisterAckNative` | 466–495 |

### Out-of-body uncredit (643)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **205** |
| **Out-of-body removed** | **205** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 643 --to 643` (0 hard / 0 soft).

## Seams

`select`/`recvfrom`/`sendto` and `hcde::master_nms1::Read*` are host-injected on wait contexts.

## Credit rationale

Whole `SendNms1Packet` through `WaitForNms1UnregisterAck`.
