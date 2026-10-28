# Chunk 733 — Whole `DoDamageMobj`, `P_DamageMobj`, `AActor::OkayToSwitchTarget`, and `CallOkayToSwitchTarget` (`p_interaction.cpp` 1573–1821). Boundaries: `csharp/docs/HCDE_CSHARP_CHUNK733_737_NATIVE_BOUNDARIES.md`.

## Scope

Whole `DoDamageMobj`, `P_DamageMobj`, `AActor::OkayToSwitchTarget`, and `CallOkayToSwitchTarget` (`p_interaction.cpp` 1573–1821). Boundaries: `csharp/docs/HCDE_CSHARP_CHUNK733_737_NATIVE_BOUNDARIES.md`.

## Native ranges

- `src/playsim/p_interaction.cpp` **DoDamageMobj** 1573–1603 (31 lines)
- `src/playsim/p_interaction.cpp` **P_DamageMobj** 1617–1657 (41 lines)
- `src/playsim/p_interaction.cpp` **AActor::OkayToSwitchTarget** 1734–1801 (68 lines)
- `src/playsim/p_interaction.cpp` **CallOkayToSwitchTarget** 1810–1821 (12 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPInteractionDamageDispatch.cs`
Managed LOC (tally): 185.
Conversion commit: `07b40841bd877d1e9f152099fdfbfc8863eb79da`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `DoDamageMobj` | `DoDamageMobj` 1573–1603 |
| `PDamageMobj` | `P_DamageMobj` 1617–1657 |
| `OkayToSwitchTarget` | `AActor::OkayToSwitchTarget` 1734–1801 |
| `CallOkayToSwitchTarget` | `CallOkayToSwitchTarget` 1810–1821 |
| `MirrorOrVisualSkips` | mirror/coop early outs 1575–1580, 1619–1624 |

### Out-of-body uncredit (733)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **152** |
| **Out-of-body removed** | **0** |
| **Honest** | **152** |


**Post-review ratio:** 185/152 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionDamageDispatchTests.DoDamageMobj_SkipsMirrorActors`
- `VmPInteractionDamageDispatchTests.PDamageMobj_UsesVirtualWhenPresent`
- `VmPInteractionDamageDispatchTests.OkayToSwitchTarget_RejectsNull`
- `VmPInteractionDamageDispatchTests.OkayToSwitchTarget_RejectsSelf`
- `VmPInteractionDamageDispatchTests.CallOkayToSwitchTarget_FallsBackToNative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 733 --to 733` (0 hard / 0 soft).

## Seams

`Net_Is*` mirror gates, `VMCall`, `DamageMobj` static body, lag-comp record, and `P_CheckSight` are injected.

## Credit rationale

Faithful damage dispatch wrappers plus full target-switch predicate moved off chunk462/473 helper bands.
