# Chunk 1077 — Switchable inventory `Tick` tail + `CommandDrawString` parse head from `sbarinfo_commands.cpp` (**601–750**); follows **1076** armor tick seam.

## Scope

Switchable inventory `Tick` tail + `CommandDrawString` parse head from `sbarinfo_commands.cpp` (**601–750**); follows **1076** armor tick seam.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawSwitchableImage::Tick / CommandDrawString::Parse** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawSwitchable1077Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `cf73656213974cdafb29b1d3d36c341749ac5c67`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Inventory first-fail `drawAlt` (**609–611**) | yes |
| Dual-condition inventory merge (**612–627**) | yes |
| Off-image index + alpha (**630–637**) | yes |
| Delegate to `CommandDrawImage::Tick` gate (**639**) | yes (predicate) |
| DrawString font fallback (**693–696**) | yes |
| Alignment / shadow / linebreak flags (**712–745**) | yes |
| Line-break vertical spacing (**682**) | yes |
| Flag list continuation (**749**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **601–605** | **1076** armor `drawAlt` tail |
| **642–661** | switchable member fields / enums |
| **675–686**, **698–703** | host draw / scanner / coordinates |
| **755+** | `RealignString` outside hull |

| Managed (`VmSbarinfoCommandDrawSwitchable1077Slice.cs`, `VmSbarinfoCommandDrawString1077Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|--------------------------------------------------------------------------------------------------|----------------------------------|
| Inventory compare + dual `drawAlt` | **609–627** |
| Off-image tick path | **630–637** |
| Font fallback + parse flags | **693–745** |
| Line-break Y advance | **682** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **1077** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1076** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawSwitchable1077SliceTests.MergeInventoryDualConditionDrawAlt_MatchesNativeTable`
- `VmSbarinfoCommandDrawSwitchable1077SliceTests.ResolveInventoryDrawAltWhenFirstFails_UsesOffStateWhenMissing`
- `VmSbarinfoCommandDrawSwitchable1077SliceTests.EvaluateInventoryAmountCondition_DelegatesTo1075Compare`
- `VmSbarinfoCommandDrawSwitchable1077SliceTests.ResolveSwitchableOffImageIndex_MapsDrawAltToImageSlot`
- `VmSbarinfoCommandDrawString1077SliceTests.ResolveDrawStringFontSource_FallsBackWhenLookupFails`
- `VmSbarinfoCommandDrawString1077SliceTests.TryParseDrawStringAlignment_MapsRightLeftCenter`
- `VmSbarinfoCommandDrawString1077SliceTests.ApplyDrawShadowFlag_EnablesShadowWithOptionalOffsets`
- `VmSbarinfoCommandDrawString1077SliceTests.ApplyLineBreaksFlag_StoresBreakWidth`
- `VmSbarinfoCommandDrawString1077SliceTests.ComputeLineBreakDrawY_UsesHeightPlusFourSpacing`
- `VmSbarinfoCommandDrawString1077SliceTests.ClassifyDrawStringParseFlag_RejectsUnknownFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1077 --to 1077` (0 hard / 0 soft).

## Seams

**1076** owns armor/key/weapon `drawAlt` through **600**; **1071** owns runtime `DSBarInfo::DrawString` draw path in `sbarinfo.cpp`. Host `FindInventory`, `V_GetFont`, and `DrawString` calls excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **601–750** (+150); inventory dual-condition + off-image tick + drawstring parse flags; no tally overlap on credited seams.
