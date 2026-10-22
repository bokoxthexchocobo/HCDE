# Chunk 689 — Whole `P_SetHealthGroupHealth`, `P_DamageHealthGroup`, `P_DamageLinedef`, `P_DamageSector`, and `P_InitHealthGroups` (`p_destructible.cpp` 43–294).

## Scope

Whole `P_SetHealthGroupHealth`, `P_DamageHealthGroup`, `P_DamageLinedef`, `P_DamageSector`, and `P_InitHealthGroups` (`p_destructible.cpp` 43–294).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_SetHealthGroupHealth** 43–71 (29 lines)
- `src/playsim/p_destructible.cpp` **P_DamageHealthGroup** 73–110 (38 lines)
- `src/playsim/p_destructible.cpp` **P_DamageLinedef** 112–146 (35 lines)
- `src/playsim/p_destructible.cpp` **P_DamageSector** 148–211 (64 lines)
- `src/playsim/p_destructible.cpp` **P_InitHealthGroups** 239–294 (56 lines)

**Credited native LOC:** 222 (tally `nativeLoc` 222).

## Managed

`VmPDestructibleHealthGroups.cs`
Managed LOC (tally): 303.
Conversion commit: `1131080e2bb860eaae18fa643ac04b1d95908295`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleHealthGroups` | `P_SetHealthGroupHealth` 43–71; `P_DamageHealthGroup` 73–110; `P_DamageLinedef` 112–146; `P_DamageSector` 148–211; `P_InitHealthGroups` 239–294 |


**Post-review ratio:** 303/222 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleHealthGroupsTests.InitHealthGroups_MergesLineIntoGroup`
- `VmPDestructibleHealthGroupsTests.DamageLinedef_ReducesHealth`
- `VmPDestructibleHealthGroupsTests.DamageSector_ReducesFloorHealthAndSyncsGroup`
- `VmPDestructibleHealthGroupsTests.DamageSector_TriggersDeathActionWhenHealthReachesZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 689 --to 689` (0 hard / 0 soft).

## Seams

`P_GetHealthGroup`, `P_ActivateLine`, `WorldLineDamaged`, and sector action triggers remain native.

## Credit rationale

Destructible health-group wiring before geometry line attack bands.
