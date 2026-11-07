# Chunk 1446 — NetUpdate send window in d_net.cpp (6940-7136).

## Scope

NetUpdate send window in d_net.cpp (6940-7136).

## Native ranges

- `src/d_net.cpp` **NetUpdate** 6940–7136 (197 lines)

**Credited native LOC:** 197 (tally `nativeLoc` 197).

## Managed

`VmNet1446Slice.cs`
Managed LOC (tally): 223.
Conversion commit: `88019ea52ea793205a0cab3b0505798fbe576675`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Send window (**6940–7136**) | yes |
| Band hull (**6940–7136**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Command generation | lines 6727-6939 are already credited |
| Packet body | line 7138 starts the consistency base and the byte loops |

Demo playback stamps every peer sequence and returns. The authority counts non-quit peers for the fragmentation cap and collects quitters. A resend-only tic is withheld unless the peer already asked for a retransmit or is missing. The header carries the low sequence across the session when this process is not the authority. An explicit resend cursor wins over a passive ack, and the passive ack still counts when the slack and the fire check both pass.

| Managed (`VmNet1446Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| SendWindow | **6940–7136** |
| Band hull (exclusive slice) | **6940–7136** |

### Chunk **1446** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **197** |
| **Net headline** | **+197** vs the prior chunk |


**Post-review ratio:** 223/197 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1446SliceTests.WindowScaleAndDemo`
- `VmNet1446SliceTests.HeadersAndPassiveResend`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1446 --to 1446` (0 hard / 0 soft).

## Seams

Newest tic through the consistency-resend arm. Packet body assembly starts at line 7138.

## Credit rationale

UNCredited d_net.cpp 6940-7136 (+197). One contiguous span inside NetUpdate, ending on the retransmit-consistency statement. The span does not cross credited code.
