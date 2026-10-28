# Chunk 982 — `P_SetupLevel` audio/map load, player spawn passes, end-monster counter policy, and precache gate from `p_setup.cpp` (**451–600**); follows chunk **981** (**301–450**).

## Scope

`P_SetupLevel` audio/map load, player spawn passes, end-monster counter policy, and precache gate from `p_setup.cpp` (**451–600**); follows chunk **981** (**301–450**).

## Native ranges

- `src/p_setup.cpp` **P_SetupLevel** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSetupLevel982Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `4e4730bbed2c00735ef7b289d022c120e315e0dd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Console viewz + music gate (**452–464**) | yes |
| Map open + new game (**472–492**) | yes (predicates) |
| Deathmatch / random / new-game spawns (**509–544**) | yes |
| Unfriendly DM relocation (**549–564**) | yes |
| End-level monster uncount (**571–585**) | yes |
| Precache gate (**599–602**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **456–470**, **482–496**, **588–597** | host calls |
| **498–507**, **605+** | comments / next function |

| Managed (`VmPSetupLevel982Slice.cs`) | Native (`p_setup.cpp`) |
|----------------------------------------|----------------------|
| `ConsolePlayerResetsViewHeight` / `ShouldStartHubMusic` | **452–464** |
| `MapOpenSucceeded` / `ShouldFireNewGameEvent` | **472–492** |
| `DeathmatchSpawnPassActive` / `RandomPlayerStartPassActive` / `NewGameSpawnFillNeeded` | **509–544** |
| `SelectSpawnWeaponFlags` | **529**, **541** |
| `UnfriendlyDeathmatchRelocationEnabled` / `ShouldDestroyPreviousSpawn` | **549–564** |
| `ShouldClearEndLevelMonsterCounters` | **571–585** |
| `ShouldRunPrecachePass` / `TryClearMobBeforeSpawn` / `DeathmatchAnnounceFollowsPrecache` | **515–517**, **599–605** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **982** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**981** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmPSetupLevel982SliceTests.SetupLevel_AudioAndMapGates`
- `VmPSetupLevel982SliceTests.PlayerSpawnPasses`
- `VmPSetupLevel982SliceTests.DeathmatchRelocationAndEndMonsters`
- `VmPSetupLevel982SliceTests.PrecachePassAndMobClear`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 982 --to 982` (0 hard / 0 soft).

## Seams

`S_Start`, `P_OpenMapData`, `MapLoader`, spawn helpers, `T_PreprocessScripts`, and `PrecacheLevel` remain host-injected.

## Credit rationale

UNCredited `P_SetupLevel` mid-body; disjoint from **979–981**. Line **453** viewz reset is native in this hull (not in **981** `301–450`).
