# Chunk 1453 — Latency and invasion accessors in d_net.cpp (9679-9911).

## Scope

Latency and invasion accessors in d_net.cpp (9679-9911).

## Native ranges

- `src/d_net.cpp` **Net_GetLatency** 9679–9911 (233 lines)

**Credited native LOC:** 233 (tally `nativeLoc` 233).

## Managed

`VmNet1453Slice.cs`
Managed LOC (tally): 153.
Conversion commit: `c6d24f5a93218efbce7964a3a285f2eccfcf6209`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Latency and invasion accessors (**9679–9911**) | yes |
| Band hull (**9679–9911**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| User-command skip | lines 9508-9676 are already credited |
| ZScript export wrappers | start at line 9914 |

Delay of 160 ms or more is severity 3. A local authority in countdown reports the larger of the state timer and the cutscene timer. Archvile counting ignores dead and euthanized monsters. Control does nothing unless this peer is the invasion authority and the game type is 4.

| Managed (`VmNet1453Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Latency through Control | **9679–9911** |
| Band hull (exclusive slice) | **9679–9911** |

### Chunk **1453** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **233** |
| **Net headline** | **+233** vs the prior chunk |


**Post-review ratio:** 153/233 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1453SliceTests.LatencyCountdownAndClassicMap`
- `VmNet1453SliceTests.CountsArchvilesAndControl`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1453 --to 1453` (0 hard / 0 soft).

## Seams

Net_GetLatency through Net_ControlInvasion. Net_SkipCommand ends at 9676. The ZScript InvasionGetState wrappers start at 9914.

## Credit rationale

UNCredited d_net.cpp 9679-9911 (+233). One contiguous span of the latency grade, invasion counters, and the control switch. The span does not cross credited code.
