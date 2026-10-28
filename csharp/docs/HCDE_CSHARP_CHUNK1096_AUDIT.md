# Chunk 1096 — `CommandIfWaterLevel` parse/tick, `CommandIfCVarInt`, and `NextCommand` dispatch head from `sbarinfo_commands.cpp` (**3451–3600**); follows **1095** water-level hull (**3450**).

## Scope

`CommandIfWaterLevel` parse/tick, `CommandIfCVarInt`, and `NextCommand` dispatch head from `sbarinfo_commands.cpp` (**3451–3600**); follows **1095** water-level hull (**3450**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandIfWaterLevel / CommandIfCVarInt / NextCommand** 3451–3600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandFlowControl1096Slice.cs`
Managed LOC (tally): 123.
Conversion commit: `8d2d12d7f962078a2ec714e342d642aaa2bc3e42`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| IfWaterLevel parse threshold (**3458–3459**) | yes |
| IfWaterLevel tick compare (**3465**) | yes |
| IfCVarInt ctor equalcomp false (**3477**) | yes |
| CVar bool/int threshold parse (**3495–3508**) | yes |
| IfCVarInt tick == vs >= (**3530–3531**) | yes |
| Command name table ordinals (**3545–3556**) | yes |
| Enum ordering (**3559–3570**) | yes |
| NextCommand identifier gate (**3574–3576**) | yes |
| Switch cases drawimage→ininventory (**3579–3600**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3304–3450** | **1095** inventory/alpha/if-health/invulnerable + water hull |
| **3452–3454** | empty ctor (host base only) |
| **3489**, **3526** | host `FindCVar` / `GetCVar` |
| **3463**, **3523** | host negatable base `Tick` |
| **3601–3605** | alpha/if* cases outside hull |
| **3608–3613** | unknown-command error + closing brace tail outside hull |

| Managed (`VmSbarinfoCommandFlowControl1096Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| IfWaterLevel + IfCVarInt + NextCommand head | **3456–3600** |
| Band hull (exclusive slice) | **3451–3600** |

### Chunk **1096** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1095** |


**Post-review ratio:** 123/150 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveIfWaterLevelFlowTruth_RequiresMinimumLevel`
- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveIfWaterLevelParseNegatableThreshold_StoresToken`
- `VmSbarinfoCommandFlowControl1096SliceTests.ClassifyIfCVarIntSupportedType_RejectsUnsupported`
- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveIfCVarIntParseThresholdFields_EnablesEqualComparison`
- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveIfCVarIntFlowTruthFromLiveValue_UsesGreaterOrEqualByDefault`
- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveIfCVarIntFlowTruthFromLiveValue_MissingCVarIsFalse`
- `VmSbarinfoCommandFlowControl1096SliceTests.TryResolveSBarInfoFlowCommandKind_MapsIfCVarIntOrdinal`
- `VmSbarinfoCommandFlowControl1096SliceTests.IsNextCommandDispatchCaseWithinBand3600_StopsAtInInventory`
- `VmSbarinfoCommandFlowControl1096SliceTests.ResolveNextCommandWhenIdentifierMissing_ClosesBlock`
- `VmSbarinfoCommandFlowControl1096SliceTests.TryResolveSBarInfoFlowCommandKind_MapsWeaponAmmo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1096 --to 1096` (0 hard / 0 soft).

## Seams

**1095** owns inventory OR/AND, amount/truth, if-health percent compare, invulnerable tick, and water-level hull defaults (**3449–3450**); **1096** credits water-level threshold parse/tick, full if-cvar-int, command-name table ordinals, and switch cases through **SBARINFO_ININVENTORY** (**3600**). Host `FindCVar`, `GetCVar`, `SetTruth`, and `new Command*` factories excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **3451–3600** (+150); if-water-level (**3456–3468**), if-cvar-int parse/tick (**3476–3541**), `SBarInfoCommandNames` + enum + `NextCommand` through in-inventory case (**3545–3600**).
