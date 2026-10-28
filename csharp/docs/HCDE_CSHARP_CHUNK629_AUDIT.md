# Chunk 629 — Remove/drop client and runtime setup driver plus incoming handlers (`i_net.cpp` 2678–3052)

## Scope

Remove/drop client and runtime setup driver plus incoming handlers (`i_net.cpp` 2678–3052)

## Native ranges

- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 2678–2705 (28 lines)
- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 2707–2773 (67 lines)
- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 2775–2918 (144 lines)
- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 2920–3024 (105 lines)
- `src/common/engine/i_net.cpp` **HandleIncomingConnectionMaintenance** 3026–3052 (27 lines)

**Credited native LOC:** 371 (tally `nativeLoc` 371).

## Managed

`VmINetRuntimeSetup.cs`, `VmINetIncomingConnection.cs`
Managed LOC (tally): 386.
Conversion commit: `34dc9dddece3de372214b5694e94f51aad0eb7a4`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `RemoveClientConnectionNative` | `RemoveClientConnection` 2678–2705 |
| `DropClientForHcdeTimeoutNative` | `DropClientForHCDETimeout` 2707–2773 |
| `DriveRuntimeSetupStateForClientNative` | `DriveRuntimeSetupStateForClient` 2775–2918 |
| `HandleIncomingConnectionNative` | `HandleIncomingConnection` 2920–3024 |
| `HandleIncomingConnectionMaintenanceNative` | `HandleIncomingConnectionMaintenance` 3026–3052 |

### Out-of-body uncredit (629)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **371** |
| **Out-of-body removed** | **0** |
| **Honest** | **371** |


**Post-review ratio:** 386/371 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmINetRuntimeSetupTests.DropClientForHcdeTimeoutNative_SkipsReadyClient`
- `VmINetIncomingConnectionTests.CallsGuestHandlerWhenNotAuthority`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 629 --to 629` (0 hard / 0 soft).

## Seams

`SendPacket`, reliable services, `Net_ReadUserInfo`, and `CountConnectedPlayers` are injected. Context DTOs are uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Whole `RemoveClientConnection` through `HandleIncomingConnectionMaintenance`.
