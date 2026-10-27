# Chunk 636 — Client slots, HCDE authority, live routing, Win32 `neterror` (`i_net.cpp` 4417–4680) Merged: [635]. Restored init-network map from merged chunk635.

## Scope

Client slots, HCDE authority, live routing, Win32 `neterror` (`i_net.cpp` 4417–4680) Merged: [635]. Restored init-network map from merged chunk635.

## Native ranges

- `src/common/engine/i_net.cpp` **I_InitNetwork** 4266–4360 (95 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4362–4365 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4367–4375 (9 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4377–4386 (10 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4388–4391 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4393–4415 (23 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4417–4421 (5 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4423–4427 (5 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4429–4432 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4434–4440 (7 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4442–4448 (7 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4450–4453 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4455–4460 (6 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4462–4468 (7 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4470–4473 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4475–4478 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4480–4483 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4490–4493 (4 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4495–4507 (13 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4509–4523 (15 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4535–4563 (29 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4565–4585 (21 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4587–4595 (9 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4597–4602 (6 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4604–4609 (6 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4611–4616 (6 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4618–4623 (6 lines)
- `src/common/engine/i_net.cpp` **I_InitNetwork** 4626–4679 (54 lines)

**Credited native LOC:** 371 (tally `nativeLoc` 371).

## Managed

`VmINetAuthorityLive.cs`, `VmINetWinNetError.cs`, `VmINetInitNetwork.cs`
Managed LOC (tally): 374.
Conversion commit: `e619c981316f037b909d2be5f1bb65c5ee859cd6`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `IInitNetworkNative` | `I_InitNetwork` 4266–4360 |
| `IIsDedicatedServerModeNative` | `I_IsDedicatedServerMode` 4362–4365 |
| `IDedicatedServerRequestStartNative` | `I_DedicatedServerRequestStart` 4367–4375 |
| `IDedicatedServerRequestAbortNative` | `I_DedicatedServerRequestAbort` 4377–4386 |
| `IUsesDedicatedServerSlotNative` | `I_UsesDedicatedServerSlot` 4388–4391 |
| `IGetReservedServerSlotNative` | `I_GetReservedServerSlot` 4393–4415 |

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `IIsServerReservedSlotNative` … `IToInternalClientSlotNative` | 4417–4448 |
| `IClientUsesHcdeServiceNative` … `IGetHcdeLiveAuthoritySlotNative` | 4450–4483 |
| `IResetHcdePregameServiceProfileNative` … `IRequestHcdeResyncNative` | 4490–4523 |
| `IIsHcdeClientSetupInProgressNative` | 4535–4563 |
| `IIsHcdeLiveRoutablePeerNative` | `I_IsHCDELiveRoutablePeer` 4565–4585 |
| `IShouldSendHcdeLiveControlToNative` … `IShouldAcceptHcdeLiveServerSnapshotFromNative` | 4587–4623 |
| `NetErrorNative` | `neterror` 4626–4679 |

### Out-of-body uncredit (636)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **386** |
| **Out-of-body removed** | **15** |
| **Honest** | **371** |


**Post-review ratio:** 374/371 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmINetAuthorityLiveTests.IGetVisibleMaxClientsNative_SubtractsReservedSlot`
- `VmINetAuthorityLiveTests.NetErrorNative_MapsConnRefused`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 636 --to 636` (0 hard / 0 soft).

## Seams

`I_GetReservedServerSlot`, reliable resync, and `WSAGetLastError` are injected. Context DTOs are uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Finishes `i_net.cpp`: slot mapping, authority queries, setup gating, live send/accept routing, and Windows socket error names.
