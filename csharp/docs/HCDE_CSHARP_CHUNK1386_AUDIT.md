# Chunk 1386 — Mirror-perf tail, buffer clear, and cutscene and tic-gate checks from d_net.cpp (4300-4493).

## Scope

Mirror-perf tail, buffer clear, and cutscene and tic-gate checks from d_net.cpp (4300-4493).

## Native ranges

- `src/d_net.cpp` **Net_ClearBuffers** 4300–4493 (194 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmDNetBufferReset1386Slice.cs`
Managed LOC (tally): 234.
Conversion commit: `44241b41345699f4f68083b18827b4a738d6924c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Case label and splices (**4300–4305**) | yes |
| Mirror perf tail (**4306–4326**) | yes |
| `Net_TickInvasionMirrorState` (**4328–4335**) | yes |
| `Net_ClearBuffers` (**4337–4454**) | yes |
| Forward declarations (**4456–4457**) | yes |
| `Net_IsCutsceneReadyParticipant` (**4459–4465**) | yes |
| `Net_IsTicGateClient` (**4467–4484**) | yes |
| `Net_IsLateJoinSyncPending` (**4486–4490**) | yes |
| Late-join entry (**4492–4493**) | yes |
| Band hull (**4300–4493**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Included snapshot and invasion fragments | other files |
| `CloseNetwork` and prediction clear | callees |
| Baseline sync, smoother, invasion reset, level-start status | callees |
| Watchdog body | outside this band |
| Late-join body | outside this band |

The mirror line prints only when the HUD debug cvar is set and the pass count is a multiple of 35, including zero. A zero pass count still prints average 0.00. Average is total microseconds over passes over 1000, and max is the max microseconds over 1000, both with two digits. Backlog is client tic minus gametic. Lag names are none, predicting, waiting, skipping, and unknown. Invasion tics decrement only when this peer is not the authority, the state is not disabled, and the tic count is positive. Clear buffers closes the network, zeroes every slot, then the report clocks, and clears only the pending-health valid bit. Slot 0 is marked in game only when it is not the reserved server slot. Network client 0 is inserted either way. A cutscene participant must be in game, not reserved, and not late-join pending. A tic-gate client must be in the network list, not reserved, and not late-join pending.

| Managed (`VmDNetBufferReset1386Slice.cs`) | Native (`d_net.cpp`) |
|------------------------------------------------|--------------------------------|
| Mirror tail, clear buffers, and gate checks | **4300–4493** |
| Band hull (exclusive slice) | **4300–4493** |

### Chunk **1386** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **194** |
| **Net headline** | **+194** vs the prior chunk |


**Post-review ratio:** 234/194 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetBufferReset1386SliceTests.MirrorPerfPrintsOnPassMultiplesAndMirrorTicDecrements`
- `VmDNetBufferReset1386SliceTests.ClearBuffersResetsPlayersAndGatesSkipReservedOrPending`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1386 --to 1386` (0 hard / 0 soft).

## Seams

Lines 4304-4305 include other files and those files stay outside this band. CloseNetwork, prediction clear, baseline sync, the view smoother, invasion reset, and level-start status stay callees. The forward declaration of the authority watchdog has no body in this band. Net_SetLateJoinSyncPending opens at the end of the band and its body stays on the later chunk. SetData(nullptr, 0) clears the tic length and leaves the command bytes. Net consistency and the last packet time are not cleared. EnterTic is copied into the last-enter fields and is not itself cleared. Pending health keeps every field except Valid.

## Credit rationale

UNCredited d_net.cpp 4300-4493 (+194). After chunk 1385 this is the unique largest hole. codegen.cpp 6316-6508 is 193. The band is shorter than 400 lines and is not sliced.
