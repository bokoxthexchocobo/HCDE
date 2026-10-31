# Chunk 1175 — Health-group health plus linedef and sector damage from p_destructible.cpp (1–238).

## Scope

Health-group health plus linedef and sector damage from p_destructible.cpp (1–238).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_DamageSector** 1–238 (238 lines)

**Credited native LOC:** 238 (tally `nativeLoc` 238).

## Managed

`VmPDestructibleHealth1175Slice.cs`
Managed LOC (tally): 321.
Conversion commit: `ec75395cdea661cfc5d45170644cd2270853cd36`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| File banner and geometry-damage comment (**1–42**) | yes |
| `P_SetHealthGroupHealth` (**43–71**) | yes |
| `P_DamageHealthGroup` (**73–110**) | yes |
| `P_DamageLinedef` (**112–146**) | yes |
| `P_DamageSector` (**148–211**) | yes |
| `P_GetHealthGroup` and `P_GetHealthGroupOrNew` (**220–237**) | yes |
| Band hull (**1–238**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **239–441** | init, geometry line attack, and the radius helpers stay for the next land |
| **442–688** | geometry radius attack is chunk **661** |
| **689–909** | projectile hits are chunk **1168** |
| **910–917** | opening of the later `DamageLinedef` stays uncredited |
| **918–1067** | line and sector health actions are chunk **1021** |

Setting a group's health copies that value onto every linked line and onto each sector plane whose group id matches. A missing id does nothing. Negative damage is clamped to zero before the world callback. The callback can replace that damage, and a zero result returns before health changes. A linedef subtracts the damage, clamps at zero, and activates the damage special, plus the death special when health is zero, masked by the line's activation bits. When groups are enabled, the group health becomes the linedef's new health and every other member is set to that health plus the damage and then damaged with groups disabled.

A sector uses the ceiling, floor, or 3D health for the requested part and returns immediately for any other part, after the world callback has run. A sector action target receives the damage special and, at zero health, the death special. The plane that was just damaged is not damaged again when the group walk returns to the same sector. `P_GetHealthGroupOrNew` inserts a group only when the id is missing and leaves an existing group's health alone.

| Managed (`VmPDestructibleHealth1175Slice.cs`) | Native (`p_destructible.cpp`) |
|------------------------------------------------|--------------------------------|
| Health groups and geometry damage | **1–238** |
| Band hull (exclusive slice) | **1–238** |

### Chunk **1175** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **238** |
| **Net headline** | **+238** vs post-**1174** |


**Post-review ratio:** 321/238 managed/native ≈ 135% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleHealth1175SliceTests.Group_CopiesPlaneHealthAndSpreadsLineDamage`
- `VmPDestructibleHealth1175SliceTests.Sector_ClampsDamageAndSkipsThePlaneThatWasHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1175 --to 1175` (0 hard / 0 soft).

## Seams

WorldLineDamaged, WorldSectorDamaged, and P_ActivateLine stay host-supplied. P_InitHealthGroups starts at 239.

## Credit rationale

UNCredited p_destructible.cpp 1–238 (+238). The hole runs 1–441, which is over 400, so this band stops before P_InitHealthGroups. 442–688 is chunk 661. A full tally line scan shows no overlap. 910–917 stays uncredited.
