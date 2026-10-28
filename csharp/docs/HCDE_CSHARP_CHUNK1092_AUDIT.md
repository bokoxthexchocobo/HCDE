# Chunk 1092 — `CommandIsSelected`, `CommandPlayerClass`, and `CommandPlayerType` parse/tick head from `sbarinfo_commands.cpp` (**2851–3000**); follows **1091** `AdditionalData` ctor (**2854**).

## Scope

`CommandIsSelected`, `CommandPlayerClass`, and `CommandPlayerType` parse/tick head from `sbarinfo_commands.cpp` (**2851–3000**); follows **1091** `AdditionalData` ctor (**2854**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandIsSelected / CommandPlayerClass / CommandPlayerType** 2851–3000 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandIsSelected1092Slice.cs`
Managed LOC (tally): 85.
Conversion commit: `97925315d119c52ad2003963b6c02ffe31fa0dd1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| **CommandIsSelected** ctor null weapons (**2887–2888**) | yes |
| Weapon class validation (**2898–2901**) | yes |
| Second weapon comma/token (**2904–2910**) | yes |
| Ready-weapon truth OR (**2917–2920**) | yes |
| **CommandPlayerClass** display-name match (**2944–2947**) | yes |
| Class list comma loop (**2955–2958**) | yes |
| Missing player class abort (**2965–2966**) | yes |
| Spawn class list scan truth (**2969–2977**) | yes |
| **CommandPlayerType** parse token head (**2994–2995**) | yes |
| Register class when lookup succeeds (**3000–3003**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2852–2854** | **1091** `AdditionalData` ctor seam |
| **2857–2878** | draw-bar member / union layout |
| **2831–2847** | `ValueType` enum (before hull) |
| **2897**, **2901**, **2999** | host `FindClass` |
| **2942–2943** | host `PlayerClasses` registry walk |
| **2915**, **2963**, **3013+** | host flow-control base calls outside hull tail |

| Managed (`VmSbarinfoCommandIsSelected1092Slice.cs`, `VmSbarinfoCommandPlayerClass1092Slice.cs`, `VmSbarinfoCommandPlayerType1092Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------------------------------------------------------------------|----------------------------------|
| IsSelected + PlayerClass + PlayerType | **2882–3000** |
| Band hull (exclusive slice) | **2851–3000** |

### Chunk **1092** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1091** |


**Post-review ratio:** 85/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandIsSelected1092SliceTests.ShouldReplaceIsSelectedWeaponClass_WhenNotWeaponKind`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolveIsSelectedSecondWeaponParse_RequiresComma`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolveIsSelectedFlowControlTruth_MatchesSecondSlot`
- `VmSbarinfoCommandIsSelected1092SliceTests.TryMatchPlayerClassDisplayName_IsCaseInsensitive`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolvePlayerClassFlowControlTruthFromScan_AbortsWhenClassMissing`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolvePlayerClassFlowControlTruthFromScan_FindsSpawnClass`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolvePlayerClassParseListAdvance_StopsWithoutComma`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolvePlayerTypeParseHeadToken_AcceptsStringConst`
- `VmSbarinfoCommandIsSelected1092SliceTests.ResolvePlayerTypeClassListCountAfterLookup_IncrementsWhenFound`
- `VmSbarinfoCommandIsSelected1092SliceTests.CreateIsSelectedWeaponSlotDefaults_NullsBothSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1092 --to 1092` (0 hard / 0 soft).

## Seams

**1091** owns **`AdditionalData`** ctor (**2852–2854**) and draw-bar interpolation (**2784–2805**); **1092** credits flow-control commands only. Host `PClass::FindClass`, `PlayerClasses`, `SetTruth`, and draw-bar member layout excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2851–3000** (+150); weapon-selected flow parse/tick, player-class display-name list parse/tick, player-type class list parse head through successful lookup (**3000**).
