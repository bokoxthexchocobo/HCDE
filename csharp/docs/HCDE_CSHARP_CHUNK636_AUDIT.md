# Chunk 636 — Client slots, HCDE authority, live routing, Win32 `neterror` (`i_net.cpp` 4417–4680)

## Scope

Client slots, HCDE authority, live routing, Win32 `neterror` (`i_net.cpp` 4417–4680)

## Native ranges

- `src/common/engine/i_net.cpp` **I_IsServerReservedSlot** 4417–4680 (264 lines)

**Credited native LOC:** 264 (tally `nativeLoc` 264).

## Managed

`VmINetAuthorityLive.cs`, `VmINetWinNetError.cs`
Managed LOC (tally): 203.
Conversion commit: `4ccd8402a856da50d6a4aaf2e107f72b625193a6`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `IIsServerReservedSlotNative` … `IToInternalClientSlotNative` | 4417–4448 |
| `IClientUsesHcdeServiceNative` … `IGetHcdeLiveAuthoritySlotNative` | 4450–4483 |
| `IResetHcdePregameServiceProfileNative` … `IRequestHcdeResyncNative` | 4490–4523 |
| `IIsHcdeClientSetupInProgressNative` | 4535–4563 |
| `IIsHcdeLiveRoutablePeerNative` | `I_IsHCDELiveRoutablePeer` 4565–4585 |
| `IShouldSendHcdeLiveControlToNative` … `IShouldAcceptHcdeLiveServerSnapshotFromNative` | 4587–4623 |
| `NetErrorNative` | `neterror` 4626–4679 |

**Post-review ratio:** 203/264 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmINetAuthorityLiveTests.IGetVisibleMaxClientsNative_SubtractsReservedSlot`
- `VmINetAuthorityLiveTests.NetErrorNative_MapsConnRefused`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 636 --to 636` (0 hard / 0 soft).

## Seams

`I_GetReservedServerSlot`, reliable resync, and `WSAGetLastError` are injected. Context DTOs are uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Finishes `i_net.cpp`: slot mapping, authority queries, setup gating, live send/accept routing, and Windows socket error names.
