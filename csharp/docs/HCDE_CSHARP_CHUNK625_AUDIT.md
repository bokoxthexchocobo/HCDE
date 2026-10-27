# Chunk 625 — Whole `GetPacket` (`i_net.cpp` 2119–2335)

## Scope

Whole `GetPacket` (`i_net.cpp` 2119–2335)

## Native ranges

- `src/common/engine/i_net.cpp` **GetPacket** 2119–2335 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`VmINetGetPacket.cs`
Managed LOC (tally): 219.
Conversion commit: `5cb16e161c3b2079c0b16db87d6ba41b364e0f96`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GetPacketNative` | `GetPacket` 2119–2335 |

### Out-of-body uncredit (625)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **217** |
| **Out-of-body removed** | **0** |
| **Honest** | **217** |


**Post-review ratio:** 219/217 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGetPacketTests.GetPacketNative_ReturnsEarlyOnServerQuery`
- `VmINetGetPacketTests.GetPacketNative_ClearsClientOnWouldBlock`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 625 --to 625` (0 hard / 0 soft).

## Seams

`recvfrom`, `uncompress`, `TryHandleServerQuery`, `TryProcessSetupConnectPacket`, and socket errors are injected via `GetPacketNativeContext`.

## Credit rationale

Single managed entry mirrors the full native function including oversize clamp and optional `from` out-parameter.
