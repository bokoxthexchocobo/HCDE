# Chunk 1148 — Sight, targetable players, block enemies, and LookForEnemiesEx from `p_enemy.cpp` (**1258–1488**); follows **1147**.

## Scope

Sight, targetable players, block enemies, and LookForEnemiesEx from `p_enemy.cpp` (**1258–1488**); follows **1147**.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy look and visibility** 1258–1488 (231 lines)

**Credited native LOC:** 231 (tally `nativeLoc` 231).

## Managed

`VmPEnemyLook1148Slice.cs`
Managed LOC (tally): 204.
Conversion commit: `fc91c1c793e2c55326b3858ae19c05143391d8ef`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_IsVisible` (**1268–1314**) | yes |
| `isTargetablePlayer` (**1316–1363**) | yes |
| `ValidEnemyInBlock` (**1365–1440**) | yes |
| `LookForEnemiesEx` (**1451–1478**) | yes |
| Look range macros (**1486–1487**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1100–1257** | random chase already credited by **1147** |
| **1489+** | `P_LookForMonsters` already credited |

`MF8_MVISBLOCKED` is **0x2000**. A parameter block uses its distances and, unless all-around, its FOV; otherwise the FOV is **0** or **180**. Zero distances are ignored. Outside the FOV still reacts when there is no minimum and the distance is within `meleerange + radius`. Sight is `SF_SEEPASTSHOOTABLELINES` (**2**) and runs last. A missing pawn, a non-shootable pawn, a friend, `CF_NOTARGET` (**8**), a dead player, or a failed sight check rejects the player. Without `MF6_SEEINVISIBLE`, a shadow (unless `COMPATF_INVISIBILITY`) or a ghost sneaks when farther than **128** and slower than **25** speed squared, and otherwise needs `pr_lookforplayers() >= 225`. `ValidEnemyInBlock` requires shootable, not self, alive, not dormant, a monster, and not never-target. A friendly monster pursues other hostiles and other players' friends. It does not adopt its friend's current target: `keepChecking` stays false, so that path returns false. A hostile pursues a friendly only with `MF8_SEEFRIENDLYMONSTERS`. An engaged healthy friend is skipped when `pr_skiptarget() > 100`. `LookForEnemiesEx` aborts on a null list, replaces range **-1** with `friendlyseeblocks * 128`, and returns the list size including entries already present. Players are skipped when `noPlayers` is set. `MONS_LOOK_RANGE` is **1280** and `MONS_LOOK_LIMIT` is **64**.

| Managed (`VmPEnemyLook1148Slice.cs`) | Native (`p_enemy.cpp`) |
|----------------------------------------|----------------------|
| Look scan | **1258–1488** |
| Band hull (exclusive slice) | **1258–1488** |

### Chunk **1148** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **231** |
| **Net headline** | **+231** vs post-**1147** |


**Post-review ratio:** 204/231 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLook1148SliceTests.IsVisible_RejectsDistanceAndFovAfterTheBlockFlag`
- `VmPEnemyLook1148SliceTests.IsTargetablePlayer_MissesSneakingShadowsUnlessTheRollClears`
- `VmPEnemyLook1148SliceTests.ValidEnemy_KeepsHostileTargetsAndDoesNotAdoptAFriendsTarget`
- `VmPEnemyLook1148SliceTests.LookForEnemies_UsesSeeBlocksAndSkipsPlayersWhenAsked`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1148 --to 1148` (0 hard / 0 soft).

## Seams

Sight, the angle delta, and the block iterator stay host-supplied. A null target list aborts instead of throwing.

## Credit rationale

UNCredited **`p_enemy.cpp`** **1258–1488** (+231), inside the 3488-line file, with no conversion or recognition overlap. The band closes the hole in front of credited `P_LookForMonsters`.
