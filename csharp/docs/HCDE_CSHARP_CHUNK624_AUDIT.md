# Chunk 624 — Outbound send, reliable queue, guest runtime (`i_net.cpp` 1812–2117)

## Scope

Outbound send, reliable queue, guest runtime (`i_net.cpp` 1812–2117)

## Native ranges

- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1812–1815 (4 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1817–1833 (17 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1835–1897 (63 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1899–1937 (39 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1939–1974 (36 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 1976–1999 (24 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2001–2005 (5 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2007–2010 (4 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2012–2058 (47 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2060–2075 (16 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2077–2085 (9 lines)
- `src/common/engine/i_net.cpp` **HandleGuestRuntimeHCDEService** 2087–2117 (31 lines)

**Credited native LOC:** 295 (tally `nativeLoc` 295).

## Managed

`VmINetPacketSend.cs`, `VmINetReliableQueue.cs`
Managed LOC (tally): 318.
Conversion commit: `87262d0c89e0267ea836b33541fa5d46b63dd2d9`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `IClearClientNative` | `I_ClearClient` 1812–1815 |
| `FindClientNative` | `FindClient` 1817–1833 |
| `SendPacketNative` | `SendPacket` 1835–1897 |
| `FlushHcdeReliableServicesNative` | `FlushHCDEReliableServices` 1899–1937 |
| `BeginReliableHcdePregameServiceNative` | `BeginReliableHCDEPregameService` 1939–1974 |
| `CommitReliableHcdePregameServiceNative` | `CommitReliableHCDEPregameService` 1976–1999 |
| `ReadBe16Native` / `WriteBe16Native` | `ReadBE16` / `WriteBE16` 2001–2010 |
| `QueueHcdeRosterServiceNative` | `QueueHCDERosterService` 2012–2058 |
| `QueueHcdeBootstrapControlServiceNative` | `QueueHCDEBootstrapControlService` 2060–2075 |
| `AckHcdeControlServiceNative` | `AckHCDEControlService` 2077–2085 |
| `HandleGuestRuntimeHcdeServiceNative` | `HandleGuestRuntimeHCDEService` 2087–2117 |

### Out-of-body uncredit (624)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **296** |
| **Out-of-body removed** | **1** |
| **Honest** | **295** |


**Post-review ratio:** 318/295 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmINetPacketSendTests.FindClientNative_MatchesAddress`
- `VmINetReliableQueueTests.WriteBe16Native_RoundTrips`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 624 --to 624` (0 hard / 0 soft).

## Seams

`compress2`, `sendto`, `Net_SetUserInfo`, `DebugTrace`, and `CheckHCDEPregameService` are injected.

## Credit rationale

Whole `I_ClearClient` through `HandleGuestRuntimeHCDEService` (ends before `GetPacket`).
