# Chunk 546 — Net_TickInvasionMirrorState, Net_ClearBuffers reset plans, cutscene/tic-gate/late-join helpers

## Scope

Net_TickInvasionMirrorState, Net_ClearBuffers reset plans, cutscene/tic-gate/late-join helpers

## Native ranges

- `src/d_net.cpp` **Net_TickInvasionMirrorState** 4328–4335 (8 lines)
- `src/d_net.cpp` **Net_ClearBuffers** 4337–4372 (36 lines)
- `src/d_net.cpp` **Net_ClearBuffers** 4375–4410 (36 lines)
- `src/d_net.cpp` **Net_ClearBuffers** 4411–4436 (26 lines)
- `src/d_net.cpp` **Net_ClearBuffers** 4438–4453 (16 lines)
- `src/d_net.cpp` **Net_IsCutsceneReadyParticipant** 4459–4465 (7 lines)
- `src/d_net.cpp` **Net_IsTicGateClient** 4467–4484 (18 lines)
- `src/d_net.cpp` **Net_IsLateJoinSyncPending** 4486–4490 (5 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmDNetClearBuffers.cs`, `VmDNetClearBuffersTests.cs`
Managed LOC (tally): 185.
Conversion commit: `764eef53bfaef0e24c628278980ca8603856fe7b`.

## Tests

- `VmDNetClearBuffersTests.NextInvasionStateTics_DecrementsWhenActive`
- `VmDNetClearBuffersTests.DefaultClientSlotClearPlan_MatchesNativeZeros`
- `VmDNetClearBuffersTests.DefaultGlobalNetClearPlan_ClearsSessionFlags`
- `VmDNetClearBuffersTests.ShouldSetConsolePlayerInGameOnClear_SkipsDedicatedAuthoritySlot`
- `VmDNetClearBuffersTests.IsCutsceneReadyParticipant_MatchesNativeGates`
- `VmDNetClearBuffersTests.IsTicGateClient_ExcludesLateJoinPending`
- `VmDNetClearBuffersTests.IsLateJoinSyncPending_MatchesBitmask`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 546 --to 546` (0 hard / 0 soft).

## Seams

`CloseNetwork`, `P_ClearPredictionData`, `Net_ResetInvasionState`, `NetEvents.ResetStream`, and live `ClientStates` tic buffers are not hosted.

## Credit rationale

Credits native reset and gate bands mirrored by `VmDNetClearBuffers` default plans and boolean gates — not full `Net_ClearBuffers` teardown.
