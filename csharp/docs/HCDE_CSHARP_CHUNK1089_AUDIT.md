# Chunk 1089 — `CommandDrawKeyBar::Parse` tail and `CommandDrawBar` ctor/draw clip/parse head from `sbarinfo_commands.cpp` (**2401–2550**); follows **1088** orientation/number seam (**2407**).

## Scope

`CommandDrawKeyBar::Parse` tail and `CommandDrawBar` ctor/draw clip/parse head from `sbarinfo_commands.cpp` (**2401–2550**); follows **1088** orientation/number seam (**2407**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawKeyBar::Parse / CommandDrawBar** 2401–2550 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawKeyBar1089Slice.cs`
Managed LOC (tally): 166.
Conversion commit: `f810ae60448187ededb9b2f046b06ac0b927af14`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Key bar `reverse` / `reverserows` flags (**2411–2416**) | yes |
| Flag list `|` vs `,` (**2417–2418**) | yes |
| Icon size `auto` vs int (**2420–2425**) | yes |
| Optional key offset / row layout (**2429–2447**) | yes (clause gates + row icon auto) |
| Parse terminating `;` (**2450**) | yes (host seam marker) |
| **CommandDrawBar** ctor defaults (**2470–2474**) | yes |
| Draw early-out (**2479–2480**) | yes |
| Border draw value invert (**2487–2489**) | yes |
| Matching bg dimensions gate (**2497–2498**) | yes |
| Clip fill remaining (**2506–2507**) | yes |
| Clip component index (**2507**) | yes |
| Border clip inset (**2509–2512**) | yes |
| Stat keywords health/armor/ammo* (**2531–2545**) | yes |
| Generic `ammo` inventory validation (**2553–2556**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2400–2407** | **1088** number/orientation seam |
| **2428**, **2622+** | host `GetCoordinates` |
| **2452–2463** | key bar member field layout |
| **2477–2505**, **2514–2520** | host `DrawGraphic` |
| **2524–2528** | host `newImage` |
| **2547–2552** | scanner token I/O outside pure policy |

| Managed (`VmSbarinfoCommandDrawKeyBar1089Slice.cs`, `VmSbarinfoCommandDrawBar1089Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------------------------------------------------|----------------------------------|
| Key bar parse tail | **2408–2451** |
| Draw bar draw/parse head | **2470–2550** |
| Band hull (exclusive slice) | **2401–2550** |

### Chunk **1089** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1088** |


**Post-review ratio:** 166/150 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawKeyBar1089SliceTests.TryClassifyKeyBarParseFlagKeyword_MapsReverseRows`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ResolveKeyBarFlagListContinuation_EndsOnComma`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ResolveKeyBarIconSizeFromParse_AutoUsesNegativeOne`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ResolveKeyBarOptionalLayoutParse_ReadsRowWhenTwoCommas`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.CreateDrawBarCtorDefaults_MatchesNativeInit`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.InvertNormalizedDrawValueForBorderDraw_FlipsWhenBorderNonZero`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ComputeDrawBarClipFillRemaining_ScalesByValue`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ResolveDrawBarClipComponentIndex_VerticalReverseUsesIndexThree`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.TryClassifyDrawBarStatKeyword_HealthUsesComparator`
- `VmSbarinfoCommandDrawKeyBar1089SliceTests.ShouldReplaceDrawBarAmmoClass_WhenNotAmmoKind`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1089 --to 1089` (0 hard / 0 soft).

## Seams

**1088** owns **2400–2407** (`number`, direction keywords); **1089** starts at post-orientation comma (**2408**). Host `GetCoordinates`, `newImage`, `DrawGraphic`, and `PClass::FindActor` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2401–2550** (+150); key bar flag/icon/optional row parse tail, **CommandDrawBar** ctor defaults, draw skip/invert/clip index policy, stat keyword head through ammo class validation (**2550** hull).
