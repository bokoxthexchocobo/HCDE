# Chunk 943 — `LS_Thing_Hate` from `p_lnspec.cpp` (**1565–1714**). Planned **3983–4132** corridor is absent (`p_lnspec.cpp` EOF **3982**); pivoted to next clean **150** avoiding deferred **2869–2910**, **3019–3129**, **3905–3982**.

## Scope

`LS_Thing_Hate` from `p_lnspec.cpp` (**1565–1714**). Planned **3983–4132** corridor is absent (`p_lnspec.cpp` EOF **3982**); pivoted to next clean **150** avoiding deferred **2869–2910**, **3019–3129**, **3905–3982**.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Thing_Hate** 1565–1714 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecThingHate943Slice.cs`
Managed LOC (tally): 81.
Conversion commit: `4e16723b04b209bddfcebebb0b111ec431c431be`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| chunk **943** hull | **1565–1714** |
| `HateeCandidateValid` | hatee scan **1577–1579** |
| `NothingToHateWhenTidNonZero` | **1584–1587** |
| `RejectPlayerAsHater` | player hater guard **1592–1596** |
| `HaterIteratorCandidate` | hater pick **1606–1608** |
| `ShouldRecordTidToHate` / `ShouldClearTargetForTid` | group TID **1620–1636** |
| `BuildHateAttitude` | `MF3`/`MF4` modes **1651–1674** |
| `CanChaseAfterHate` | target assign **1697–1707** |
| `HateeScanContinue` | group hatee loop **1690–1694** |
| `ShouldWakeSeeState` | `SetState(SeeState)` **1704–1706** |
| `ContinueHaterIterator` | iterator **1710–1718** |
| `AssignActivatorAsHatee` | **1676–1678** |
| `UsesGroupHateMode` | **1620** |
| `ShouldStoreLastEnemyBeforeRetarget` | **1699–1701** |
| `HasSeeStateForHate` | `SeeState` gate **1615** |
| `ShouldBreakHaterLoop` | hater iterator exit **1720–1723** |

### Chunk **943** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**942** |


**Post-review ratio:** 81/150 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecThingHate943SliceTests.HateeCandidateValid_RequiresShootableAliveAwake`
- `VmPLnspecThingHate943SliceTests.NothingToHateWhenTidNonZero_DetectsMissingHatee`
- `VmPLnspecThingHate943SliceTests.RejectPlayerAsHater_BlocksActivatorPlayer`
- `VmPLnspecThingHate943SliceTests.BuildHateAttitude_SetsNoSightForMode2`
- `VmPLnspecThingHate943SliceTests.CanChaseAfterHate_RejectsSelf`
- `VmPLnspecThingHate943SliceTests.HateeScanContinue_StopsOnDormant`
- `VmPLnspecThingHate943SliceTests.ShouldWakeSeeState_RequiresPositiveHealth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 943 --to 943` (0 hard / 0 soft).

## Seams

`GetActorIterator`, `SeeState`, inventory, and `SetState` remain host seams.

## Credit rationale

Honest in-repo **150**-line UNCredited band; exclusive `VmPLnspecThingHate943Slice.cs` (live `VmPLnspecThingHate` / retired **417** are not matrix credit for these lines).
