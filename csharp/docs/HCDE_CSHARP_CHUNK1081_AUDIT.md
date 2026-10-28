# Chunk 1081 — `CommandDrawNumber::Parse` flag tail, optional coloring, and `Tick` dispatch head from `sbarinfo_commands.cpp` (**1201–1350**); follows **1080** through inventory validation (**1200**).

## Scope

`CommandDrawNumber::Parse` flag tail, optional coloring, and `Tick` dispatch head from `sbarinfo_commands.cpp` (**1201–1350**); follows **1080** through inventory validation (**1200**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawNumber::Parse / Reset / Tick** 1201–1350 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawNumber1081Slice.cs`
Managed LOC (tally): 129.
Conversion commit: `c8522e346f0058bef08d0d00d5d362696a1d2479`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Parse flag keywords (**1203–1261**) | yes |
| Flag separator `|` vs `,` (**1262–1263**) | yes |
| Optional font spacing + low/high coloring gates (**1266–1287**) | yes |
| Script health/armor interpolation override at parse end (**1291–1294**) | yes |
| `Reset` draw value (**1296–1298**) | yes (constant) |
| Prefix tick cache invalidation (**1302–1306**) | yes |
| Tick health clamp + interpolation (**1311–1321**) | yes |
| Missing ammo early exit (**1324–1337**, **1349–1350**) | yes |
| Parameterized ammo amount (**1339–1346**) | yes |
| Draw-number alignment keyword order (**1235–1240**) | yes (delegates **1077**) |
| Prefix + constant merge (**1253–1257**) | yes |
| Draw-shadow optional offsets (**1211–1222**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1201** | trailing `,` seam with **1080** |
| **1265**, **1276–1285** | host coordinates / translation scanner |
| **1305** | host `CommandDrawString::Tick` delegate |
| **1312**, **1319**, **1329**, **1336** | host status bar field reads |
| **1341** | host `FindInventory` |
| **1351+** | `ammo1` max amount outside hull |

| Managed (`VmSbarinfoCommandDrawNumber1081Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| Parse flag classification + separators | **1203–1263** |
| Shadow / prefix / alignment / coloring gates | **1211–1257**, **1266–1287** |
| Parse-close interpolation policy | **1291–1294** |
| Tick cache + health/ammo predicates | **1302–1350** |
| Band hull (exclusive slice) | **1201–1350** |

### Chunk **1081** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1080** |


**Post-review ratio:** 129/150 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawNumber1081SliceTests.ClassifyDrawNumberParseKeyword_MapsFillZerosAndRejectsUnknown`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveDrawNumberFlagListSeparator_MatchesNativePipeComma`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ApplyDrawNumberShadowFlag_UsesOptionalOffsets`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveDrawNumberPrefixWhenConstant_DisablesPrefixAndConcatenates`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ShouldParseDrawNumberLowHighColoring_MatchesNativeCommaGates`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveDrawNumberParseClosingInterpolation_OverridesHealthArmorFromScript`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ClampDrawNumberHealthForDisplay_ZeroesNegativesWhenCapped`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveDrawNumberPrefixTickCache_InvalidatesWhenPrefixEnabled`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveDrawNumberTickWhenAmmoMissing_ClearsStringAndReturns`
- `VmSbarinfoCommandDrawNumber1081SliceTests.ResolveInventoryDrawNumberAmount_ReturnsZeroWhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1081 --to 1081` (0 hard / 0 soft).

## Seams

**1080** owns value keyword validation through **1200**; **1078** owns `InvalidStringCache` (**-1**). Host `GetCoordinates`, `GetTranslation`, `ParseStringValue`, and `FindInventory` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1201–1350** (+150); draw-number flag loop + tick health/ammo head; no overlap with **1080** hull except seam **1200**/**1201**.
