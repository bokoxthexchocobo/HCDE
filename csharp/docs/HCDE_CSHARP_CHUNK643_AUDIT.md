# Chunk 643 — NMS1 UDP receive loop and wait-for-ack parsers (`sv_master.cpp` 284–464)

## Scope

NMS1 UDP receive loop and wait-for-ack parsers (`sv_master.cpp` 284–464)

## Native ranges

- `src/common/engine/sv_master.cpp` **ReceiveNms1Reply** 284–464 (181 lines)

**Credited native LOC:** 181 (tally `nativeLoc` 181).

## Managed

`VmSvMasterNms1Wait.cs`
Managed LOC (tally): 91.
Conversion commit: `31c4766166ac7776910b29286701789a77c5f16a`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `ShouldContinueAfterNotForRequestNative` | wait-loop `continue` paths 393–401 |
| `MapHeartbeatAckNative` / `IsStaleNms1TokenErrorNative` | 370–374, 456–458 |
| `ShouldAcceptUnregisterStaleNative` | 487–489 |

**Post-review ratio:** 91/181 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterNms1WaitTests.MapHeartbeatAckNative_ClassifiesStaleToken`
- `VmSvMasterNms1WaitTests.SplitSelectTimeoutNative_SplitsMilliseconds`
- `VmSvMasterNms1WaitTests.ShouldIgnoreUnexpectedReplyNative_WhenAddressDiffers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 643 --to 643` (0 hard / 0 soft).

## Seams

`select`/`recvfrom` and `hcde::master_nms1::Read*` live behind host I/O.

## Credit rationale

Whole `ReceiveNms1Reply` and `WaitForNms1*` switch bodies.
