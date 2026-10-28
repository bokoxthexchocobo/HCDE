# Chunk 1000 — `G_SpawnPlayer` bot tail, `CheckSpot`, deathmatch start selection, and `PickDeathmatchStart` from `g_game.cpp` (**2021–2170**); follows chunk **999** (**1101–1250**). Orphan `VmGGameSpawnSpot*` / `VmGGamePickDeathmatch` not listed.

## Scope

`G_SpawnPlayer` bot tail, `CheckSpot`, deathmatch start selection, and `PickDeathmatchStart` from `g_game.cpp` (**2021–2170**); follows chunk **999** (**1101–1250**). Orphan `VmGGameSpawnSpot*` / `VmGGamePickDeathmatch` not listed.

## Native ranges

- `src/g_game.cpp` **FLevelLocals::CheckSpot** 2021–2170 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameSpawnDm1000Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `e79a2e692ba033aa642de4682325fd2abdff299e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Bot reinit tail (**2023–2030**) | yes |
| `CheckSpot` (**2040–2081**) | yes (predicates + overlap) |
| `PlayersRangeFromSpot` (**2092–2109**) | yes |
| `SelectFarthestDeathmatchSpot` (**2113–2130**) | yes |
| `SelectRandomDeathmatchSpot` (**2134–2148**) | yes |
| `PickDeathmatchStart` (**2151–2178**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2012–2020** | `IFVIRTUAL GiveDefaultInventory` host |
| **2074–2077** | `MF_SOLID` / `P_CheckPosition` host |
| **1–2020** | upstream `G_SpawnPlayer` body |
| Orphan spawn/pick managed | not re-credited |

| Managed (`VmGGameSpawnDm1000Slice.cs`) | Native (`g_game.cpp`) |
|----------------------------------------|----------------------|
| `ShouldSyncReadyWeaponAfterInventory` / `AssignReadyFromPending` | **2012–2020** |
| `ShouldReinitializeBot` / `CaptureBotSkill` | **2024–2029** |
| `IsInvalidMapthingType` / spot Z helpers / `AnotherLivingPlayerAtSpot` | **2046–2061** |
| `NearestLivingPlayerDistance` | **2094–2109** |
| `SelectFarthestDeathmatchIndex` | **2119–2127** |
| `SelectRandomOpenDeathmatchIndex` | **2138–2148** |
| `ResolvePickDeathmatchStart` / `CountPickDeathmatchReturns` | **2157–2178** |
| Band hull (exclusive slice) | **2021–2170** |

### Chunk **1000** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**999** |


**Post-review ratio:** 158/150 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameSpawnDm1000SliceTests.SpawnPlayerBotTail`
- `VmGGameSpawnDm1000SliceTests.CheckSpotAndRange`
- `VmGGameSpawnDm1000SliceTests.DeathmatchSelection`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1000 --to 1000` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `IFVIRTUAL` inventory, and `pr_dmspawn` RNG remain host-injected.

## Credit rationale

UNCredited **2021–2170** CLI band; disjoint from chunks **995–999** and chunk **9** credited segments.
