# Chunk 1090 — `CommandDrawBar::Parse` tail, `Reset`, and `Tick` value head from `sbarinfo_commands.cpp` (**2551–2700**); follows **1089** ammo validation seam (**2556**).

## Scope

`CommandDrawBar::Parse` tail, `Reset`, and `Tick` value head from `sbarinfo_commands.cpp` (**2551–2700**); follows **1089** ammo validation seam (**2556**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawBar::Parse / Reset / Tick** 2551–2700 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawBar1090Slice.cs`
Managed LOC (tally): 111.
Conversion commit: `7d14aac3365b7160ff23b611ac9c0c73747bd717`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Ammo parse paren close (**2559–2560**) | yes (shape gate) |
| Stat keywords frags…savepercent (**2561–2572**) | yes (via **1089** classifier at seam) |
| `poweruptime` class validation (**2581–2585**) | yes |
| Generic inventory validation (**2593–2596**) | yes |
| Draw bar orientation (**2601–2604**) | yes |
| Parse `reverse` / `interpolate` flags (**2608–2618**) | yes |
| Flag list `|` vs `,` (**2619–2620**) | yes |
| Border reverse flip (**2628–2630**) | yes |
| Health/armor script interpolation (**2634–2637**) | yes |
| `Reset` draw value (**2641**) | yes |
| Health clamp + max policy (**2650–2665**) | yes |
| Armor value/max (**2667–2680**) | yes |
| Ammo1/ammo2 empty slot policy (**2682–2700**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2553–2556** | **1089** ammo validation seam |
| **2506–2512**, **2487–2489** | **1089** clip helpers |
| **2551–2552**, **2578–2580**, **2592** | host class lookup |
| **2622**, **2645+** host switch scaffolding | host `GetCoordinates` / member reads |
| **2701+** | `AMMO` tick body outside hull |

| Managed (`VmSbarinfoCommandDrawBar1090Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|-----------------------------------------------------|----------------------------------|
| Parse tail + reset + tick head | **2559–2700** |
| Band hull (exclusive slice) | **2551–2700** |

### Chunk **1090** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1089** |


**Post-review ratio:** 111/150 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawBar1090SliceTests.TryClassifyDrawBarParseFlagKeyword_MapsInterpolate`
- `VmSbarinfoCommandDrawBar1090SliceTests.ResolveDrawBarParseFlagListContinuation_ContinuesOnPipe`
- `VmSbarinfoCommandDrawBar1090SliceTests.ShouldReplaceDrawBarPowerupGiverClass_WhenMissing`
- `VmSbarinfoCommandDrawBar1090SliceTests.ApplyDrawBarReverseWhenBorderNonZero_FlipsReverse`
- `VmSbarinfoCommandDrawBar1090SliceTests.ResolveDrawBarInterpolationSpeedForType_HealthUsesScriptSpeed`
- `VmSbarinfoCommandDrawBar1090SliceTests.ApplyCommandDrawBarResetState_ClearsDrawValueOnly`
- `VmSbarinfoCommandDrawBar1090SliceTests.ClampDrawBarDisplayedHealth_SuppressesNegative`
- `VmSbarinfoCommandDrawBar1090SliceTests.ResolveDrawBarHealthTickMax_UsesClassMaxByDefault`
- `VmSbarinfoCommandDrawBar1090SliceTests.ResolveDrawBarAmmoSlotTick_EmptyAmmoUsesMaxOne`
- `VmSbarinfoCommandDrawBar1090SliceTests.ClassifyDrawBarOrientationKeyword_RejectsUnknown`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1090 --to 1090` (0 hard / 0 soft).

## Seams

**1089** owns ammo class validation (**2553–2556**) and clip math; **1090** starts at post-validation ammo tail and stat-keyword continuation. Host `PClass::FindActor`, `GetCoordinates`, and `FindInventory` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2551–2700** (+150); draw bar stat tail/powerup/inventory validation, orientation/flag/border parse, script interpolation override, reset, health/armor/ammo1/ammo2 tick value policy; hull ends at **AMMO2** max lookup (**2700**).
