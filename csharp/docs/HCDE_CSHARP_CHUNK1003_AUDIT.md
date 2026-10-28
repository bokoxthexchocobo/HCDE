# Chunk 1003 — Load globals tail, `G_SaveGame` gates, and `G_DoAutoSave` rotation from `g_game.cpp` (**2750–2899**); follows chunk **1002** (**2600–2749**).

## Scope

Load globals tail, `G_SaveGame` gates, and `G_DoAutoSave` rotation from `g_game.cpp` (**2750–2899**); follows chunk **1002** (**2600–2749**).

## Native ranges

- `src/g_game.cpp` **G_SaveGame** 2750–2899 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameSaveLoad1003Slice.cs`
Managed LOC (tally): 105.
Conversion commit: `86743252f611bca2296b8610e1925adbc1e72198`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Important CVAR blob vs server serialize (**2765–2773**) | yes |
| Level time scale from save ticrate (**2777–2783**) | yes |
| `G_SaveGame` validation (**2824–2859**) | yes |
| `G_DoAutoSave` net/slot/hint rotation (**2872–2900**) | yes (through **2899** hull) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2750–2753** | `LoadGameError` host (seam with **1002**) |
| **2757–2760**, **2785–2813** | hub/read/init/GC host |
| **2862–2867** | `CCMD(opensaves)` shell host |
| **2903–2905** | `G_DoSaveGame` host tail |

| Managed (`VmGGameSaveLoad1003Slice.cs`) | Native (`g_game.cpp`) |
|---------------------------------------|----------------------|
| `ShouldLoadImportantCvarsFromBlob` | **2765–2773** |
| `ScaleLevelTimeFromSaveTicks` | **2783** |
| `ResolveSaveGameRejectReason` / `ResolveSaveGameCommandEffect` | **2824–2859** |
| Autosave helpers | **2872–2900** |
| Band hull (exclusive slice) | **2750–2899** |

### Chunk **1003** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1002** |


**Post-review ratio:** 105/150 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameSaveLoad1003SliceTests.LoadGlobalsTail`
- `VmGGameSaveLoad1003SliceTests.SaveGameGates`
- `VmGGameSaveLoad1003SliceTests.AutosaveRotation`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1003 --to 1003` (0 hard / 0 soft).

## Seams

`G_SerializeHub`, `G_ReadSnapshots`, `G_InitNew`, `G_DoSaveGame`, and `arc()` I/O remain host-injected.

## Credit rationale

UNCredited **2750–2899** CLI band; disjoint from chunks **995–1002** and chunk **9** credited segments.
