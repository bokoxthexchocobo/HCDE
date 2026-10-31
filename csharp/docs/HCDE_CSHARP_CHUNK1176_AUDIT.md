# Chunk 1176 — Health-group init, geometry line attack, and radius-attack helpers from p_destructible.cpp (239–441).

## Scope

Health-group init, geometry line attack, and radius-attack helpers from p_destructible.cpp (239–441).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_GeometryLineAttack** 239–441 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmPDestructibleGeometry1176Slice.cs`
Managed LOC (tally): 345.
Conversion commit: `1e79246cae25507635000b45f76e54333bc82231`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_InitHealthGroups` (**239–294**) | yes |
| `P_GeometryLineAttack` (**303–376**) | yes |
| Radius helper struct and closest point (**385–408**) | yes |
| Explosion block and closer-point insert (**410–438**) | yes |
| Band hull (**239–441**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–238** | health-group damage is chunk **1175** |
| **442–688** | `P_GeometryRadiusAttack` is chunk **661** |
| **689–909** | projectile hits are chunk **1168** |
| **910–917** | opening of the later `DamageLinedef` stays uncredited |

Init clears the groups, then builds one from every line and sector plane whose group id is positive. A member whose health differs from the group warns with the group's health before any raise, and a higher value replaces the group health. A sector is stored on its floor group only when that group is not the ceiling group. After the walk, each mismatched id reports the highest health that remains.

A wall hit on a 3D-floor tier damages the model when that model has 3D health. A vulnerable linedef then damages the back sector's floor or ceiling for a lower or upper tier, and damages the linedef when its health is positive. A floor or ceiling hit walks solid 3D floors that are not shoot-through. A plane within one map unit of 1/65536 counts as the hit, damages 3D health when it is positive, and returns before the real sector plane. The closest-point helper clamps to the segment. An explosion probe lifts the check by one map unit, shortens the distance by one half, and blocks with the block-everything flag. A blocked probe is not stored. An equal distance keeps the earlier point.

| Managed (`VmPDestructibleGeometry1176Slice.cs`) | Native (`p_destructible.cpp`) |
|--------------------------------------------------|--------------------------------|
| Init, line attack, and radius helpers | **239–441** |
| Band hull (exclusive slice) | **239–441** |

### Chunk **1176** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **203** |
| **Net headline** | **+203** vs post-**1175** |


**Post-review ratio:** 345/203 managed/native ≈ 170% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleGeometry1176SliceTests.Init_KeepsTheHighestHealthAndSharesOneSector`
- `VmPDestructibleGeometry1176SliceTests.Attack_DamagesThe3DFloorAndStopsAtThePlane`
- `VmPDestructibleGeometry1176SliceTests.Helpers_ClampTheSegmentAndKeepTheNearerGroup`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1176 --to 1176` (0 hard / 0 soft).

## Seams

P_CheckLinedefVulnerable and P_CheckSectorVulnerable stay host-supplied. Damage calls the chunk 1175 health routines. P_GeometryRadiusAttack at 442 is chunk 661.

## Credit rationale

UNCredited p_destructible.cpp 239–441 (+203). Chunk 1175 ends at 238. 442–688 is chunk 661. A full tally line scan shows no overlap. 910–917 stays uncredited.
