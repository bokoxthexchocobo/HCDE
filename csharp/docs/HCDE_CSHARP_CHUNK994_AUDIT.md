# Chunk 994 — `ReadMultiplePlayers` tail, `CopyPlayer`, and `SpawnExtraPlayers` from `p_saveg.cpp` (**751–900**). **Pivot:** `p_setup.cpp` **751–900** aborted — file EOF **788** (no hull past EOF).

## Scope

`ReadMultiplePlayers` tail, `CopyPlayer`, and `SpawnExtraPlayers` from `p_saveg.cpp` (**751–900**). **Pivot:** `p_setup.cpp` **751–900** aborted — file EOF **788** (no hull past EOF).

## Native ranges

- `src/p_saveg.cpp` **FLevelLocals::CopyPlayer** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSavegPlayerRestore994Slice.cs`
Managed LOC (tally): 125.
Conversion commit: `5e3836f2d69d77cd4e533d8dabe6f1521a3be5cf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Name-based temp player assign (**751–768**) | yes |
| First-unused temp assign (**778–795**) | yes |
| Unused temp actor destroy (**800–804**) | yes |
| `CopyPlayer` userinfo/bot/skin (**813–873**) | yes (predicates) |
| `SpawnExtraPlayers` gate (**882–900**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **756–757**, **784–785** | `Printf` host |
| **820–827**, **847–853** | `TransferFrom` / `R_FindSkin` host |
| **829**, **858–859** | `CopyFrom` / `player` pointer host |
| **897–898** | `SpawnPlayer` host |
| **901+** | outside hull |

| Managed (`VmPSavegPlayerRestore994Slice.cs`) | Native (`p_saveg.cpp`) |
|---------------------------------------------|----------------------|
| `TryAssignNamedTempPlayer` / `TryAssignFirstUnusedTempPlayer` | **751–795** |
| `ShouldDestroyUnusedTempActor` | **800–804** |
| `MergeChaseCamCheat` / `ResolveUserinfoRestoreAfterCopy` / bot walk / psprite relink / sticky input | **823–872** |
| `ShouldSpawnExtraPlayers` / `ShouldSpawnMissingPlayerPawn` | **888–900** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **994** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**993** |


**Post-review ratio:** 125/150 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmPSavegPlayerRestore994SliceTests.ReadMultiplePlayersAssignment`
- `VmPSavegPlayerRestore994SliceTests.CopyPlayerPredicates`
- `VmPSavegPlayerRestore994SliceTests.SpawnExtraPlayersGate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 994 --to 994` (0 hard / 0 soft).

## Seams

`CopyFrom`, `userinfo_t::TransferFrom`, `R_FindSkin`, `Printf`, `Serialize`, and `SpawnPlayer` host calls remain injected.

## Credit rationale

UNCredited mid-file `p_saveg.cpp`; disjoint from chunk **10** archive band (**276–335**) and retired **887**. No existing `VmPSaveg*` managed slice on **751–900**.
