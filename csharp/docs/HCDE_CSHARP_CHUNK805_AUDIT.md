# Chunk 805 — Whole `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, `FrictionToMoveFactor` (`p_spec.cpp` 406–501, 685–696, 751–816). Retired (2026-10-24): `--band` hull overlaps existing credit (p-spec-sector-damage-run-chunk808 src/playsim/p_spec.cpp 509-531).

## Scope

Whole `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, `FrictionToMoveFactor` (`p_spec.cpp` 406–501, 685–696, 751–816). Retired (2026-10-24): `--band` hull overlaps existing credit (p-spec-sector-damage-run-chunk808 src/playsim/p_spec.cpp 509-531).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `000bf9eede5a05cd756b0e2e24e1726a9eef525d`.

## Padding review (2026-10-21)

**Behavior ported (matrix credit)**

| Native | Behavior ported |
|--------|-----------------|
| `P_ActorInSpecialSector` **406–501** | **yes** (includes heal branch **478–495**; not on **438**) |
| `P_UpdateSpecials` **685–696** | **yes** |
| `P_SetSectorFriction` **751–797** | **yes** |
| `FrictionToMoveFactor` **799–816** | **yes** |
| `P_TestActivateLine` / `P_PredictLine` | **no** — off-tally / retired **438**; **875** does not credit `p_spec.cpp` |

### Gross vs net (**805**)

| | Value |
|--|------:|
| Gross native (`nativeLoc`) | **173** |
| Net behavior-ported | **173** |

### Out-of-body uncredit (805)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **173** |
| **Out-of-body removed** | **0** |
| **Honest** | **173** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 805 --to 805` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_GiveBody`, `P_GiveSecret`, `P_HitWater`, player iteration, and sector tag iterators are injected.

## Credit rationale

Faithful sector actor entry, timelimit exit, and friction mapping; retires slice credit on `p-spec-friction-sector-chunk439`.
