# Chunk 1443 — Heartbeat, dead clients, and desync diagnostics in d_net.cpp (6228-6517).

## Scope

Heartbeat, dead clients, and desync diagnostics in d_net.cpp (6228-6517).

## Native ranges

- `src/d_net.cpp` **SendHeartbeat** 6228–6517 (290 lines)

**Credited native LOC:** 290 (tally `nativeLoc` 290).

## Managed

`VmNet1443Slice.cs`
Managed LOC (tally): 329.
Conversion commit: `a8b5cc315ba7ee0539d352ece399ff1a99272c1d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Heartbeat through desync dump (**6228–6517**) | yes |
| Band hull (**6228–6517**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetPackets | lines 5848-6227 are already credited |
| Consistency marker | line 6518 starts the next function |

The heartbeat averages only completed probes and clamps a sample at 5000 ms. A client still in setup, and the console player, are not probed. A silent peer is disconnected once the dead-client timeout elapses, after the scan, and a zero last-packet stamp is primed instead of reaped. The legacy consistency word folds pose and health and never returns zero. The desync report is written only when the debug switch is on.

| Managed (`VmNet1443Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Heartbeat through Dump | **6228–6517** |
| Band hull (exclusive slice) | **6228–6517** |

### Chunk **1443** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **290** |
| **Net headline** | **+290** vs the prior chunk |


**Post-review ratio:** 329/290 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1443SliceTests.HeartbeatAndDeadClients`
- `VmNet1443SliceTests.SeedsConsistencyAndDump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1443 --to 1443` (0 hard / 0 soft).

## Seams

SendHeartbeat through Net_DumpSyncDiagnostics. MakeConsistencies starts at line 6518.

## Credit rationale

UNCredited d_net.cpp 6228-6517 (+290). One contiguous run of whole functions. Interior comments and the forward declaration are non-behavior. The span does not cross credited code.
