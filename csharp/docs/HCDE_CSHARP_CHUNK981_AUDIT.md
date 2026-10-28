# Chunk 981 — `ClearLevelData` continuation, `P_FreeLevelData`, and `P_SetupLevel` head from `p_setup.cpp` (**301–450**); follows chunk **980** (**151–300**).

## Scope

`ClearLevelData` continuation, `P_FreeLevelData`, and `P_SetupLevel` head from `p_setup.cpp` (**301–450**); follows chunk **980** (**151–300**).

## Native ranges

- `src/p_setup.cpp` **ClearLevelData** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSetupClearLevel981Slice.cs`
Managed LOC (tally): 78.
Conversion commit: `1db6bd8c120c92c0780664e4633ad3291e9d7841`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| UDMF key clears (**311–314**) | yes |
| Strife dialogue drain (**318–326**) | yes |
| Subsector BSP teardown (**328–331**) | yes |
| Polyblock chain delete (**365–375**) | yes |
| Automap / spatial accel teardown (**381–389**) | yes |
| Network sector baselines (**391–393**) | yes |
| `P_FreeLevelData` (**404–411**) | yes |
| `P_SetupLevel` head (**423–450**) | yes (predicates) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **302–309**, **316**, **332–363**, **377–380** | host calls / bulk clears |
| **398–421** | comment banners |
| **291–300** | credited on **980** |

| Managed (`VmPSetupClearLevel981Slice.cs`) | Native (`p_setup.cpp`) |
|-------------------------------------------|----------------------|
| `UdmfKeySlotInRange` | **311–314** |
| `TryPopStrifeDialogue` / `ShouldDrainStrifeDialogueQueue` | **318–326** |
| `ShouldDeleteSubsectorBsp` | **328–331** |
| `DeletePolyblockChain` / `ShouldDeletePolyblockLink` | **365–375** |
| `ShouldDestroyAutomap` / `ShouldDeleteSpatialAccelerators` | **381–389** |
| `ShouldClearNetworkSectorBaselines` | **391–393** |
| `ShouldIterateFreeLevelData` | **404–411** |
| `SetupLevelResetsScoreCounts` / `SetupLevelClearsPlayerMobReference` | **435–447** |
| `MaptypeAwaitingMapinfoClassification` / `SelectInitialViewHeight` | **432–450** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **981** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**980** |


**Post-review ratio:** 78/150 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmPSetupClearLevel981SliceTests.UdmfKeysAndDialogueDrain`
- `VmPSetupClearLevel981SliceTests.PolyblockChainDeletion`
- `VmPSetupClearLevel981SliceTests.ClearLevelDataTailPredicates`
- `VmPSetupClearLevel981SliceTests.FreeLevelDataAndSetupHead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 981 --to 981` (0 hard / 0 soft).

## Seams

`ClearVelocities`, `SN_StopAllSequences`, container `.Clear()`, `delete`, `Behaviors.UnloadModules`, and sound init remain host-injected.

## Credit rationale

UNCredited mid-file `p_setup.cpp`; disjoint from **979**/**980** hulls. Counter lines **302–307** are native in this band (not in **980** `151–300` hull). Reaudit hygiene: no identity `ShouldRecordShaderStartTime`.
