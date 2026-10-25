# Chunk 644 — NMS1 register/unregister/heartbeat orchestration (`sv_master.cpp` 497–672) Merged sources: [645]. Merged sources: [645]. Merged: [645]. Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Scope

NMS1 register/unregister/heartbeat orchestration (`sv_master.cpp` 497–672) Merged sources: [645]. Merged sources: [645]. Merged: [645]. Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `57912749ea0862fb7a7dd1b06b5b309accf149cc`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `TryRegisterNms1MasterNative` | 497–578 |
| `TryUnregisterNms1MasterNative` | 580–614 |
| `TryHeartbeatNms1MasterNative` | 616–672 |

<!-- merged chunk645 -->
| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `WriteLongNative` / `WriteShortNative` | 674–686 |
| `SendHeartbeatNative` / `BuildLegacyHeartbeatPacketNative` | 688–708 |
| `MasterListContainsNative` | 710–732 |
| `MasterMatchesPrefixNative` | 734–747 |
| `UpdateMasterServersInternalNative` | 749–765 |
| `UnregisterMasterServersInternalNative` | 767–771 |
| `IsMissingStartupMasterValueNative` | 782–788 |
| `AddStartupMastersNative` | 790–812 |
| `ResolveStartupAdvertisingNative` | 814–835 |

<!-- merged chunk645 -->
| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `WriteLongNative` / `WriteShortNative` | 674–686 |
| `SendHeartbeatNative` / `BuildLegacyHeartbeatPacketNative` | 688–708 |
| `MasterListContainsNative` | 710–732 |
| `MasterMatchesPrefixNative` | 734–747 |
| `UpdateMasterServersInternalNative` | 749–765 |
| `UnregisterMasterServersInternalNative` | 767–771 |
| `IsMissingStartupMasterValueNative` | 782–788 |
| `AddStartupMastersNative` | 790–812 |
| `ResolveStartupAdvertisingNative` | 814–835 |

### Out-of-body uncredit (644)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **319** |
| **Out-of-body removed** | **267** |
| **Honest** | **52** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 644 --to 644` (0 hard / 0 soft).

## Seams

`I_GetLocalServerSnapshot`, packet writers, and `Printf`/`DebugTrace` are host callbacks.

## Credit rationale

Whole `TryRegisterNms1Master`, `TryUnregisterNms1Master`, and `TryHeartbeatNms1Master`.
