# Chunk 1445 — NetUpdate command generation in d_net.cpp (6727-6939).

## Scope

NetUpdate command generation in d_net.cpp (6727-6939).

## Native ranges

- `src/d_net.cpp` **NetUpdate** 6727–6939 (213 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmNet1445Slice.cs`
Managed LOC (tally): 231.
Conversion commit: `1fa397549a649b2ff42308603208c997604a7815`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Command generation (**6727–6939**) | yes |
| Band hull (**6727–6939**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Status gate | lines 6518-6726 are already credited |
| Packet assembly | line 6940 starts the send window |

An idle call still drains packets. The authority heartbeat ages the latency window and then the level-start gate runs. A start delay is subtracted before the send cap. Command generation stops at the backup-tic lead ceiling, skips a batch without building, and on a duplicated tic ORs buttons backward and averages the axes.

| Managed (`VmNet1445Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| NetUpdate command loop | **6727–6939** |
| Band hull (exclusive slice) | **6727–6939** |

### Chunk **1445** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **213** |
| **Net headline** | **+213** vs the prior chunk |


**Post-review ratio:** 231/213 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1445SliceTests.HeartbeatDelayAndLead`
- `VmNet1445SliceTests.CommandDupAndButtons`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1445 --to 1445` (0 hard / 0 soft).

## Seams

Opening of NetUpdate through the local command loop. Packet assembly starts at line 6940.

## Credit rationale

UNCredited d_net.cpp 6727-6939 (+213). One contiguous span ending at the command-loop brace. The span does not cross credited code.
