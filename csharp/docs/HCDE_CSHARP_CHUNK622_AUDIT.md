# Chunk 622 — `I_QueryServerInfo` and network session lifecycle (`i_net.cpp` 1468–1638) Merged: [619, 623]. Merged: [620]. Restored launcher/pregame-ui maps from 619/623. Split: launcher/pregame UI bands moved to chunk649.

## Scope

`I_QueryServerInfo` and network session lifecycle (`i_net.cpp` 1468–1638) Merged: [619, 623]. Merged: [620]. Restored launcher/pregame-ui maps from 619/623. Split: launcher/pregame UI bands moved to chunk649.

## Native ranges

- `src/common/engine/i_net.cpp` **SetConnectFlow** 1468–1548 (81 lines)
- `src/common/engine/i_net.cpp` **SetConnectFlow** 1550–1572 (23 lines)
- `src/common/engine/i_net.cpp` **SetConnectFlow** 1574–1601 (28 lines)
- `src/common/engine/i_net.cpp` **SetConnectFlow** 1603–1607 (5 lines)
- `src/common/engine/i_net.cpp` **SetConnectFlow** 1611–1629 (19 lines)
- `src/common/engine/i_net.cpp` **SetConnectFlow** 1631–1638 (8 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmINetQueryClient.cs`, `VmINetSessionLifecycle.cs`, `VmINetSocketAddress.cs`
Managed LOC (tally): 275.
Conversion commit: `3ee50251becca09a5ad3d9b3066f0220e92cdc4e`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `QueryServerInfoNative` | `I_QueryServerInfo` 1468–1548 |
| `StartNetworkNative` | `StartNetwork` 1550–1572 |
| `CloseNetworkNative` | `CloseNetwork` 1574–1601 |
| `GenerateGameIdNative` | `GenerateGameID` 1603–1607 |
| `INetLogNative` | `I_NetLog` 1611–1629 |
| `SetConnectFlowNative` | `SetConnectFlow` 1631–1638 |

<!-- merged from chunk619 -->
| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GetLocalServerSnapshotNative` | `I_GetLocalServerSnapshot` 904–907 |
| `GetCachedServerQuerySnapshotNative` | `GetCachedServerQuerySnapshot` 955–970 |
| `EncodeLauncherInfoNative` | `SendLauncherInfo` encode 972–1042 |
| `SendLauncherInfoNative` | `SendLauncherInfo` send 1044–1050 |
| `WriteSessionTokenNative` | `WriteSessionToken` 1053–1066 |
| `ReadSessionTokenNative` | `ReadSessionToken` 1068–1071 |
| `TryHandleServerQueryNative` | `TryHandleServerQuery` 1073–1092 |

<!-- merged from chunk623 -->
| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `INetErrorNative` | `I_NetError` 1641–1649 |
| `INetInitNative` | `I_NetInit` 1651–1665 |
| `INetMessageNative` | `I_NetMessage` 1672–1686 |
| `INetLoopNative` | `I_NetLoop` 1697–1728 |
| `INetClientConnectedNative` | `I_NetClientConnected` 1732–1750 |
| `INetClientUpdatedNative` | `I_NetClientUpdated` 1753–1756 |
| `INetClientDisconnectedNative` | `I_NetClientDisconnected` 1758–1770 |
| `INetUpdatePlayersNative` | `I_NetUpdatePlayers` 1772–1780 |
| `IShouldStartNetGameNative` | `I_ShouldStartNetGame` 1782–1787 |
| `IGetKickClientsNative` | `I_GetKickClients` 1789–1796 |
| `IGetBanClientsNative` | `I_GetBanClients` 1798–1805 |
| `INetDoneNative` | `I_NetDone` 1807–1810 |

### Out-of-body uncredit (622)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **164** |
| **Out-of-body removed** | **0** |
| **Honest** | **164** |


**Post-review ratio:** 275/164 managed/native ≈ 168% (src file LOC at HEAD; see tally).

## Tests

- `VmINetQueryClientTests.QueryServerInfoNative_SucceedsWhenPipelineWorks`
- `VmINetQueryClientTests.CloseNetworkNative_ClearsSessionFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 622 --to 622` (0 hard / 0 soft).

## Seams

`WSAStartup`, `select`, `recvfrom`, `SV_ShutdownMasters`, and `DebugTrace` are injected.

## Credit rationale

Whole `I_QueryServerInfo` through `SetConnectFlow`.
