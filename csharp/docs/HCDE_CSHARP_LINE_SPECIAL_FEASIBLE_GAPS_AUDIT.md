# Feasible line-special gaps audit

## Scope

Non-poly specials: 138, 139, 157, 174, 179, 180, 218.

## Implementation

- `LineSpecialFeasibleGaps.cs`, `ThingSpawnLineSpecials.ExecuteSpawnFacing`

## Notes

- 157/179 accept calls without full renderer/skill mutation (scripts continue).
- Wind pushes write `LevelSector.PushForce*`; waggle adjusts `sim.Floors[]`.

## Verified

- Full Playsim suite green; gap count 10 → 3 after gap regen.
