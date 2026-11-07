# Chunk 1448 — Passive resend stall and HCDE live replay in d_net.cpp (2418-2626).

## Scope

Passive resend stall and HCDE live replay in d_net.cpp (2418-2626).

## Native ranges

- `src/d_net.cpp` **HCDEPassiveResendShouldFire** 2418–2626 (209 lines)

**Credited native LOC:** 209 (tally `nativeLoc` 209).

## Managed

`VmNet1448Slice.cs`
Managed LOC (tally): 293.
Conversion commit: `58b0dcb132fbe2377fcfa1095cf5d5101d6c74ec`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Stall and live replay (**2418–2626**) | yes |
| Band hull (**2418–2626**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Report interval helper | lines 2395-2402 stay outside this span |
| Packet wrap helpers | start at line 2628 |

An unchanged ack waits eight tics and then another eight before the next fire. A zero transmit sequence is skipped. Stale gameplay packets print at most once every 2000 ms. Six replay strikes disconnect a remote peer when this process is the authority, and suppress the authority slot instead.

| Managed (`VmNet1448Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| ShouldFire through RequestReplay | **2418–2626** |
| Band hull (exclusive slice) | **2418–2626** |

### Chunk **1448** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **209** |
| **Net headline** | **+209** vs the prior chunk |


**Post-review ratio:** 293/209 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1448SliceTests.StallWindowAndFreshLane`
- `VmNet1448SliceTests.ReplayPressureAndClear`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1448 --to 1448` (0 hard / 0 soft).

## Seams

HCDEPassiveResendShouldFire through RequestHCDELiveReplay. The comment banner above the function stays outside the span. ShouldWrapHCDEClientCommandPacket starts on line 2628.

## Credit rationale

UNCredited d_net.cpp 2418-2626 (+209). One contiguous span of the stall detector, live header, sequence lanes, and replay pressure. The span does not cross credited code.
