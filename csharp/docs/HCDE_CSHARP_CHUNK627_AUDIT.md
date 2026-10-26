# Chunk 627 — `I_NetCmd` and `TryProcessSetupConnectPacket` (`i_net.cpp` 2337–2523)

## Scope

`I_NetCmd` and `TryProcessSetupConnectPacket` (`i_net.cpp` 2337–2523)

## Native ranges

- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2337–2348 (12 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2350–2364 (15 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2366–2372 (7 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2374–2378 (5 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2380–2395 (16 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2397–2406 (10 lines)
- `src/common/engine/i_net.cpp` **TryProcessSetupConnectPacket** 2408–2523 (116 lines)

**Credited native LOC:** 181 (tally `nativeLoc` 181).

## Managed

`VmINetNetCmdSetup.cs`
Managed LOC (tally): 213.
Conversion commit: `8218c2dcbb259fadd86b8299ca096c502fed6e94`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `INetCmdNative` | `I_NetCmd` 2337–2348 |
| `SetClientAckNative` | `SetClientAck` 2350–2364 |
| `ClientGotAckNative` | `ClientGotAck` 2366–2372 |
| `GetConnectionNative` | `GetConnection` 2374–2378 |
| `TryParseStrictIntNative` | `TryParseStrictInt` 2380–2395 |
| `CountConnectedPlayersNative` | `CountConnectedPlayers` 2397–2406 |
| `TryProcessSetupConnectPacketNative` | `TryProcessSetupConnectPacket` 2408–2523 |

### Out-of-body uncredit (627)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **181** |
| **Out-of-body removed** | **0** |
| **Honest** | **181** |


**Post-review ratio:** 213/181 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmINetNetCmdSetupTests.TryParseStrictIntNative_ParsesDecimal`
- `VmINetNetCmdSetupTests.INetCmdNative_DispatchesGet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 627 --to 627` (0 hard / 0 soft).

## Seams

`SendPacket`, `Net_VerifyEngine`, `AddClientConnection`, and ban list scans are injected.

## Credit rationale

Whole `I_NetCmd` through successful `TryProcessSetupConnectPacket` admission.
