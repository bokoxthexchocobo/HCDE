# Chunk 808 — Whole `DoSectorDamage`, `P_SectorDamage`, `P_GiveSecret`, `GiveSecret`, and `P_ActorOnSpecialFlat` (`p_spec.cpp` 509–677, 629–637).

## Scope

Whole `DoSectorDamage`, `P_SectorDamage`, `P_GiveSecret`, `GiveSecret`, and `P_ActorOnSpecialFlat` (`p_spec.cpp` 509–677, 629–637).

## Native ranges

- `src/playsim/p_spec.cpp` **DoSectorDamage** 509–531 (23 lines)
- `src/playsim/p_spec.cpp` **P_SectorDamage** 533–585 (53 lines)
- `src/playsim/p_spec.cpp` **P_GiveSecret** 596–627 (32 lines)
- `src/playsim/p_spec.cpp` **FLevelLocals::GiveSecret** 629–637 (9 lines)
- `src/playsim/p_spec.cpp` **P_ActorOnSpecialFlat** 645–677 (33 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSpecSectorDamageRun.cs`
Managed LOC (tally): 156.
Conversion commit: `11d50c67b4ccb49d6a20a668d29193fee45f12c1`.

## Padding review (2026-10-21)

| Managed | Native (`p_spec.cpp`) |
|---------|----------------------|
| `DoSectorDamage` | **509–531** |
| `SectorDamage` | **533–585** |
| `GiveSecret` | **596–627** |
| `GiveSecretAction` | **629–637** |
| `ActorOnSpecialFlat` | **645–677** |

**Post-review ratio:** 156/150 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmPSpecSectorDamageRunTests.DoSectorDamage_SkipsNonShootable`
- `VmPSpecSectorDamageTests.VavoomSwapHeights_Swaps`
- `VmPSpecSectorDamageRunTests.GiveSecret_IncrementsFound`
- `VmPSpecSectorDamageTests.TerrainDamageTick`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 808 --to 808` (0 hard / 0 soft).

## Seams

Sector tag iterators, `P_DamageMobj`, `S_Sound`, and terrain tables are injected.

## Credit rationale

Faithful sector damage iteration and secret/terrain flat hooks; retires slices on `p-spec-sector-damage-chunk442`.

## Portfolio delta (vs chunk **807** headline **93,039**)

| | Native LOC |
|---|---:|
| **Gross** (`p-spec-sector-damage-run-chunk808`) | **+150** |
| **Overlap trim** (`p-spec-sector-damage-chunk442` 150→2) | **−148** |
| **Net headline Δ** | **+2** → **93,041** |

Re-port of chunk **442** bands; not counted as forward progress under 2026-10-22 rules. **`VmPSpecSectorDamage.cs` removed**; chunk **442** cvar band only in `VmPSpecShowSecretCvars.cs`.
