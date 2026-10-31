# Chunk 1169 — Hitscan `P_LineAttack` from `p_map.cpp` (**4691–5050**).

## Scope

Hitscan `P_LineAttack` from `p_map.cpp` (**4691–5050**).

## Native ranges

- `src/playsim/p_map.cpp` **P_LineAttack** 4691–5050 (360 lines)

**Credited native LOC:** 360 (tally `nativeLoc` 360).

## Managed

`VmPMapLineAttack1169Slice.cs`
Managed LOC (tally): 631.
Conversion commit: `8a0b76e223d963381e09870aa3acf72593c2c053`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Pre-fire, suppress, and aim (**4703–4848**) | yes |
| Miss puff and sound (**4850–4876**) | yes |
| Wall, sky, and decal (**4877–4938**) | yes |
| Actor puff, poison, damage, and blood (**4939–5025**) | yes |
| Splash, fired event, and temporary puff (**5026–5050**) | yes |
| Band hull (**4691–5050**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **4494–4565** | aim traverse is already conversion credit |
| **4566–4690** | aim tail and `CheckForActor` stay uncredited |
| **5245–5327** | `P_TraceBleed` is already conversion credit |
| **6101–6522** | radius damage stays off the queue |
| `Trace` / `P_SpawnPuff` / `P_DamageMobj` | the trace and the effect calls stay host-supplied |

A pre-fired hitscan returns immediately. A suppressed local hitscan clears the victim and stores damage **0**. Otherwise a `lineattack` lag scope is entered. Melee sets puff flag **2**. No-random-Z sets flag **16**. The direction is `(pitchCos * angleCos, pitchCos * angleSin, -pitchSin)`. Shoot Z is center minus floor clip plus attack offset, or the actor Z when override is set, then plus the Z argument. A player adds `DMG_PLAYERATTACK` (**32**).

Ghosts are pierced when the weapon or the puff has thru-ghost. Sky explode, spectral, and missile-thru-species come from the puff defaults. Allow-thru-flags copies thru-species and thru-actors, spawns a replacement probe, copies its species and thru-bits, and destroys the probe. `None`, `Melee`, and `Hitscan` take the puff damage type when that type is not `None`. Other names stay. Trace flags start as no-sky plus impact. No-interact or no-trigger drops impact. Sky explode drops no-sky and adds hit-sky. Absolute position uses the three offsets. Absolute offset and a rotated offset call the actor placement. A zero offset uses the actor at the shoot Z.

The trace mask is shootable (**4**) and the wall mask is block-everything plus block-hitscan. A miss plays the puff active sound unless interaction is off. No-interact or always-puff spawns at the trace position with up-down **2** and returns immediately when interaction is off. A wall or sky hit runs geometry and copies the unlinked bit. Horizon without sky-explode and with interaction on does not spawn. Otherwise the puff is pulled back four units along the hit vector and its radius is **1/65536**. A solid wall that is not a horizon or a visual portal, and is not blocked by the no-decal flags, uses the puff decal when force-decal is set and the puff has one; otherwise the shooter or weapon decal; otherwise the puff decal; otherwise the shooter. A floor with no crossed water and no height sector hits water.

An actor hit shifts the bleed point by ten units when the hitscan compat bit is set, including `z -= -10 * hitZ`. No blood, dormant, invulnerable, puff-on-actors, or no-interact spawns a puff. The bleed flag is added only when the actor has blood. The victim stores the actor, the source angle, and either that angle or the linked angle-to. No-interact returns before poison. Poison runs when damage is positive and the duration is not `int.MinValue`, using the puff if one already exists. Damage runs when the amount is non-zero or the puff forces or causes pain. Pierce-armor adds no-armor. A missing puff is spawned as temporary. Target-is-source uses the shooter's target. The returned damage is stored. Bloodless impact skips the blood call and the wall bleed. Bleed uses the positive applied damage, otherwise the original amount, at the unshifted hit. Crossed water spawns a temporary puff when needed and splashes before the puff is destroyed. The fired event sees the puff. A kept puff is recorded for the cosmetic. A temporary puff is destroyed and the function returns null.

| Managed (`VmPMapLineAttack1169Slice.cs`) | Native (`p_map.cpp`) |
|-------------------------------------------|----------------------|
| `P_LineAttack` | **4691–5050** |
| Band hull (exclusive slice) | **4691–5050** |

### Chunk **1169** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **360** |
| **Net headline** | **+360** vs post-**1168** |


**Post-review ratio:** 631/360 managed/native ≈ 175% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapLineAttack1169SliceTests.Setup_StopsBeforeTheTraceAndPlacesTheShot`
- `VmPMapLineAttack1169SliceTests.Miss_PlaysTheSoundOrReturnsTheForcedPuff`
- `VmPMapLineAttack1169SliceTests.Wall_PullsThePuffBackAndChoosesTheDecal`
- `VmPMapLineAttack1169SliceTests.Actor_DamagesFromTheTargetAndBleedsAtTheUnshiftedPoint`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1169 --to 1169` (0 hard / 0 soft).

## Seams

The trace, puff spawn, damage, poison, blood, decal, water, and splash stay host-supplied. This slice decides when each one runs and which flags and positions it receives. `CheckForActor` at **4659–4689** stays uncredited.

## Credit rationale

UNCredited **`p_map.cpp`** **4691–5050** (+360). Credit before the hole ends at **4565**. `P_TraceBleed` credit starts at **5245**. A full tally line scan shows no overlap. **4566–4690** stays uncredited. Radius damage **6101–6522** is not this band.
