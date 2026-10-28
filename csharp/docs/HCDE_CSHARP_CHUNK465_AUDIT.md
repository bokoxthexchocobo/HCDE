# Chunk 465 — aim_t result helpers, P_AimLineAttack, CheckForActor

## Scope

aim_t result helpers, P_AimLineAttack, CheckForActor

## Native ranges

- `src/playsim/p_map.cpp` **aim_t::Clone** 3943–3957 (15 lines)
- `src/playsim/p_map.cpp` **aim_t::SetResult** 3965–3984 (20 lines)
- `src/playsim/p_map.cpp` **aim_t::Result** 3992–4007 (16 lines)
- `src/playsim/p_map.cpp` **P_AimLineAttack** 4574–4638 (65 lines)
- `src/playsim/p_map.cpp` **CheckForActor** 4659–4689 (31 lines)

**Credited native LOC:** 147 (tally `nativeLoc` 147).

## Managed

`VmPEnemyLookPlayers.cs`, `VmPEnemyLookPlayersTests.cs`, `VmPMapAimLineAttack.cs`, `VmPMapAimLineAttackTests.cs`, `VmPMobjNightmareHash.cs`, `VmPMobjNightmareHashTests.cs`
Managed LOC (tally): 108.
Conversion commit: `ef628f22f2692eb799cbc51beaa22fb4e5cac25d`.

## Tests

- `VmPMapAimLineAttackTests.SetResultWhenCloser`
- `VmPMapAimLineAttackTests.WeaponNoAutoaim_Flag`
- `VmPMapAimLineAttackTests.SpectralSkip_WhenPuffNotSpectral`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 465 --to 465` (0 hard / 0 soft).

## Seams

`aim_t::AimTraverse`, `P_AimLineAttack_ShadowHandling`, and `FPathTraverse` are not hosted.

## Credit rationale

Credits autoaim vrange policy, aim result selection, line-attack trace skip rules from `p_map.cpp`.
