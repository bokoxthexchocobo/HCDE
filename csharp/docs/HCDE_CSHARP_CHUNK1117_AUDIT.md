# Chunk 1117 — `P_SpawnPlayer` counter reset, inventory/respawn branches, ceiling drop, script dispatch, and `SpawnMapThing` editor-number reject from `p_mobj.cpp` (**6427–6576**); follows **1116**.

## Scope

`P_SpawnPlayer` counter reset, inventory/respawn branches, ceiling drop, script dispatch, and `SpawnMapThing` editor-number reject from `p_mobj.cpp` (**6427–6576**); follows **1116**.

## Native ranges

- `src/playsim/p_mobj.cpp` **P_SpawnPlayer tail and SpawnMapThing head** 6427–6576 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjSpawnPlayerTail1117Slice.cs`
Managed LOC (tally): 162.
Conversion commit: `632f68764788c96a31b008d24cbcc5b6990cdcdb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Live counter reset (**6427–6448**) | yes |
| Camera retarget + demo chase (**6458–6468**) | yes |
| Psprite / deathmatch / coop inventory gates (**6471–6491**) | yes |
| Old inventory destroy + respawn hook (**6493–6507**) | yes |
| Status bar, fog, ceiling drop (**6510–6525**) | yes |
| Enter vs respawn scripts and sound-target clear (**6528–6558**) | yes |
| `SpawnMapThing` ednum reject + multi flags (**6575–6576**) | yes (head) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **6450–6456**, **6478–6506** | VM calls |
| **6517**, **6532**, **6544–6554** | host fog / thinker scan / substitution |
| **6584+** | unknown-thing warning outside hull |

| Managed (`VmPMobjSpawnPlayerTail1117Slice.cs`) | Native (`p_mobj.cpp`) |
|-----------------------------------------------|---------------------|
| Spawn-player tail + ednum reject | **6427–6576** |
| Band hull (exclusive slice) | **6427–6576** |

### Chunk **1117** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1116** |


**Post-review ratio:** 162/150 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSpawnPlayerTail1117SliceTests.RunSpawnPlayerTail_RespawnClearsSoundTargetsAndDropsCeiling`
- `VmPMobjSpawnPlayerTail1117SliceTests.RunSpawnPlayerTail_SkipsScriptsForTemporaryPlayer`
- `VmPMobjSpawnPlayerTail1117SliceTests.RejectMapThingEditorNumber_RejectsZeroAndMinusOne`
- `VmPMobjSpawnPlayerTail1117SliceTests.ApplyDemoChaseCam_SetsCheatWhenWatching`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1117 --to 1117` (0 hard / 0 soft).

## Seams

VM inventory hooks, `P_SetupPsprites`, teleport fog, and thinker iteration remain host-injected; unknown-thing warnings continue at **6584**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **6427–6576** (+150). Skips **5577–5726** (chunk **481**) and earlier credited movement/tick bands.
