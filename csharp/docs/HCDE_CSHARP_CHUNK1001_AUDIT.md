# Chunk 1001 — `G_DoPlayerPop`, screenshot queue, load-game action, save WAD validation, and save CVAR filter from `g_game.cpp` (**2449–2598**); follows chunk **1000** (**2021–2170**). Orphan session managed files not listed.

## Scope

`G_DoPlayerPop`, screenshot queue, load-game action, save WAD validation, and save CVAR filter from `g_game.cpp` (**2449–2598**); follows chunk **1000** (**2021–2170**). Orphan session managed files not listed.

## Native ranges

- `src/g_game.cpp` **G_DoPlayerPop** 2449–2598 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameSession1001Slice.cs`
Managed LOC (tally): 80.
Conversion commit: `403dc0460c5107c415d4a7c4a9751455a3e8c1e5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_DoPlayerPop` spy/status/morph teardown (**2463–2491**) | yes |
| `G_ScreenShot` gate (**2503–2508**) | yes |
| `G_LoadGame` (**2517–2521**) | yes |
| `CheckSingleWad` / `G_CheckSaveGameWads` (**2530–2569**) | yes |
| `C_SerializeCVars` write filter (**2593–2598**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2449–2447** | prior spawn function close |
| **2457–2460**, **2572–2576** | string `Printf` host |
| **2476–2496** | script/destroy host |
| **2601+** | deserialize branch (next band) |
| Orphan `VmGGameLoadGame*` / `VmGGamePlayerPop` | not re-credited |

| Managed (`VmGGameSession1001Slice.cs`) | Native (`g_game.cpp`) |
|----------------------------------------|----------------------|
| `ShouldRevertSpyCameraToSelf` / morph helpers | **2465–2491** |
| `ShouldQueueScreenshot` | **2503–2508** |
| `TryCreateLoadGameRequest` | **2517–2521** |
| `IsResourceWadLoaded` / WAD merge helpers | **2530–2569** |
| `ShouldSerializeCvarForSave` | **2593–2598** |
| Band hull (exclusive slice) | **2449–2598** |

### Chunk **1001** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1000** |


**Post-review ratio:** 80/150 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameSession1001SliceTests.PlayerPopAndScreenshot`
- `VmGGameSession1001SliceTests.LoadGameAndWadChecks`
- `VmGGameSession1001SliceTests.SaveCvarFilter`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1001 --to 1001` (0 hard / 0 soft).

## Seams

`Printf`, `GStrings`, `M_ScreenShot`, and `arc()` host I/O remain injected.

## Credit rationale

UNCredited **2449–2598** CLI band; disjoint from chunks **995–1000** and chunk **9** credited segments.
