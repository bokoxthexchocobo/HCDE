# Chunk 765 — Find/create psprite, weapon bob, and button routing (`p_pspr.cpp` 227–249, 258–270, 659–821; excludes `CUSTOM_CVAR` 704–708 and credited `SetState` 485+).

## Scope

Find/create psprite, weapon bob, and button routing (`p_pspr.cpp` 227–249, 258–270, 659–821; excludes `CUSTOM_CVAR` 704–708 and credited `SetState` 485+).

## Native ranges

- `src/playsim/p_pspr.cpp` **DPSprite::DPSprite** 163–219 (57 lines)
- `src/playsim/p_pspr.cpp` **player_t::FindPSprite** 227–242 (16 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, FindPSprite)** 244–249 (6 lines)
- `src/playsim/p_pspr.cpp` **P_CreatePsprite** 258–270 (13 lines)
- `src/playsim/p_pspr.cpp` **P_BringUpWeapon** 659–673 (15 lines)
- `src/playsim/p_pspr.cpp` **R_WeaponBobSmoothFrac** 710–724 (15 lines)
- `src/playsim/p_pspr.cpp` **P_BobWeapon** 726–751 (26 lines)
- `src/playsim/p_pspr.cpp` **P_BobWeapon3D** 753–779 (27 lines)
- `src/playsim/p_pspr.cpp` **P_CheckWeaponButtons** 789–814 (26 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(APlayerPawn, CheckWeaponButtons)** 816–821 (6 lines)

**Credited native LOC:** 207 (tally `nativeLoc` 150).

## Managed

`VmPPSpriteFindPsprite.cs`, `VmPPSpriteCreatePsprite.cs`, `VmPPSpriteWeaponBob.cs`, `VmPPSpriteCheckWeaponButtons.cs`
Managed LOC (tally): 320.
Conversion commit: `c2db57e528ee481b32d4d3f0e7879b8e4be517a9`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `Construct` | `DPSprite::DPSprite` **163–219** |
| `FindPsprite` / `FindPspriteAction` | `player_t::FindPSprite` **227–242**, DEFINE **244–249** |
| `CreatePsprite` | `P_CreatePsprite` **258–270** |
| `BringUpWeapon` | **659–673** |
| `WeaponBobSmoothFrac` | **710–724** |
| `BobWeapon` / `BobWeapon3D` | **726–779** |
| `CheckWeaponButtons` / `CheckWeaponButtonsAction` | **789–821** |


**Post-review ratio:** 320/150 managed/native ≈ 213% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSpriteCreatePspriteTests.Construct_InsertsSortedAndReplacesDuplicateId`
- `VmPPSpriteFindPspriteTests.FindPsprite_ReturnsNullForLayerZero`
- `VmPPSpriteFindPspriteTests.FindPsprite_WalksLinkedList`
- `VmPPSpriteCreatePspriteTests.CreatePsprite_AppliesWeaponScaleWhenCallerIsWeapon`
- `VmPPSpriteWeaponBobTests.BringUpWeapon_InvokesVirtualHook`
- `VmPPSpriteWeaponBobTests.WeaponBobSmoothFrac_AppliesSmoothstepWhenEnabled`
- `VmPPSpriteWeaponBobTests.BobWeapon_AppliesInventoryModifyChain`
- `VmPPSpriteCheckWeaponButtonsTests.CheckWeaponButtons_SetsPspriteOnFirstMatchingState`
- `VmPPSpriteCheckWeaponButtonsTests.CheckWeaponButtons_SkipsWhenStateMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 765 --to 765` (0 hard / 0 soft).

## Seams

VM `BringUpWeapon`/`BobWeapon` virtuals, inventory override virtuals, `P_SetPsprite`, and GC/write barriers on list insert are injected.

## Credit rationale

Single-TU band linking psprite construction/lookup with the weapon bob and `ButtonChecks` loop (not a floor-padded scatter credit).
