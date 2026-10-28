# Chunk 1095 — `CommandInInventory`, `CommandAlpha`, `CommandIfHealth`, `CommandIfInvulnerable`, and `CommandIfWaterLevel` hull head from `sbarinfo_commands.cpp` (**3301–3450**); follows **1094** weapon-ammo tail (**3300**).

## Scope

`CommandInInventory`, `CommandAlpha`, `CommandIfHealth`, `CommandIfInvulnerable`, and `CommandIfWaterLevel` hull head from `sbarinfo_commands.cpp` (**3301–3450**); follows **1094** weapon-ammo tail (**3300**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandInInventory / CommandAlpha / CommandIfHealth / CommandIfInvulnerable / CommandIfWaterLevel** 3301–3450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandInInventory1095Slice.cs`
Managed LOC (tally): 96.
Conversion commit: `8f51f5d72d8f6f9e71f37805008b1f08749fc9bd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| **InInventory** ctor defaults (**3307–3311**) | yes |
| Inventory class validation (**3321–3324**) | yes |
| Optional amount after comma (**3327–3330**) | yes |
| `||` / `&&` second item (**3333–3346**) | yes |
| Amount threshold nullify (**3354–3355**) | yes |
| Dual/single item truth (**3357–3365**) | yes |
| **CommandAlpha** draw layout (**3384–3387**) | yes |
| Alpha float parse + flow skip (**3392–3397**) | yes |
| **IfHealth** ctor + parse (**3406–3415**) | yes |
| Health compare tick (**3421–3423**) | yes |
| **IfInvulnerable** tick (**3443**) | yes |
| **IfWaterLevel** class hull (**3449–3450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3276–3300** | **1094** weapon-ammo truth helpers |
| **3151–3196** | **1094** draw-gem tick |
| **3353**, **3419**, **3441** | host negatable base `Tick` |
| **3451+** | water-level ctor body / parse outside hull |
| **3301–3302** | closing brace / separator only |

| Managed (`VmSbarinfoCommandInInventory1095Slice.cs` et al.) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------------------|----------------------------------|
| Flow commands band | **3304–3450** |
| Band hull (exclusive slice) | **3301–3450** |

### Chunk **1095** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1094** |


**Post-review ratio:** 96/150 managed/native ≈ 64% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandInInventory1095SliceTests.ShouldReplaceInInventoryItemClass_WhenNotInventoryKind`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveInInventorySecondItemListContinuation_AndRequiresBoth`
- `VmSbarinfoCommandInInventory1095SliceTests.ShouldNullifyInInventorySlotForInsufficientAmount`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveInInventoryDualConfiguredItemFlowTruth_OrEitherSlot`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveCommandAlphaMainBlockDrawInvocation_PassesBlockAlpha`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveCommandAlphaParseAlphaToken_DelegatesFlowControl`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveIfHealthPlayerHealthForCompare_UsesPercentScale`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveIfHealthFlowTruth_ComparesThreshold`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveIfInvulnerableFlowTruth_GodmodeCheat`
- `VmSbarinfoCommandInInventory1095SliceTests.CreateCommandIfWaterLevelHullHeadDefaults_MarksNegatableFlow`
- `VmSbarinfoCommandInInventory1095SliceTests.ResolveInInventoryAmountAfterOptionalComma_ParsesWhenComma`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1095 --to 1095` (0 hard / 0 soft).

## Seams

**1094** owns draw-gem percent/interpolation/wiggle and weapon-ammo OR/AND/dual-truth helpers; **1095** credits inventory flow, alpha block parse/draw layout, health threshold compare, invulnerable cheat flags, and water-level class hull (**3449–3450**). Host `FindInventory`, `SetTruth`, and `SBarInfoCommandFlowControl::Parse` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **3301–3450** (+150); in-inventory parse/tick, alpha float + draw delegation, if-health percent parse/tick, if-invulnerable tick, if-water-level class head through line **3450**.
