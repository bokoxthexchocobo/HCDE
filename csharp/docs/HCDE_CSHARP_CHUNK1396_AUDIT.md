# Chunk 1396 — DoQuakeDamage, GetModWave, GetModIntensity, and GetFalloff from a_quake.cpp (165-334).

## Scope

DoQuakeDamage, GetModWave, GetModIntensity, and GetFalloff from a_quake.cpp (165-334).

## Native ranges

- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::DoQuakeDamage** 165–334 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmQuake1396Slice.cs`
Managed LOC (tally): 167.
Conversion commit: `e8dc757d3e27a5c8222751536fba5c5420e512cd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DoQuakeDamage (**174–208**) | yes |
| GetModWave (**220–224**) | yes |
| GetModIntensity (**234–296**) | yes |
| GetFalloff (**306–322**) | yes |
| Band hull (**165–334**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Tick | ends at line 164 |
| StaticGetQuakeIntensities | starts at line 335 |
| Distance2D | callee; the slice reads the supplied distance |
| pr_quake and HitDice | callees |
| P_DamageMobj | callee; the damage type is Quake |

GetFalloff returns 1 when the actor is not strictly between the falloff and the radius, or when the falloff is not a positive gap inside the radius. GetModWave is a sine of the remaining time. GetModIntensity doubles the base, then scales it for scaledown, scaleup, both, the highpoint, and full intensity. DoQuakeDamage rolls damage only inside the radius and on the floor, forces at least 1 point, and thrusts unless the victim refuses thrust. A countdown that runs ahead of its start, or a zero divider, does not return.

| Managed (`VmQuake1396Slice.cs`) | Native (`a_quake.cpp`) |
|----------------------------------|------------------------|
| Damage, wave, intensity, and falloff | **165–334** |
| Band hull (exclusive slice) | **165–334** |

### Chunk **1396** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **170** |
| **Net headline** | **+170** vs the prior chunk |


**Post-review ratio:** 167/170 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmQuake1396SliceTests.FalloffWaveAndIntensityFollowTheFlagBranches`
- `VmQuake1396SliceTests.DamageAndThrustUseRadiusFloorAndTheRandomCalls`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1396 --to 1396` (0 hard / 0 soft).

## Seams

The tick ends at line 164. StaticGetQuakeIntensities starts at line 335. Distance2D, P_DamageMobj, pr_quake, and HitDice are callees.

## Credit rationale

UNCredited a_quake.cpp 165-334 (+170). After chunk 1395 this is the unique largest hole. r_interpolate.cpp 1-169 is 169. The band is shorter than 400 lines and is not sliced.
