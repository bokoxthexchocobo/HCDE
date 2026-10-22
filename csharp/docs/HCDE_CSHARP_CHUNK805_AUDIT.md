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

## Portfolio delta (vs `convertedBehaviorNative` **93,153** at commit `6cce2c20`, pre–805)

Baseline headline **93,153** (`6cce2c20`, after chunk 804). After **805+806** overlap hygiene closes at **`057c9a38`**, headline **93,034** (**−119** net). This chunk’s gross credit and overlap moves:

| Tally row | Gross / trim (native LOC) | When |
| --- | ---: | --- |
| **805** `p-spec-actor-sector-friction-chunk805` | **+173** | `10ffe036` (tally row added) |
| **439** `p-spec-friction-sector-chunk439` | **−150** | `d7886022` (ranges cleared; `nativeLoc` 0) |
| **438** `p-spec-test-activate-line-chunk438` | **−18** | `d7886022` (168→150) |
| **442** `p-spec-sector-damage-chunk442` | **−5** | `d7886022` (155→150; secret-slice trim only) |

**805 close (`d7886022`):** gross **+173**, overlap **−173** → headline restored to **93,153**.

### Re-port / duplicate managed code

Chunk **439** had already credited overlapping `p_spec.cpp` bands (actor-in-sector helpers, friction, timelimit) in `VmPSpecFrictionSector.cs`. **805** is a **whole-function** re-port in `VmPSpecActorInSpecialSector.cs` for `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, and `FrictionToMoveFactor`. Native credit moved to **805** and **439**’s ranges were cleared in **`d7886022`** (same sync as overlap fix, not in `10ffe036`). **`VmPSpecFrictionSector.cs` removed** (2026-10-22 course correction); friction/timelimit tests now target `VmPSpecActorInSpecialSector`.

### Process / gate (805 tally commit)

| Commit | `LocTallyIntegrity` | Hardened `verify_full_solution_tests.sh` | Notes |
| --- | --- | --- | --- |
| `10ffe036` | **FAIL** (header drift **−172**, range overlap) | **FAIL** (`MigrationMatrix`) | **RED** on `origin`; tally landed before overlap retire. |
| `d7886022` | **PASS** | **PASS** | Overlap retire + headline resync. |

**Rule:** land only through `land_chunk_fast.sh` (overlap retired in the **same** tally commit as the new row).
