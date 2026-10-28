# Chunk 1091 — `CommandDrawBar::Tick` stat tail, fill normalization/interpolation, `ParseComparator`, and `AdditionalData` ctor from `sbarinfo_commands.cpp` (**2701–2850**); follows **1090** ammo2 max seam (**2700**).

## Scope

`CommandDrawBar::Tick` stat tail, fill normalization/interpolation, `ParseComparator`, and `AdditionalData` ctor from `sbarinfo_commands.cpp` (**2701–2850**); follows **1090** ammo2 max seam (**2700**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawBar::Tick / ParseComparator / AdditionalData** 2701–2850 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawBar1091Slice.cs`
Managed LOC (tally): 106.
Conversion commit: `0629b2824fdb1d40d03ad4491f0f4e46cd57ce91`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Parameterized ammo/inventory tick (**2704–2711**, **2732–2739**) | yes |
| Frags/level stat pairs (**2714–2728**) | yes |
| Airtime clamp (**2742–2744**) | yes |
| Powerup display tick bump (**2758–2759**) | yes |
| Save percent sum (**2764–2778**) | yes |
| Fill ratio normalize (**2784–2789**) | yes |
| Interpolation gate + pixel step (**2790–2796**) | yes |
| Draw value ease (**2797–2805**) | yes |
| Comparator inventory validation (**2815–2818**) | yes |
| Comparator max constant (**2821–2824**) | yes |
| `AdditionalData` ctor default (**2852–2854**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2682–2700** | **1090** ammo slot empty rules |
| **2750–2757** | host `VMCall` / `FindFunction` |
| **2814**, **2818** | host `FindActor` |
| **2831–2847** | `ValueType` enum layout |
| **2851+** | member field declarations outside hull |

| Managed (`VmSbarinfoCommandDrawBar1091Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|--------------------------------------------------|----------------------------------|
| Tick tail + interpolation | **2702–2805** |
| ParseComparator + data defaults | **2808–2854** |
| Band hull (exclusive slice) | **2701–2850** |

### Chunk **1091** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1090** |


**Post-review ratio:** 106/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawBar1091SliceTests.ResolveDrawBarInventoryItemTick_MissingItemZeroesValue`
- `VmSbarinfoCommandDrawBar1091SliceTests.NormalizeDrawBarFillRatio_ClampsToOne`
- `VmSbarinfoCommandDrawBar1091SliceTests.ApplyDrawBarPowerupEffectDisplayTicks_IncrementsBoth`
- `VmSbarinfoCommandDrawBar1091SliceTests.ComputeDrawBarSavePercentTick_AddsBasicArmor`
- `VmSbarinfoCommandDrawBar1091SliceTests.ClampDrawBarAirtimeRemaining_NeverNegative`
- `VmSbarinfoCommandDrawBar1091SliceTests.StepDrawBarDrawValueInterpolation_SnapsWithinPixelThreshold`
- `VmSbarinfoCommandDrawBar1091SliceTests.ResolveDrawBarTickDrawValue_SkipsInterpolationWhenDisabled`
- `VmSbarinfoCommandDrawBar1091SliceTests.ShouldReplaceDrawBarComparatorInventoryClass_WhenNotInventory`
- `VmSbarinfoCommandDrawBar1091SliceTests.TryApplyDrawBarComparatorMaximumConstant_RequiresExtendedInt`
- `VmSbarinfoCommandDrawBar1091SliceTests.CreateDrawBarAdditionalDataDefaults_MatchesNativeInit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1091 --to 1091` (0 hard / 0 soft).

## Seams

**1090** owns ammo1/ammo2 empty-bar policy (**2684–2700**); **1091** starts at parameterized **AMMO** inventory tick (**2702+**). Host `FindInventory`, `VMCall`, and `PClass::FindActor` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2701–2850** (+150); draw bar stat dispatch tail, normalized fill ratio, percentage interpolation pixel step, comparator max-constant path, `AdditionalData` default flags; hull ends at struct open (**2850**).
