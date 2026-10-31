# Chunk 1150 — Clear last heard, wander, and A_Look2 from `p_enemy.cpp` (**2245–2396**); follows **1149**.

## Scope

Clear last heard, wander, and A_Look2 from `p_enemy.cpp` (**2245–2396**); follows **1149**.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy wander and A_Look2** 2245–2396 (152 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPEnemyWander1150Slice.cs`
Managed LOC (tally): 144.
Conversion commit: `b43c887f1d9c8f0de62abaf79e8d523dfdc7ed93`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `A_ClearLastHeard` (**2255–2260**) | yes |
| Chase flags (**2268–2281**) | yes |
| `A_Wander` (**2283–2322**) | yes |
| `A_Look2` (**2330–2381**) | yes |
| Boss strafe range (**2395**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1778–2062** | player look already credited by **1149** |
| **2063–2244** | `A_LookEx` already credited |
| **2397+** | `A_DoChase` already credited |

`A_ClearLastHeard` drops the heard actor. `A_Wander` clears `MF4_INCOMBAT` (**0x8000**) before returning for conversation or `MF4_STANDSTILL` (**0x20000**). A nonzero reaction time counts down and skips the step. Otherwise yaw snaps with `floor(yaw / 45) * 45`, then steps **45** degrees toward `movedir * 45` when `deltaangle` is not zero and `CHF_NODIRECTIONTURN` is clear. `180` degrees normalizes to **-180** through a signed BAM, so that facing turns backward. `movecount` decrements first. A negative count chases a new direction unless `CHF_NORANDOMTURN` is set, and that path does not call `P_SmartMove`. A failed smart move also chases unless `CHF_STOPIFBLOCKED` is set. A new direction adds **5** to the decremented count. `A_Look2` returns during conversation before clearing the threshold. A dead heard actor is ignored. Different friendliness, or `LEVEL_NOALLIES` (**0x20000000**), sees a shootable actor unless it is ambushing without sight. Matching friends enter the see state and `MF4_INCOMBAT` only when the player search succeeds, and their threshold stays **0**. A hostile wake sets the threshold to **10**. The idle path plays spawn frame `(roll & 1) + 1` when `pr_look2() < 30`, consuming the second roll only in that case, then spawn frame **3** when not standing still and the next roll is below **40**. `CLASS_BOSS_STRAFE_RANGE` is **640**. Sight for an ambush uses `SF_SEEPASTBLOCKEVERYTHING` (**4**).

| Managed (`VmPEnemyWander1150Slice.cs`) | Native (`p_enemy.cpp`) |
|------------------------------------------|----------------------|
| Wander and `A_Look2` | **2245–2396** |
| Band hull (exclusive slice) | **2245–2396** |

### Chunk **1150** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **152** |
| **Net headline** | **+152** vs post-**1149** |


**Post-review ratio:** 144/152 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyWander1150SliceTests.WanderStep_ClearsCombatAndTurnsBeforeANewDirection`
- `VmPEnemyWander1150SliceTests.Look2_SeesAHostileAndFidgetsWhenTheSearchFails`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1150 --to 1150` (0 hard / 0 soft).

## Seams

Smart movement and the player search stay host-supplied. The facing delta uses the native BAM `Normalized180`.

## Credit rationale

UNCredited **`p_enemy.cpp`** **2245–2396** (+152), inside the 3488-line file, with no conversion or recognition overlap. `A_DoChase` at **2397** is already credited.
