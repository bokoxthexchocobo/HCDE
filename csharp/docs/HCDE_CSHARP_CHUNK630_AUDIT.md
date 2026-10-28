# Chunk 630 — Pregame host connection loop (`i_net.cpp` 3054–3364)

## Scope

Pregame host connection loop (`i_net.cpp` 3054–3364)

## Native ranges

- `src/common/engine/i_net.cpp` **Host_CheckForConnections** 3054–3364 (311 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmINetHostCheckConnections.cs`
Managed LOC (tally): 319.
Conversion commit: `0dea0485709e3784716e8ccc50749f31645931d6`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HostCheckForConnectionsNative` | `Host_CheckForConnections` 3054–3364 |
| `ApplyDedicatedAutostart` / `ProcessKickClients` / `ProcessBanClients` | dedicated + moderation **3059–1007** |
| `ProcessInboundSetupPackets` / `ProcessHcdeServicePacket` | packet loop **3109–2223** |
| `DriveClientSetupState` / `DriveWaitingClient` | client drive **3228–355** |

### Out-of-body uncredit (630)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **311** |
| **Out-of-body removed** | **0** |
| **Honest** | **311** |


**Post-review ratio:** 319/311 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmINetHostCheckConnectionsTests.HostCheckForConnectionsNative_ReturnsReadyWhenFull`
- `VmINetHostCheckConnectionsTests.HostCheckForConnectionsNative_RemovesKickedClient`
- `VmINetHostCheckConnectionsTests.ApplyDedicatedAutostart_EnablesWhenEnoughPlayers`
- `VmINetHostCheckConnectionsTests.ShouldIgnoreHcdeServicePacket_WhenRemoteInvalid`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 630 --to 630` (0 hard / 0 soft).

## Seams

`GetConnection`, `TryProcessSetupConnectPacket`, kick/ban UI lists, and master update are injected. `HostCheckForConnectionsContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Phased host pregame loop mirrors `Host_CheckForConnections` with injected I/O seams.
