# Chunk 1082 — `CommandDrawNumber::Tick` stat dispatch tail and draw-value interpolation from `sbarinfo_commands.cpp` (**1351–1500**); follows **1081** through **AMMO1CAPACITY** null-check (**1350**).

## Scope

`CommandDrawNumber::Tick` stat dispatch tail and draw-value interpolation from `sbarinfo_commands.cpp` (**1351–1500**); follows **1081** through **AMMO1CAPACITY** null-check (**1350**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawNumber::Tick** 1351–1500 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawNumber1082Slice.cs`
Managed LOC (tally): 115.
Conversion commit: `9fa9bd9bab9aa7fe25c35a4203809279a00c284f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| **AMMO1/2CAPACITY** max amount (**1354–1364**) | yes |
| Parameterized **AMMOCAPACITY** default fallback (**1369–1372**) | yes |
| Intermission / score stat dispatch (**1375–1398**) | yes |
| **ARMORCLASS** / **SAVEPERCENT** slot math (**1402–1417**) | yes |
| **GLOBALVAR** / **GLOBALARRAY** reads (**1419–1424**) | yes (call-site policy only) |
| Powerup time seconds conversion (**1434**) | yes |
| Generic inventory amount (**1439–1443**) | yes (delegates **1081**) |
| **AIRTIME** above/under water (**1448–1451**) | yes |
| **SELECTEDINVENTORY** optional update (**1455–1456**) | yes |
| **KEYS** fold counter (**1465–1470**) | yes |
| **INTCVAR** typed read fallback (**1475–1488**) | yes |
| Draw-value interpolation (**1492–1500**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1349–1353** | **1081** ammo1 null abort seam |
| **1368**, **1403**, **1429–1433**, **1439**, **1474** | host pawn/inventory/VM/CVar lookup |
| **1419–1423**, **1459–1462** | host field reads (wiring only at call site) |
| **1501+** | **whenNotZero** / translation outside hull |

| Managed (`VmSbarinfoCommandDrawNumber1082Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| Capacity + level stat resolution | **1354–1398** |
| Armor class / save percent | **1402–1417** |
| Powerup / air / keys / cvar tick policy | **1434–1488** |
| Interpolation step + snap | **1492–1500** |
| Band hull (exclusive slice) | **1351–1500** |

### Chunk **1082** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1081** |


**Post-review ratio:** 115/150 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveParameterizedAmmoCapacityMax_UsesDefaultWhenMissing`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveDrawNumberPrimaryLevelStat_MapsKillsAndSecrets`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ComputeDrawNumberArmorClassOrSavePercent_DividesOnlyArmorClass`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ConvertPowerupEffectTicsToDisplaySeconds_MatchesNativeFormula`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveDrawNumberAirTimeTickNum_SwitchesOnWaterLevel`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveSelectedInventoryTickNum_LeavesPriorWhenMissing`
- `VmSbarinfoCommandDrawNumber1082SliceTests.FoldDrawNumberKeyCount_IncrementsForKeys`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveDrawNumberIntCVarTickNum_FallsBackToZero`
- `VmSbarinfoCommandDrawNumber1082SliceTests.StepDrawNumberInterpolationTowardTarget_ClampsStep`
- `VmSbarinfoCommandDrawNumber1082SliceTests.ResolveDrawNumberDisplayValueAfterTick_SnapsWhenInterpolationDisabled`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1082 --to 1082` (0 hard / 0 soft).

## Seams

**1081** owns missing-ammo abort through **1350** `{`; **1080** owns `IntCVarAcceptedType` and level-stat keyword kinds. Host `FindInventory`, `GetDefaultByType`, `GetCVar`, and `VMCall` for poweruptime excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1351–1500** (+150); capacity/stat/armorclass/airtime/key/cvar tick values + interpolation toward `drawValue`; hull ends before **whenNotZero** (**1501+**).
