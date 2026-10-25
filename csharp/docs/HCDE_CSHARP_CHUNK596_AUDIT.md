# Chunk 596 — TryRunTics scheduler (7447–7780) + stall-branch guards (7845–8184); chunk 597 retired

## Scope

TryRunTics scheduler (7447–7780) + stall-branch guards (7845–8184); chunk 597 retired

## Native ranges

- `src/d_net.cpp` **TryRunTics** 7447–7464 (18 lines)
- `src/d_net.cpp` **TryRunTics** 7512–7548 (37 lines)
- `src/d_net.cpp` **TryRunTics** 7566–7608 (43 lines)
- `src/d_net.cpp` **TryRunTics** 7720–7780 (61 lines)
- `src/d_net.cpp` **TryRunTics** 7845–7849 (5 lines)
- `src/d_net.cpp` **TryRunTics** 7894–7896 (3 lines)
- `src/d_net.cpp` **TryRunTics** 7947–7948 (2 lines)
- `src/d_net.cpp` **TryRunTics** 7951–7954 (4 lines)
- `src/d_net.cpp` **TryRunTics** 7959–7959 (1 lines)
- `src/d_net.cpp` **TryRunTics** 7970–7971 (2 lines)
- `src/d_net.cpp` **TryRunTics** 7972–7978 (7 lines)
- `src/d_net.cpp` **TryRunTics** 8102–8104 (3 lines)
- `src/d_net.cpp` **TryRunTics** 8132–8133 (2 lines)
- `src/d_net.cpp` **TryRunTics** 8138–8143 (6 lines)
- `src/d_net.cpp` **TryRunTics** 8144–8147 (4 lines)
- `src/d_net.cpp` **TryRunTics** 8152–8156 (5 lines)
- `src/d_net.cpp` **TryRunTics** 8168–8168 (1 lines)
- `src/d_net.cpp` **TryRunTics** 8171–8174 (4 lines)
- `src/d_net.cpp` **TryRunTics** 8177–8180 (4 lines)
- `src/d_net.cpp` **TryRunTics** 8181–8182 (2 lines)

**Credited native LOC:** 214 (tally `nativeLoc` 214).

## Managed

`VmDNetTryRunTicsScheduler.cs`, `VmDNetTryRunTicsStall.cs`
Managed LOC (tally): 279.
Conversion commit: `410ea35a611288a72bccf5aa20093e74fc852694`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `GetClientPredictionEndCapTicNative` | 7432–7435 |
| `ShouldWaitForEnterTicNative` / `ClampSingleticsTicsNative` | 7447–7464 |
| `ComputeLowestSequenceNative` / `ApplyDedicatedSnapshotFrontierNative` | 7512–7548 |
| `ComputeAvailableTicsAuthorityNative` / `ComputeAvailableTicsClientNative` | 7566–7608 |
| `InitialRunTicsNative` / `ApplyPredictionLeadCapNative` | 7720–7780 |
| `ShouldSuppressTicGateStallWarningNative` | 7845–7849 |
| `IsSustainedDedicatedTicGateStallNative` | 7894–7896 |
| `ShouldRunTicStabilityWaitNative` | `if (totalTics <= 0)` 7947–7948 |
| `SelectLagStateWhenNoRunTicsNative` | `LagState` assign 7951–7954 |
| `ShouldAdvanceCommandPredictionNative` | `ClientTic > startCommand` 7959 |
| `IsStaleAuthorityGateNative` | 7970–7971 |
| `ShouldReportPredictionFaultNative` | 7972–7978 |
| `ShouldWritePredictionFaultBundleNative` | 8102–8104 |
| `ShouldUnpredictOnBacklogNative` | 8132–8133 |
| `SelectLagStateOnBacklogNative` | 8138–8143 |
| `ShouldMirrorVisualWhilePredictingNative` | 8144–8147 |
| `ShouldRunCoopInterpolationWhilePredictingNative` | 8152–8156 |
| `ShouldRunClientSideLogicWhenIdleNative` | 8168 |
| `ShouldVerifyPredictedEntitiesNative` | 8171–8174 |
| `SoundWorldTimerNative` | 8173 |
| `IsTicGateStalledNative` | 8177–8180 |
| `ShouldTickStalledCutsceneNative` | 8181–8182 |

### Out-of-body uncredit (596)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **214** |
| **Out-of-body removed** | **0** |
| **Honest** | **214** |


**Post-review ratio:** 279/214 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetTryRunTicsSchedulerTests.ClampSingleticsTicsNative_LimitsToOne`
- `VmDNetTryRunTicsSchedulerTests.ComputeAvailableTicsClientNative_AddsDedicatedBonus`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 596 --to 596` (0 hard / 0 soft).

## Seams

`GC::CheckGC`, `I_WaitForTic`, `NetUpdate`, `Net_PredictionDebugTick`, and `DebugTrace` are not hosted.

## Credit rationale

Scheduler half of `TryRunTics` plus faithful stall-branch guard expressions (merged ex-597); no whole-branch 7835–8185 credit.
