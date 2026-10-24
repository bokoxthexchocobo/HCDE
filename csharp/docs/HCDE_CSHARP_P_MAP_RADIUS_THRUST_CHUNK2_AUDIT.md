# p_map.cpp radius thrust chunk 2 audit

## Scope

Native `src/playsim/p_map.cpp` `P_RadiusAttack` thrust path (**6390–6458**, 69 lines behavior-credited): explosion thrust from falloff `points`, self-thrust scale, circular thrust, Z thrust, `MF2_BLASTED`, vulnerable/shootable eligibility, `RADF_*` thrust-less and no-damage thrust branches (flat subset).

## Implementation

- `MapRadiusAttack.Apply` thrust after damage; `SelfThrustScale` on `AuthoritySimulation`
- Rockets/projectiles and `A_Explode` share the same radius attack core

## Tests

`MapRadiusThrustTests`; rocket splash / DontHarm regressions unchanged.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17,019).
