# Chunk 512 — P_FallingDamage, player_t::Serialize, P_IsPlayerTotallyFrozen

## Scope

P_FallingDamage, player_t::Serialize, P_IsPlayerTotallyFrozen

## Native ranges

- `src/playsim/p_user.cpp` **P_FallingDamage** 1586–1681 (96 lines)
- `src/playsim/p_user.cpp` **player_t::Serialize** 2476–2607 (132 lines)
- `src/playsim/p_user.cpp` **P_IsPlayerTotallyFrozen** 2609–2615 (7 lines)

**Credited native LOC:** 235 (tally `nativeLoc` 235).

## Managed

`VmPUserSerializeFreeze.cs`
Managed LOC (tally): 59.
Conversion commit: `4787006c4521e95eecd91940afe74a0a76358efe`.

## Tests

- `VmPUserSerializeFreezeTests.FallingDamage_TelefragHighVelocity`
- `VmPUserSerializeFreezeTests.FallingDamage_ScalarBelowTelefrag`
- `VmPUserSerializeFreezeTests.Serialize_RollbackUsesAttackDown`
- `VmPUserSerializeFreezeTests.Serialize_WriteClearsButtons`
- `VmPUserSerializeFreezeTests.TotallyFrozen_ByCheat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 512 --to 512` (0 hard / 0 soft).

## Seams

`FSerializer`, `ReadUserInfo`, `P_DamageMobj`, `LastSafePos::Serialize`, and `DEFINE_FIELD` blocks are not hosted.

## Credit rationale

Credits falling-damage policy, savegame player serialization branches, and total-freeze gating from `p_user.cpp` (1586–1681, 2476–2607, 2609–2615).
