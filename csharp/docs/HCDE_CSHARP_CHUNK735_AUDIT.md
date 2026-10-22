# Chunk 735 — Whole `AActor::FallAndSink` and `P_NightmareRespawn` (`p_mobj.cpp` 3355–3551).

## Scope

Whole `AActor::FallAndSink` and `P_NightmareRespawn` (`p_mobj.cpp` 3355–3551).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::FallAndSink** 3355–3432 (78 lines)
- `src/playsim/p_mobj.cpp` **P_NightmareRespawn** 3459–3551 (93 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`VmPMobjFallNightmare.cs`
Managed LOC (tally): 107.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `FallAndSink` | `AActor::FallAndSink` 3355–3432 |
| `NightmareRespawn` | `P_NightmareRespawn` 3459–3551 |


**Post-review ratio:** 107/171 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjFallNightmareTests.FallAndSink_AppliesGravityWhenDry`
- `VmPMobjFallNightmareTests.NightmareRespawn_FailsWhenPositionBlocked`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 735 --to 735` (0 hard / 0 soft).

## Seams

`StaticSpawn`, `P_CheckPosition`, `P_FindFloorCeiling`, and teleport fog spawns are injected.

## Credit rationale

Water sink policy and nightmare respawn orchestrator as two whole functions in one TU.
