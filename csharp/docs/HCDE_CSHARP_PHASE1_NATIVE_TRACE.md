# Phase 1 Gate 5 — native trace pairing (scaffold)

Gate 5 in the master plan requires tick-level comparison against a **native**
HCDE/UZDoom build on a fixed IWAD + map, not managed-only checksums alone.

This document defines the managed baseline we will diff against once a native
recorder exists.

## Managed baseline fixture

- Test: `ManagedGameplayTraceTests.Map01RoomIdleBaseline_IsStable`
- Map: single sector, player 1 at (32, 64), no lines, no behavior lump
- Seed: `0x48534445` (`"HCDE"` as little-endian uint)
- Duration: 35 tics with no player input
- Recorded fields today: simulation `Checksum` (`67100778` after adding configurable default view height), player position, health, tic count. Earlier hashes `1525314970` (actor gravity), `4261759856` (JumpZ), `251903926` (texture transforms) and `3995474422` used smaller field sets and are not comparable.

When native recording lands, capture the same map/seed/duration and compare:

| Field | Managed source | Native (TBD) |
|-------|----------------|--------------|
| Tic count | `Thinkers.Clock.Tic` | level time |
| Player X/Y | fixed units | mobj coords |
| Health | `PlayerPawn.Health` | player mobj health |
| Checksum | `AuthoritySimulation.Checksum` | optional aggregate hash |

## Native recorder (not implemented)

Planned inputs:

- IWAD path and map name (likely `MAP01` test lump or minimal PWAD)
- CLI flag to dump CSV or JSON each tic: time, player xyz, health, key flags

Until that exists, Gate 5 stays **open**. Updating the managed baseline checksum
is expected when unrelated simulation fields join `RecomputeChecksum`.
