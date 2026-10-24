# Chunk 777 — Weapon slot scan, resurrection, player iteration, fullbright VM wrappers, crouch sprite setup, PlayIdle gate, `A_PlayerScream`, and `P_CheckPlayerSprite` (`p_user.cpp` 1148–1572 only). Excludes userinfo `DEFINE_ACTION_FUNCTION` band 1270–1371 and inline `GetFullbrightMode`/`SetFullbrightMode` in `d_player.h`.

## Scope

Weapon slot scan, resurrection, player iteration, fullbright VM wrappers, crouch sprite setup, PlayIdle gate, `A_PlayerScream`, and `P_CheckPlayerSprite` (`p_user.cpp` 1148–1572 only). Excludes userinfo `DEFINE_ACTION_FUNCTION` band 1270–1371 and inline `GetFullbrightMode`/`SetFullbrightMode` in `d_player.h`.

## Native ranges

- `src/playsim/p_user.cpp` **player_t::HasWeaponsInSlot** 1148–1156 (9 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, HasWeaponsInSlot)** 1158–1163 (6 lines)
- `src/playsim/p_user.cpp` **player_t::Resurrect** 1165–1199 (35 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, Resurrect)** 1201–1205 (5 lines)
- `src/playsim/p_user.cpp` **player_t::GetNextPlayer** 1207–1211 (5 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(_PlayerInfo, GetNextPlayer)** 1213–1220 (8 lines)
- `src/playsim/p_user.cpp` **player_t::GetNextPlayerNumber** 1222–1234 (13 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(_PlayerInfo, GetNextPlayerNumber)** 1236–1243 (8 lines)
- `src/playsim/p_user.cpp` **GetFullbrightMode** 1245–1254 (10 lines)
- `src/playsim/p_user.cpp` **SetFullbrightMode** 1256–1268 (13 lines)
- `src/playsim/p_user.cpp` **SetupCrouchSprite** 1379–1420 (42 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(APlayerPawn, SetupCrouchSprite)** 1422–1427 (6 lines)
- `src/playsim/p_user.cpp` **PlayIdle** 1435–1442 (8 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_PlayerScream)** 1453–1520 (68 lines)
- `src/playsim/p_user.cpp` **P_CheckPlayerSprite** 1531–1572 (42 lines)

**Credited native LOC:** 278 (tally `nativeLoc` 278).

## Managed

`VmPUserWeaponSlot.cs`, `VmPUserResurrectBright.cs`, `VmPUserCrouchSprite.cs`, `VmPUserDeathIdle.cs`
Managed LOC (tally): 316.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `HasWeaponsInSlot` / `GetNextPlayerNumber` / `GetNextPlayerIndex` | **1148–1243** |
| `CanResurrect` / `ApplyTranslationAfterRevive` / `InvokeWeaponBringUp` / `InvokeUnmorph` / `ClearInvisibleRenderFlags` | `player_t::Resurrect` **1165–1205** |
| `ResolveFullbrightMode` / `ApplyFullbrightMode` | `GetFullbrightMode` / `SetFullbrightMode` wrappers **1245–1268** |
| `EvaluateSetupCrouchSprite` | `SetupCrouchSprite` **1379–1427** |
| `ShouldCallPlayIdleVirtual` | `PlayIdle` **1435–1442** |
| `PlanPlayerScream` / `AuxChannelsToStop` | `A_PlayerScream` **1453–1520** |
| `EvaluateCheckPlayerSprite` | `P_CheckPlayerSprite` **1531–1572** |


**Post-review ratio:** 316/278 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserWeaponSlotTests.HasWeaponsInSlot_FindsInventory`
- `VmPUserWeaponSlotTests.HasWeaponsInSlot_EmptySlot`
- `VmPUserResurrectBrightTests.Resurrect_AllowsLiveMo`
- `VmPUserResurrectBrightTests.Resurrect_RejectsPlayerChunk`
- `VmPUserWeaponSlotTests.GetNextPlayerIndex_FromNullStartsAtZero`
- `VmPUserWeaponSlotTests.GetNextPlayerNumber_SkipsBotsWhenRequested`
- `VmPUserWeaponSlotTests.GetNextPlayerNumber_ReturnsNegativeWhenExhausted`
- `VmPUserResurrectBrightTests.Fullbright_ExplicitModePassthrough`
- `VmPUserResurrectBrightTests.Fullbright_DefaultMapsToNightVision`
- `VmPUserResurrectBrightTests.Fullbright_DefaultMapsToFullbright`
- `VmPUserCrouchSpriteTests.SetupCrouchSprite_RejectsMissingLumps`
- `VmPUserCrouchSpriteTests.SetupCrouchSprite_AllowsWhenDisabled`
- `VmPUserDeathIdleTests.PlayIdle_RequiresVirtual`
- `VmPUserDeathIdleTests.PlayerScream_SplatUsesBodyChannel`
- `VmPUserCrouchSpriteTests.CheckPlayerSprite_UsesCrouchSprite`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 777 --to 777` (0 hard / 0 soft).

## Seams

`mo->Revive`, `SET_PLAYER_STATE`, `P_BringUpWeapon`, `P_UnmorphActor`, ACS/event hooks, `IFVIRTUALPTRNAME`/`VMCall`, `S_FindSkinnedSound*`, `S_Sound`, `fileSystem` lump lookup, and skin tables remain native.

## Credit rationale

Faithful whole-function ports for player utility/scream/sprite bands; replaces mistaken p_map portal-missile placeholder in chunk777.json.
