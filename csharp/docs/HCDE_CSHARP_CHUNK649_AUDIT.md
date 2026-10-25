# Chunk 649 — Split from chunk622: server query respond + pregame UI (`i_net.cpp` 904–1092, 1641–1810). Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Scope

Split from chunk622: server query respond + pregame UI (`i_net.cpp` 904–1092, 1641–1810). Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `20cf17c0970f1cfb812c0200724b14df11745a8d`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GetLocalServerSnapshotNative` | `I_GetLocalServerSnapshot` 904–907 |
| `GetCachedServerQuerySnapshotNative` | `GetCachedServerQuerySnapshot` 955–970 |
| `EncodeLauncherInfoNative` | `SendLauncherInfo` encode 972–1042 |
| `SendLauncherInfoNative` | `SendLauncherInfo` send 1044–1050 |
| `WriteSessionTokenNative` | `WriteSessionToken` 1053–1066 |
| `ReadSessionTokenNative` | `ReadSessionToken` 1068–1071 |
| `TryHandleServerQueryNative` | `TryHandleServerQuery` 1073–1092 |
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

### Out-of-body uncredit (649)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **278** |
| **Out-of-body removed** | **132** |
| **Honest** | **146** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 649 --to 649` (0 hard / 0 soft).

## Seams

`DebugTrace`, socket I/O, and UI pumps are injected.

## Credit rationale

Whole-function bands for launcher respond and `I_Net*` pregame UI entry points.
