# Chunk 629 — Remove/drop client and runtime setup driver plus incoming handlers (`i_net.cpp` 2678–3052)

## Scope

Remove/drop client and runtime setup driver plus incoming handlers (`i_net.cpp` 2678–3052)

## Native ranges

- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 2678–3052 (375 lines)

**Credited native LOC:** 375 (tally `nativeLoc` 375).

## Managed

`VmINetRuntimeSetup.cs`, `VmINetIncomingConnection.cs`
Managed LOC (tally): 488.
Conversion commit: `bef5e93c61b6f818705321e7a248488dd11092c5`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `RemoveClientConnectionNative` | `RemoveClientConnection` 2678–2705 |
| `DropClientForHcdeTimeoutNative` | `DropClientForHCDETimeout` 2707–2773 |
| `DriveRuntimeSetupStateForClientNative` | `DriveRuntimeSetupStateForClient` 2775–2918 |
| `HandleIncomingConnectionNative` | `HandleIncomingConnection` 2920–3024 |
| `HandleIncomingConnectionMaintenanceNative` | `HandleIncomingConnectionMaintenance` 3026–3052 |

**Post-review ratio:** 488/375 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmINetRuntimeSetupTests.DropClientForHcdeTimeoutNative_SkipsReadyClient`
- `VmINetIncomingConnectionTests.CallsGuestHandlerWhenNotAuthority`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 629 --to 629` (0 hard / 0 soft).

## Seams

`SendPacket`, reliable services, `Net_ReadUserInfo`, and `CountConnectedPlayers` are injected.

## Credit rationale

Whole `RemoveClientConnection` through `HandleIncomingConnectionMaintenance`.
