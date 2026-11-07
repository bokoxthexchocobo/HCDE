# Chunk 1457 — Packet trace, live endian helpers, and legacy gameplay reject in d_net.cpp (1770-1985).

## Scope

Packet trace, live endian helpers, and legacy gameplay reject in d_net.cpp (1770-1985).

## Native ranges

- `src/d_net.cpp` **Net_ShouldAggregatePacketTrace** 1770–1985 (216 lines)

**Credited native LOC:** 216 (tally `nativeLoc` 216).

## Managed

`VmNet1457Slice.cs`
Managed LOC (tally): 211.
Conversion commit: `c9f6418053fefbda94742b17278851de38bef876`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Trace and legacy reject (**1770–1985**) | yes |
| Band hull (**1770–1985**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Announcements | lines 1506-1738 are already credited |
| Capability-gated live reject | starts at line 1993 |

Latency headers aggregate until 32 packets or 1000 ms. Outgoing traces skip the console player. A legacy gameplay hit disconnects the peer only when this process is the authority and the peer is not the authority slot.

| Managed (`VmNet1457Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| ShouldAggregate through RejectLegacy | **1770–1985** |
| Band hull (exclusive slice) | **1770–1985** |

### Chunk **1457** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **216** |
| **Net headline** | **+216** vs the prior chunk |


**Post-review ratio:** 211/216 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1457SliceTests.AggregateTraceAndEndian`
- `VmNet1457SliceTests.CapabilitiesAndLegacyReject`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1457 --to 1457` (0 hard / 0 soft).

## Seams

Net_ShouldAggregatePacketTrace through HCDERejectLegacyGameplayPeer. The announcement span ends at 1738. HCDERejectLiveGameplayForCapabilities starts at 1993.

## Credit rationale

UNCredited d_net.cpp 1770-1985 (+216). One contiguous span of the trace window, the endian helpers, and the legacy gameplay reject. The span does not cross credited code.
