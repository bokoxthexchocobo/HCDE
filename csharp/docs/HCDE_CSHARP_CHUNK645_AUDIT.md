# Chunk 645 — Legacy heartbeat encoding and master startup advertising (`sv_master.cpp` 674–835)

## Scope

Legacy heartbeat encoding and master startup advertising (`sv_master.cpp` 674–835)

## Native ranges

- `src/common/engine/sv_master.cpp` **SendHeartbeat** 674–835 (162 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmSvMasterHeartbeat.cs`
Managed LOC (tally): 87.
Conversion commit: `31c4766166ac7776910b29286701789a77c5f16a`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `WriteLongNative` / `WriteShortNative` | 674–686 |
| `BuildLegacyHeartbeatPacketNative` | `SendHeartbeat` 688–708 |
| `MasterListContainsNative` | 710–732 |
| `MasterMatchesPrefixNative` | 734–747 |
| `IsMissingStartupMasterValueNative` | 782–788 |
| `ResolveStartupAdvertisingNative` | 814–835 |

**Post-review ratio:** 87/162 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterHeartbeatTests.BuildLegacyHeartbeatPacketNative_WritesMarkerAndPort`
- `VmSvMasterHeartbeatTests.IsMissingStartupMasterValueNative_DetectsNextSwitch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 645 --to 645` (0 hard / 0 soft).

## Seams

`sendto`, `Args->CheckParm`, and `Printf` are host callbacks.

## Credit rationale

Whole `WriteLong` through `SV_ResolveStartupAdvertising`.
