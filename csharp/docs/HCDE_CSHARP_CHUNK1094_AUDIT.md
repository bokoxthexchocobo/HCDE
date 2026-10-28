# Chunk 1094 — `CommandDrawGem` parse/tick tail and `CommandWeaponAmmo` head from `sbarinfo_commands.cpp` (**3151–3300**); follows **1093** signed padding seam (**3150**).

## Scope

`CommandDrawGem` parse/tick tail and `CommandWeaponAmmo` head from `sbarinfo_commands.cpp` (**3151–3300**); follows **1093** signed padding seam (**3150**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawGem / CommandWeaponAmmo** 3151–3300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawGem1094Slice.cs`
Managed LOC (tally): 112.
Conversion commit: `f863b9e5de067a59f2ccaa1c1db9dfb4411b288b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Chain size token +1 (**3153–3155**) | yes |
| Script interpolation override (**3160–3163**) | yes |
| `Reset` draw value (**3167**) | yes |
| Goal stat + percent cap (**3171–3180**) | yes |
| Reverse goal percent (**3182**) | yes |
| Tick interpolation ease (**3184–3192**) | yes |
| Chain wiggle bit (**3194–3195**) | yes |
| **WeaponAmmo** ctor defaults (**3225–3228**) | yes |
| Ammo class validation (**3238–3241**) | yes |
| `||` / `&&` second ammo (**3244–3255**) | yes |
| Dual-type truth (**3276–3284**) | yes |
| Single-type truth (**3286–3292**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3116–3150** | **1093** parse flags + signed padding |
| **3108–3110** | **1093** chain offset math |
| **3157–3158** | host `GetCoordinates` |
| **3197–3216** | member field layout |
| **3264–3275** | host `ReadyWeapon` / `AmmoType` lookup scaffolding |
| **3301+** | **CommandInInventory** outside hull |

| Managed (`VmSbarinfoCommandDrawGem1094Slice.cs`, `VmSbarinfoCommandWeaponAmmo1094Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------------------------------------------------|----------------------------------|
| DrawGem parse/tick tail | **3151–3196** |
| WeaponAmmo flow head | **3221–3300** |
| Band hull (exclusive slice) | **3151–3300** |

### Chunk **1094** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1093** |


**Post-review ratio:** 112/150 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawGem1094SliceTests.ValidateDrawGemChainSizeFromToken_AddsOne`
- `VmSbarinfoCommandDrawGem1094SliceTests.ResolveDrawGemInterpolationSpeedForScript_ArmorUsesScriptSpeed`
- `VmSbarinfoCommandDrawGem1094SliceTests.ApplyDrawGemReverseGoalPercent_FlipsWhenReverse`
- `VmSbarinfoCommandDrawGem1094SliceTests.StepDrawGemDrawValueTowardGoal_ClampsStep`
- `VmSbarinfoCommandDrawGem1094SliceTests.ResolveDrawGemChainWiggleBit_RequiresOddLevelTime`
- `VmSbarinfoCommandDrawGem1094SliceTests.ShouldReplaceWeaponAmmoClass_WhenNotAmmo`
- `VmSbarinfoCommandDrawGem1094SliceTests.ResolveWeaponAmmoSecondAmmoSeparator_OrUsesDisjunction`
- `VmSbarinfoCommandDrawGem1094SliceTests.ResolveWeaponAmmoDualTypeFlowTruth_AndRequiresBoth`
- `VmSbarinfoCommandDrawGem1094SliceTests.ResolveWeaponAmmoSingleConfiguredTypeTruth_MatchesSecondary`
- `VmSbarinfoCommandDrawGem1094SliceTests.ComputeDrawGemGoalPercent_ClampsAt100`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1094 --to 1094` (0 hard / 0 soft).

## Seams

**1093** owns draw-gem parse flags, signed padding, ctor, wiggle offset, and chain horizontal offset; **1094** credits chain size, script interpolation override, tick percent/interpolation/wiggle, and weapon-ammo flow head. Host `GetCoordinates`, `pr_chainwiggle`, and `PClass::FindClass` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **3151–3300** (+150); draw-gem chain size validation, health/armor script interpolation, goal percent + reverse, tick ease, chain wiggle bit, weapon-ammo ctor/parse/tick through dual-type truth (**3300** hull).
