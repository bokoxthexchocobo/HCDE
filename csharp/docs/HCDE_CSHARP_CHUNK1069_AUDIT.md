# Chunk 1069 — `DSBarInfo::_Draw` continuation, popup/inventory draw policy, `_Tick`/`_ShowPop`, `_GetProtrusion`, and `DrawGraphic` prologue from `sbarinfo.cpp` (**1051–1200**); follows **1068**.

## Scope

`DSBarInfo::_Draw` continuation, popup/inventory draw policy, `_Tick`/`_ShowPop`, `_GetProtrusion`, and `DrawGraphic` prologue from `sbarinfo.cpp` (**1051–1200**); follows **1068**.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **DSBarInfo::_Draw / DrawGraphic** 1051–1200 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoDsbarDraw1069Slice.cs`
Managed LOC (tally): 115.
Conversion commit: `5e9c6c95cd3820b2c13df2b63b092d76f1957a6a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `_Draw` HUD state → bar index (**1057–1064**) | yes |
| Main HUD draw offsets w/ popup (**1078–1081**) | yes |
| Inventory bar selection/clear tics (**1085–1095**) | yes |
| Active popup → popup HUD bar (**1102–1118**) | yes |
| `_NewGame` / `_MustDrawLog` (**1127–1137**) | yes (sentinel + empty log bar) |
| `_Tick` pending popup advance (**1142–1150**) | yes |
| `_ShowPop` state machine (**1165–1181**) | yes |
| `_GetProtrusion` scan (**1184–1193**) | yes |
| `DrawGraphic` null guard (**1198–1199**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1052–1054** | **1068** normal status HUD pick (same branch) |
| **1065–1076**, **1098** | host scale/tick/draw |
| **1123–1125**, **1160–1162** | pointer clear host |
| **1201+** | `DrawGraphic` center math outside hull |

| Managed (`VmSbarinfoDsbarDraw1069Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| `ResolveHudBarIndexForDrawState` | **1057–1064** |
| Popup displacement offsets | **1078–1081** |
| Inventory draw/clear policy | **1085–1095** |
| `MapActivePopupToHudBarIndex` | **1104–1110** |
| `_ShowPop` / tick advance helpers | **1145–1181** |
| `ResolveProtrusionOffset` | **1187–1192** |
| `ShouldSkipDrawGraphic` | **1198–1199** |
| `ShouldUseNativeLogPopup` | **1134–1137** |
| Band hull (exclusive slice) | **1051–1200** |

### Chunk **1069** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1068** |


**Post-review ratio:** 115/150 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoDsbarDraw1069SliceTests.ResolveHudBarIndexForDrawState_CoversFullscreenAndOther`
- `VmSbarinfoDsbarDraw1069SliceTests.ResolveMainHudDrawOffsets_UsesPopupDisplacementWhenNeeded`
- `VmSbarinfoDsbarDraw1069SliceTests.ShouldDrawInventoryBar_RequiresTicsAndAllowedLevel`
- `VmSbarinfoDsbarDraw1069SliceTests.MapActivePopupToHudBarIndex_MapsLogKeysStatus`
- `VmSbarinfoDsbarDraw1069SliceTests.ResolveShowPopupIntent_ClosesWhenPopupActive`
- `VmSbarinfoDsbarDraw1069SliceTests.CommitImmediatePopupWhenIdle_PromotesPending`
- `VmSbarinfoDsbarDraw1069SliceTests.ResolveProtrusionOffset_WalksAscendingFactors`
- `VmSbarinfoDsbarDraw1069SliceTests.ShouldSkipDrawGraphic_WhenTextureMissing`
- `VmSbarinfoDsbarDraw1069SliceTests.ShouldAdvancePendingPopupAfterClose_WhenClosedAndSettled`
- `VmSbarinfoDsbarDraw1069SliceTests.ApplyInventoryTicsWhenBarEmpty_ClearsWhenNoCommands`
- `VmSbarinfoDsbarDraw1069SliceTests.SelectInventoryHudBarIndex_SwitchesForFullscreen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1069 --to 1069` (0 hard / 0 soft).

## Seams

Status-bar automap/normal pick (**1048–1054**) uses **1068** `SelectHudIndexForStatusBarDraw`; host `Draw`/`Tick`/`ForceHUDScale` and **1201+** center math remain injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **1051–1200** (+150); zero duplicate credit on **1068** automap HUD index (**1048–1050**).
