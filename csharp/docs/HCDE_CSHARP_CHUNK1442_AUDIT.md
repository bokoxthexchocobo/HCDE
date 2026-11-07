# Chunk 1442 — Inbound packet pump GetPackets in d_net.cpp (5848-6227).

## Scope

Inbound packet pump GetPackets in d_net.cpp (5848-6227).

## Native ranges

- `src/d_net.cpp` **GetPackets** 5848–6227 (380 lines)

**Credited native LOC:** 380 (tally `nativeLoc` 380).

## Managed

`VmNet1442Slice.cs`
Managed LOC (tally): 464.
Conversion commit: `7f8584d0fd05a77a10139ca69eb439ca9d690278`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Inbound packet pump (**5848–6227**) | yes |
| Band hull (**5848–6227**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Malformed-packet strike note | lines 5796-5847 stay outside this span |
| Latency ack struct | lines 5837-5843 stay outside this span |
| Heartbeat | line 6228 starts the next function |

Exit, setup, latency, level-ready, and live packets return before the gameplay body. A short header, a truncated field, and a bad player index request the missing sequence and do not clear the strike counter. Six strikes inside five seconds queue a quit when this process is the authority and the sender is not the authority slot. A well-formed packet clears the counter. Consistency and command records are inserted at their offset, so a hole or a skipped number sets the missing flag. The authority's own packet does not advance another player's sequence. Latency acknowledgements are sent once per client after the receive loop.

| Managed (`VmNet1442Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| GetPackets | **5848–6227** |
| Band hull (exclusive slice) | **5848–6227** |

### Chunk **1442** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **380** |
| **Net headline** | **+380** vs the prior chunk |


**Post-review ratio:** 464/380 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1442SliceTests.ControlPackets`
- `VmNet1442SliceTests.GameplayFieldsAndStrikes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1442 --to 1442` (0 hard / 0 soft).

## Seams

The strike note, command skipper, quit, and level-start check are callees. SendHeartbeat starts at line 6228.

## Credit rationale

UNCredited d_net.cpp 5848-6227 (+380). One contiguous function. The trailing blank is non-behavior. The span does not cross credited code.
