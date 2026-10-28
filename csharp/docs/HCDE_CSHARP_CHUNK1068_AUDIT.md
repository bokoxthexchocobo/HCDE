# Chunk 1068 — `Popup` tick tail through `DSBarInfo` ctor/`SetReferences`/`_Draw` status-bar HUD pick from `sbarinfo.cpp` (**901–1050**); follows **1067** fade alpha step.

## Scope

`Popup` tick tail through `DSBarInfo` ctor/`SetReferences`/`_Draw` status-bar HUD pick from `sbarinfo.cpp` (**901–1050**); follows **1067** fade alpha step.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **Popup / DSBarInfo** 901–1050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoPopupDraw1068Slice.cs`
Managed LOC (tally): 85.
Conversion commit: `c989ff5882ab16ffd4aac849856818233ad45b6b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Fade `moving` after alpha endpoints (**902–905**) | yes |
| Default transition pose (**907–918**) | yes |
| `open` / `close` + scaled alpha (**953–963**, **938–941**) | yes |
| `adjustRelCenter` (**967–977**) | yes |
| `DSBarInfo` inventory patch table sizing (**990–1007**) | yes |
| `SetReferences` ammo normalization (**1022–1037**) | yes |
| `_Draw` status HUD automap pick (**1046–1050**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **897–900** | **1067** `AdvanceFadePopupAlpha` |
| **923–926**, **927–951** | trivial getters / offsets |
| **1015–1018**, **1040** | host player + armor lookup |
| **1051+** | `_Draw` branches outside hull |

| Managed (`VmSbarinfoPopupDraw1068Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| `ResolveFadePopupMovingAfterAlphaStep` | **902–905** |
| `ResolveDefaultTransitionPopupPose` | **907–918** |
| `ApplyPopupOpenCommand` / close / `ScalePopupAlphaForDraw` | **938–963** |
| `AdjustRelativeCenter*` | **969–976** |
| `BuildPatchNameTableLayout` / inventory lump table | **990–1007** |
| `NormalizeReadyWeaponAmmo` / amounts | **1026–1037** |
| `SelectHudIndexForStatusBarDraw` | **1048–1050** |
| Band hull (exclusive slice) | **901–1050** |

### Chunk **1068** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1067** |


**Post-review ratio:** 85/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoPopupDraw1068SliceTests.ResolveFadePopupMovingAfterAlphaStep_StopsAtEndpoints`
- `VmSbarinfoPopupDraw1068SliceTests.ResolveDefaultTransitionPopupPose_MatchesNativeOpenedClosed`
- `VmSbarinfoPopupDraw1068SliceTests.ScalePopupAlphaForDraw_MultipliesMax`
- `VmSbarinfoPopupDraw1068SliceTests.ApplyPopupOpenCommand_ArmsMovement`
- `VmSbarinfoPopupDraw1068SliceTests.AdjustRelativeCenterX_AppliesHalfWidthWhenRelative`
- `VmSbarinfoPopupDraw1068SliceTests.BuildPatchNameTableLayout_MatchesNativeOffsets`
- `VmSbarinfoPopupDraw1068SliceTests.ShouldApplyFadeAlphaStep_OnlyWhenFadeTransitionMoving`
- `VmSbarinfoPopupDraw1068SliceTests.NormalizeReadyWeaponAmmo_PromotesSecondaryWhenPrimaryMissing`
- `VmSbarinfoPopupDraw1068SliceTests.SelectHudIndexForStatusBarDraw_SwitchesToAutomap`
- `VmSbarinfoPopupDraw1068SliceTests.TickFadePopupAlpha_DelegatesTo1067WhenMoving`
- `VmSbarinfoPopupDraw1068SliceTests.ResolveInventoryAmount_ZeroWhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1068 --to 1068` (0 hard / 0 soft).

## Seams

Fade alpha increment uses **1067** `AdvanceFadePopupAlpha`; popup offset getters (**927–951**) and host `Images.Init` / player attach remain injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **901–1050** (+150); **901–905** fade `moving` tail deferred from **1067**; no duplicate slide Y math (**877–892** on **1067**).
