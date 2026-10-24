# Run 8 behavior LOC tally correction

## Finding

Run 8 credited **3,285** native lines across three chunks by attributing entire `P_Move`, `P_RadiusAttack`, and `p_enemy` movement stacks. Managed code and tests only cover narrow slices. This correction replaces those credits with line ranges whose behavior is reproduced and tested in C#.

## Corrected run 8 chunks

| Chunk | Was | Now | Native ranges counted |
| --- | ---: | ---: | --- |
| `p-map-radius-thrust-chunk2` | 1580 | **69** | `p_map.cpp` `P_RadiusAttack` **6390–6458** (thrust, thrustless damage mask, self-thrust scale, circular/Z thrust, `MF2_BLASTED` nodamage path) |
| `p-enemy-move-blocked-chunk2` | 1520 | **22** | `p_enemy.cpp` **518–524** (`MF6_JUMPDOWN` dropoff assist), **697–710** (spechit door/push + `pr_opendoor`) |
| `p-mobj-missile-spawn-live` | 185 | **81** | `p_mobj.cpp` `P_CheckMissileSpawn` **7781–7861** (randomize tics, advance, `P_TryMove`, rip/dontrip, horizon destroy, explode) |

**Run 8 native total:** 172 (was 3285). Managed run 8 total unchanged at **465**.

## Cumulative impact

- `convertedBehaviorNative`: **11079 → 7966** (−3113)
- `remainingNative`: **496007 → 500120** (baseline 508086)

## Verified

`loc_tally.json` chunk sums match corrected totals; `dotnet build -warnaserror` and full `HCDE.Playsim.Tests` green after correction commit.
