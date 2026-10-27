# Chunk 917 — `StartFloorSound`, `DFloor::Serialize` / `DEFINE_FIELD`, and full `DFloor::Tick` through stairlock chain from `mapthinkers/a_floor.cpp` (**45–225**). Stops before `SetFloorChangeType` **233+**; wider **226–940** deferred.

## Scope

`StartFloorSound`, `DFloor::Serialize` / `DEFINE_FIELD`, and full `DFloor::Tick` through stairlock chain from `mapthinkers/a_floor.cpp` (**45–225**). Stops before `SetFloorChangeType` **233+**; wider **226–940** deferred.

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **StartFloorSound** 45–61 (17 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::Serialize** 72–106 (35 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::Tick** 114–225 (112 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmAFloorDflooorHead917Slice.cs`
Managed LOC (tally): 289.
Conversion commit: `ca5c8af9e00bb6f03ae5326875a66a30bd90a983`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `StartFloorSound` (**45–61**) | yes |
| `DFloor::Serialize` (**72–90**) | yes |
| `DEFINE_FIELD` bundle (**92–106**) | yes (field manifest) |
| `DFloor::Tick` (**114–225**) | yes (stairlock chain **201–220**, `Destroy` seam **222**) |

| Managed (`VmAFloorDflooorHead917Slice.cs`) | Native (`a_floor.cpp`) |
|---------------------------------------------|----------------------|
| `PlayFloorSound` / `ShouldSkipFloorSound` | **45–61** |
| `CaptureSerializeFields` / `ApplySerializeFields` | **72–90** |
| `DfloorDefinedFieldNames` | **92–106** |
| `AdvanceStairTiming` | **119–143** |
| `TickFloorMover` / `ApplyPastDestFloorChange` | **145–197** |
| `ReleaseStairlockChainIfComplete` | **198–220** |
| `destroy` callback | **222** |

### Chunk **917** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **≥150** (three `nativeRanges`) |
| **Net headline** | **+nativeLoc** vs post-**916** |


**Post-review ratio:** 289/164 managed/native ≈ 176% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorDflooorHead917SliceTests.PlayFloorSound_UsesSeqTypeWhenSet`
- `VmAFloorDflooorHead917SliceTests.PlayFloorSound_SkipsWhenSilentMove`
- `VmAFloorDflooorHead917SliceTests.SerializeBundle_RoundTripsFields`
- `VmAFloorDflooorHead917SliceTests.DfloorDefinedFieldNames_MatchesSerializeArity`
- `VmAFloorDflooorHead917SliceTests.AdvanceStairTiming_ResetCountZero_SetsResetStair`
- `VmAFloorDflooorHead917SliceTests.AdvanceStairTiming_PauseTime_ReturnsEarly`
- `VmAFloorDflooorHead917SliceTests.TickFloorMover_PastDest_AppliesTextureAndPromotesStairlock`
- `VmAFloorDflooorHead917SliceTests.TickFloorMover_BuildStairPastDest_BecomesWaitStair`
- `VmAFloorDflooorHead917SliceTests.PromoteStairlockBuilding_NoOpWhenNotBuilding`
- `VmAFloorDflooorHead917SliceTests.ReleaseStairlockChainIfComplete_ClearsLinearChain`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 917 --to 917` (0 hard / 0 soft).

## Seams

`MoveFloor`, `SN_*` sequences, interpolation stop, and `Destroy` are injected; no reuse of retired chunk **798** tally row.

## Credit rationale

Honest **≥150** UNCredited pivot after `p_acs` RunScript corridor; exclusive `VmAFloorDflooorHead917Slice.cs` (chunk **798** retired, **0** matrix credit).
