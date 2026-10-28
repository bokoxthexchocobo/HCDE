# Chunk 635 — `I_InitNetwork` and dedicated slot helpers (`i_net.cpp` 4266–4415)

## Scope

`I_InitNetwork` and dedicated slot helpers (`i_net.cpp` 4266–4415)

## Native ranges

- `src/common/engine/i_net.cpp` **I_InitNetwork** 4266–4415 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmINetInitNetwork.cs`
Managed LOC (tally): 153.
Conversion commit: `ce22e81662d6ef9785e0bc889bc2093d2f33c31d`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `IInitNetworkNative` | `I_InitNetwork` 4266–4360 |
| `IIsDedicatedServerModeNative` | `I_IsDedicatedServerMode` 4362–4365 |
| `IDedicatedServerRequestStartNative` | `I_DedicatedServerRequestStart` 4367–4375 |
| `IDedicatedServerRequestAbortNative` | `I_DedicatedServerRequestAbort` 4377–4386 |
| `IUsesDedicatedServerSlotNative` | `I_UsesDedicatedServerSlot` 4388–4391 |
| `IGetReservedServerSlotNative` | `I_GetReservedServerSlot` 4393–4415 |

**Post-review ratio:** 153/150 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmINetInitNetworkTests.IInitNetworkNative_SinglePlayerPath`
- `VmINetInitNetworkTests.IGetReservedServerSlotNative_ReturnsAuthorityForDedicated`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 635 --to 635` (0 hard / 0 soft).

## Seams

`HostGame`, `JoinGame`, `Args`, and `HCDE_ServerMode_*` are injected.

## Credit rationale

Whole `I_InitNetwork` plus dedicated-server request and reserved-slot helpers.
