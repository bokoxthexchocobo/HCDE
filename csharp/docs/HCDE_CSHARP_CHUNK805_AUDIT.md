# Chunk 805 — Whole `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, `FrictionToMoveFactor` (`p_spec.cpp` 406–501, 685–696, 751–816).

## Scope

Whole `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, `FrictionToMoveFactor` (`p_spec.cpp` 406–501, 685–696, 751–816).

## Native ranges

- `src/playsim/p_spec.cpp` **P_ActorInSpecialSector** 406–501 (96 lines)
- `src/playsim/p_spec.cpp` **P_UpdateSpecials** 685–696 (12 lines)
- `src/playsim/p_spec.cpp` **P_SetSectorFriction** 751–797 (47 lines)
- `src/playsim/p_spec.cpp` **FrictionToMoveFactor** 799–816 (18 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmPSpecActorInSpecialSector.cs`
Managed LOC (tally): 168.
Conversion commit: `afd3e37d61c1875ac1906424bfdd601cf6c6ab4f`.

## Padding review (2026-10-21)

| Managed | Native (`p_spec.cpp`) |
|---------|----------------------|
| `ActorInSpecialSector` | `P_ActorInSpecialSector` **406–501** |
| `UpdateSpecialsTimelimit` | `P_UpdateSpecials` **685–696** |
| `ApplySectorFriction` | `P_SetSectorFriction` **751–797** |
| `FrictionToMoveFactor` | `FrictionToMoveFactor` **799–816** |


**Post-review ratio:** 168/173 managed/native ≈ 97% (src file LOC at HEAD; see tally).

## Tests

- `VmPSpecActorInSpecialSectorTests.ActorInSpecialSector_RepairsZeroDamageInterval`
- `VmPSpecActorInSpecialSectorTests.ActorInSpecialSector_ExitSectorKillsPlayers`
- `VmPSpecActorInSpecialSectorTests.UpdateSpecialsTimelimit_WhenExceeded`
- `VmPSpecActorInSpecialSectorTests.ApplySectorFriction_SetsMoveFactor`
- `VmPSpecActorInSpecialSectorTests.FrictionToMoveFactor_ClampsTiny`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 805 --to 805` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_GiveBody`, `P_GiveSecret`, `P_HitWater`, player iteration, and sector tag iterators are injected.

## Credit rationale

Faithful sector actor entry, timelimit exit, and friction mapping; retires slice credit on `p-spec-friction-sector-chunk439`.
