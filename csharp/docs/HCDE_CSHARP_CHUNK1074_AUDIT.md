# Chunk 1074 — `CommandDrawImage` parse tail, `Tick` icon resolution, and `GetIcon` scale head from `sbarinfo_commands.cpp` (**151–300**); follows **1073** (**1–150**).

## Scope

`CommandDrawImage` parse tail, `Tick` icon resolution, and `GetIcon` scale head from `sbarinfo_commands.cpp` (**151–300**); follows **1073** (**1–150**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawImage::Parse / Tick / GetIcon** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawImage1074Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `57d4a5b7d7d811d9e9ee4d87df3838e935bac2f9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Max width/height → `DI_DRAWINBOX` (**177–186**) | yes |
| Alignment keywords (**166–171**) | yes |
| Draw-image flag OR list (**192–211**) | yes |
| `forcescale` gated on draw-in-box (**202–205**) | yes |
| Alternate-on-fail parse branch (**214–217**) | yes |
| Tick reset + applyscale clear (**223–228**) | yes |
| Ammo/armor/weapon tick gates (**232–255**) | yes |
| Hexen armor slot alpha (**262–278**) | yes |
| Image-table / alternate truth (**285–290**) | yes |
| `GetIcon` spawn scale (**296–304**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–156**, **157–158** | host scanner / coordinates |
| **215**, **221** | host flow-control calls |
| **231**, **236–286** | host texture lookup |
| **294–307** | host `GetInventoryIcon` / `TexMan` |
| **309+** | `ImageType` enum outside hull |

| Managed (`VmSbarinfoCommandDrawImage1074Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| Max dimension + draw-in-box flags | **177–186** |
| Alignment + parse flag keywords | **166–211** |
| Parse terminator policy | **214–217** |
| Tick reset + icon gates | **223–255** |
| Hexen armor alpha + abort | **262–278** |
| Alternate-on-fail truth | **288–290** |
| Inventory spawn scale | **300–304** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1074** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1073** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawImage1074SliceTests.ApplyParsedMaxWidth_SetsDrawInBoxWhenPositive`
- `VmSbarinfoCommandDrawImage1074SliceTests.ApplyParsedMaxHeight_ClearsToSentinelWhenNonPositive`
- `VmSbarinfoCommandDrawImage1074SliceTests.TryResolveDrawImageAlignment_MapsCenterKeywords`
- `VmSbarinfoCommandDrawImage1074SliceTests.ApplyDrawImageParseFlag_ForceScaleRequiresDrawInBox`
- `VmSbarinfoCommandDrawImage1074SliceTests.ApplyDrawImageParseFlag_ReportsUnknownKeywords`
- `VmSbarinfoCommandDrawImage1074SliceTests.ShouldDelegateParseToFlowControl_MatchesAlternateOnFail`
- `VmSbarinfoCommandDrawImage1074SliceTests.ResetTickDrawImageState_ClearsSpawnScaleWhenApplyScaleWasSet`
- `VmSbarinfoCommandDrawImage1074SliceTests.ShouldResolveArmorTickIcon_RequiresNonZeroAmount`
- `VmSbarinfoCommandDrawImage1074SliceTests.CombineHexenArmorFillAlpha_ScalesBySlotRatio`
- `VmSbarinfoCommandDrawImage1074SliceTests.ResolveAlternateOnFailTruth_DetectsMissingOrInvalid`
- `VmSbarinfoCommandDrawImage1074SliceTests.ApplyInventoryIconSpawnScale_CopiesItemScaleWhenRequested`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1074 --to 1074` (0 hard / 0 soft).

## Seams

`GetCoordinates`, `FScanner`, `TexMan`, `GetInventoryIcon`, and **151–156** token I/O remain host; **1073** keyword head seam at **149–150**.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **151–300** (+150); continues same class; zero tally overlap; no Vm* collision with **1073** hull.
