# Chunk 1168 — Projectile geometry hits and health-group actions from `p_destructible.cpp` (**689–909**).

## Scope

Projectile geometry hits and health-group actions from `p_destructible.cpp` (**689–909**).

## Native ranges

- `src/playsim/p_destructible.cpp` **Geometry hits** 689–909 (221 lines)

**Credited native LOC:** 221 (tally `nativeLoc` 221).

## Managed

`VmPDestructibleHit1168Slice.cs`
Managed LOC (tally): 382.
Conversion commit: `1210a93c803609dee214c5c5a1d3b008140cff74`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_ProjectileHitLinedef` (**697–747**) | yes |
| `P_ProjectileHitPlane` (**750–784**) | yes |
| `P_CheckLinedefVulnerable` / `P_CheckSectorVulnerable` (**795–821**) | yes |
| Health-group serialize (**825–868**) | yes |
| ZScript field and action wrappers (**874–909**) | yes |
| Band hull (**689–909**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **442–688** | geometry radius attack is chunk **661** |
| **918–1067** | line and sector health actions are chunk **1021** |
| **910–917** | opening of `DamageLinedef`, left uncredited so this band does not end mid-function |
| `GetMissileDamage` / `P_PointOnLineSide` / `ZatPoint` | damage, side, and plane height stay host-supplied |

A blocking 3D floor with health above **0** takes strife factor **3** when `MF4_STRIFEDAMAGE` (**0x100**) is set, otherwise factor **7**, and scale **1**. The linedef side comes from the host. The other side is **1** only when that side is **0**. The closest point is `pos - unit(rotate90CW(delta)) * abs(dot(unit, v1 - pos))`. A bottom below the other floor plus **1/65536**, with floor health and a vulnerable line, damages the floor. A top above the other ceiling minus that epsilon, with ceiling health, damages the ceiling. Line health above **0** damages the line on the missile side. Any of those hits returns true.

A negative plane part becomes the ceiling when the ceiling is blocking, otherwise the floor when the floor is blocking. A blocking 3D floor returns after that check: health above **0** damages part **2**, and health **0** returns false without the floor or ceiling. Floor and ceiling parts also require health and a vulnerable sector.

A horizon line (special **9**) is not vulnerable. A missing sidedef is not vulnerable. The sector-part argument does not change that result. A 3D sector part is vulnerable. Any other part reads the ceiling texture and plane when the part is ceiling, otherwise the floor, and rejects the sky texture.

Group serialization writes id and health only. A closed array changes nothing. A reader copies health onto an existing id and skips an unknown id. A writer walks the live groups. `FindHealthGroup` returns the lookup result. `SetHealth` forwards the value and returns **0**. `DamageSector` records the position and sets groups, then returns **0**.

| Managed (`VmPDestructibleHit1168Slice.cs`) | Native (`p_destructible.cpp`) |
|---------------------------------------------|-------------------------------|
| Geometry hits | **689–909** |
| Band hull (exclusive slice) | **689–909** |

### Chunk **1168** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **221** |
| **Net headline** | **+221** vs post-**1167** |


**Post-review ratio:** 382/221 managed/native ≈ 173% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleHit1168SliceTests.LinedefHit_DamagesTheNearPlaneAndTheWall`
- `VmPDestructibleHit1168SliceTests.PlaneHit_PrefersABlockingCeilingAndStopsOnAnEmptyThreeDFloor`
- `VmPDestructibleHit1168SliceTests.Groups_CopyHealthOntoAnExistingIdOnly`
- `VmPDestructibleHit1168SliceTests.Actions_ApplyHealthAndDamageTheNamedSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1168 --to 1168` (0 hard / 0 soft).

## Seams

Missile damage, line side, and plane height stay host-supplied. `P_DamageSector` and `P_SetHealthGroupHealth` are called as recorded hits because their bodies sit outside this band.

## Credit rationale

UNCredited **`p_destructible.cpp`** **689–909** (+221). Chunk **661** ends at **688**. Chunk **1021** starts at **918**. A full tally line scan shows no overlap. **910–917** is the opening of `DamageLinedef` and stays uncredited so the band ends on the `DamageSector` action.
