# Chunk 618 — HCDE connect block and `BuildServerQuerySnapshot` (`i_net.cpp` 711–903) Merged: [617]. Restored pregame map from merged chunk617. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

HCDE connect block and `BuildServerQuerySnapshot` (`i_net.cpp` 711–903) Merged: [617]. Restored pregame map from merged chunk617. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f30a6fe0f8473599b4cf573e94b59e52b084cd8d`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `ReadBe32Native` / `WriteBe32Native` | 448–458 |
| `BeginHcdePregameServiceNative` | 496–504 |
| `ClearAckedReliableServicesNative` | 543–553 |
| `CheckHcdePregameServiceNative` | 623–695 |
| `FindStringEndNative` | 698–708 |

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `ReadHcdeConnectInfoNative` | `ReadHCDEConnectInfo` 711–723 |
| `AppendHcdeConnectInfoNative` | `AppendHCDEConnectInfo` 725–734 |
| `BuildLocalHcdeConnectFlagsNative` | `BuildLocalHCDEConnectFlags` 736–752 |
| `ConnectFlowNameNative` | `ConnectFlowName` 754–763 |
| `ServerGameModeNameNative` | `ServerGameModeName` 765–779 |
| `ServerInvasionStateNameNative` | `ServerInvasionStateName` 781–795 |
| `CountConnectedClientsNative` | `CountConnectedClients` 802–812 |
| `BuildServerQuerySnapshotNative` | `BuildServerQuerySnapshot` 814–898 |

### Out-of-body uncredit (618)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **291** |
| **Out-of-body removed** | **291** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 618 --to 618` (0 hard / 0 soft).

## Seams

`sv_hostname`, `primaryLevel`, invasion getters, and player rows use host callbacks.

## Credit rationale

Whole helpers from `ReadHCDEConnectInfo` through `BuildServerQuerySnapshot`.
