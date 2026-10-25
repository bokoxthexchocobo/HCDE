# Chunk 615 — Net_Write*, FDynamicBuffer, RemoveClass, SelectWeapon, UseFlechette (8269–8510; event pump lines are seams) Merged: [610, 614]. Merged: [611]. Restored invasion map credit from merged chunk611.

## Scope

Net_Write*, FDynamicBuffer, RemoveClass, SelectWeapon, UseFlechette (8269–8510; event pump lines are seams) Merged: [610, 614]. Merged: [611]. Restored invasion map credit from merged chunk611.

## Native ranges

- `src/d_net.cpp` **Net_NewClientTic** 8289–8294 (6 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8296–8299 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8301–8304 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8306–8309 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8311–8314 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8316–8319 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8321–8324 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8326–8329 (4 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8331–8335 (5 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8343–8347 (5 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8349–8357 (9 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8359–8376 (18 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8378–8383 (6 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8390–8420 (31 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8426–8481 (56 lines)
- `src/d_net.cpp` **Net_NewClientTic** 8483–8510 (28 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmDNetNetEventWrite.cs`, `NetDynamicBuffer.cs`, `VmDNetTrySkipUserCmd.cs`, `VmDNetGetLatency.cs`, `VmDNetCcmdSettings.cs`, `VmDNetInvasionGetters.cs`, `VmDNetInvasionScriptExports.cs`, `VmDNetScreenJobReady.cs`, `VmDNetInvasionPreset.cs`, `VmDNetCcmdInvasionConsole.cs`, `VmDNetCcmdConsole.cs`
Managed LOC (tally): 818.
Conversion commit: `60b8b80c086d459f78cff29fe77b48d87570ae20`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `NetAdoptServerRoomIdNative` | `Net_AdoptServerRoomID` 8289–8294 |
| `NetWriteInt8Native` | `Net_WriteInt8` 8296–8299 |
| `NetWriteInt16Native` | `Net_WriteInt16` 8301–8304 |
| `NetWriteInt32Native` | `Net_WriteInt32` 8306–8309 |
| `NetWriteInt64Native` | `Net_WriteInt64` 8311–8314 |
| `NetWriteFloatNative` | `Net_WriteFloat` 8316–8319 |
| `NetWriteDoubleNative` | `Net_WriteDouble` 8321–8324 |
| `NetWriteStringNative` | `Net_WriteString` 8326–8329 |
| `NetWriteBytesNative` | `Net_WriteBytes` 8331–8335 |
| `NetDynamicBuffer` ctor | `FDynamicBuffer::FDynamicBuffer` 8343–8347 |
| `NetDynamicBuffer.DisposeBuffer` | `FDynamicBuffer::~FDynamicBuffer` 8349–8357 |
| `NetDynamicBuffer.SetData` | `FDynamicBuffer::SetData` 8359–8376 |
| `NetDynamicBuffer.GetData` | `FDynamicBuffer::GetData` 8378–8383 |
| `RemoveClassNative` | `RemoveClass` 8390–8420 |
| `SelectWeaponNative` | `SelectWeapon` 8426–8481 |
| `UseFlechetteNative` | `UseFlechette` 8483–8510 |

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `RawInvasionStateNative` | `Net_GetInvasionState` 9701–9704 |
| `InvasionStateTicsNative` | `Net_GetInvasionStateTics` 9711–9714 |
| `ClassicInvasionStateNative` | `Net_GetClassicInvasionState` 9722–9738 |
| `CountArchvilesAmongActiveMonstersNative` | `Net_GetInvasionArchvileCount` 9786–9794 |
| `ResolveControlInvasionNative` | `Net_ControlInvasion` 9894–9909 |
| `DObjectNetGetInvasionWaveActionNative` … | `DEFINE_ACTION_FUNCTION_NATIVE` 9803–9837 |
| `InvasionGetWaveThunkNative` … | `InvasionGetState` statics 9936+ |
| `InvasionControlThunkNative` | `InvasionControl` 10090–10093 |

<!-- merged from chunk610 -->
| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `AllowImplicitEmptyAtEndNative` | `Net_TrySkipUserCmdMessage` return 9579 |
| `CommandOffsetInRangeNative` | `Net_TrySkipRemainingUserCmdRecords` 9593 |
| `PlausibleNextCommandOffsetNative` | `Net_TrySkipUserCmdMessageWithBoundary` 9644–9648 |
| `GameDelayMsNative` / `LatencySeverityNative` | `Net_GetLatency` 9681–9688 |

<!-- merged from chunk614 -->
| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ClearMuteClientMaskNative` | `unmute` 10494 |
| `ClearAllMutedClientsNative` | `unmuteall` 10508 |
| `ShouldEmitAddControllerNative` | `Net_ChangeSettingsControllers` add 10544–10555 |
| `ShouldEmitRemoveControllerNative` | remove branch 10548–10555 |
| `ShouldPushClientForRemoveAllNative` | `removeallsettingscontrollers` 10625 |
| `ShouldEnqueueSettingsClientNative` | add/remove settingscontrollers parse 10580–10581 |
| `ListSettingsControllerEligibleNative` | `listsettingscontrollers` 10649 |

### Out-of-body uncredit (615)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **192** |
| **Out-of-body removed** | **0** |
| **Honest** | **192** |


**Post-review ratio:** 818/192 managed/native ≈ 426% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetNetEventWriteTests.NetAdoptServerRoomIdNative_SetsPendingForGuest`
- `VmDNetNetEventWriteTests.NetDynamicBuffer_SetData_RoundsCapacity`
- `VmDNetNetEventWriteTests.SelectWeaponNative_UsesInventoryWhenResolved`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 615 --to 615` (0 hard / 0 soft).

## Seams

`Net_NewClientTic`/`Net_Initialize`/`Net_GetCurrentRoomID` (8270–8283), `NetEvents` stream operators, `M_Realloc`/`M_Free`, iterator/VM/sound/HUD calls are not hosted.

## Credit rationale

Managed bodies mirror credited natives; one-line event pump getters are host-only.
