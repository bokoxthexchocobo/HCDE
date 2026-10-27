# Chunk 932 — `ChangeCamera` activator tail, `PROP_*` enum, and `LS_SetPlayerProperty` power branch through all-player grant (`p_lnspec.cpp` **2869–3018**). Follows **931** (**≤2868**); flag-property `switch` at **3027** deferred; **3019–3129** sub-150 tail deferred.

## Scope

`ChangeCamera` activator tail, `PROP_*` enum, and `LS_SetPlayerProperty` power branch through all-player grant (`p_lnspec.cpp` **2869–3018**). Follows **931** (**≤2868**); flag-property `switch` at **3027** deferred; **3019–3129** sub-150 tail deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_ChangeCamera** 2869–3018 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecCameraProperty932Slice.cs`
Managed LOC (tally): 98.
Conversion commit: `62890983430a608cdf731fe74351baefcdd8eaaf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `AssignActivatorCamera` | `ChangeCamera` activator branch **2869–2872** |
| `ShouldClearViewer` | **2873–2876** |
| `ChangeCamera` return | **2879–2880** |
| `PROP_*` constants | enum **2882–2909** |
| `ShouldRejectPropertyCall` | `LS_SetPlayerProperty` guard **2918–2919** |
| `IsPowerProperty` / `PowerIndex` / `PowerNameAt` | power table **2922–2946** |
| `PowerEntryValid` / `IsAllMapPower` | **2943–2946**, **2952–2963** |
| `IsGivePower` / activator give-take | **2948–2979** |
| `ApplyToAllPlayers` / `PlayerSlotActive` | all-player loop **2981–3018** |
| `AllmapFlags2ForGrant` | console ALLMAP **2960–2963**, **3001–3004** |
| `InvulnerabilityBlendApplies` | **2955–2958**, **2996–2999** |
| `PowerGrantUsesInventory` / `PowerRevokeUsesDestroy` | inventory paths **2952–2973**, **2993–3014** |

### Chunk **932** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**931** |


**Post-review ratio:** 98/150 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecCameraProperty932SliceTests.AssignActivatorCamera_ClearsRevertWhenNoCamera`
- `VmPLnspecCameraProperty932SliceTests.ShouldRejectPropertyCall_WhenNoActivatorPlayer`
- `VmPLnspecCameraProperty932SliceTests.PowerBranch_RejectsInvalidPowerSlot`
- `VmPLnspecCameraProperty932SliceTests.AllmapGrant_UpdatesLevelFlags`
- `VmPLnspecCameraProperty932SliceTests.InvulnerabilityBlend_AppliesOnGive`
- `VmPLnspecCameraProperty932SliceTests.ShouldClearViewer_WhenReferenceChanges`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 932 --to 932` (0 hard / 0 soft).

## Seams

`R_ClearPastViewer`, `GiveInventoryType`, `FindInventory`, `Destroy`, and player iteration remain host seams.

## Credit rationale

Honest **150**-line slice in **2269–3129** corridor; exclusive `VmPLnspecCameraProperty932Slice.cs`.
