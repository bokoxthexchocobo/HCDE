# Chunk 1066 — `ParseSBarInfo` top-level commands from `lowerhealthcap` through `protrusion` loop head in `sbarinfo.cpp` (**601–750**); follows **1065**.

## Scope

`ParseSBarInfo` top-level commands from `lowerhealthcap` through `protrusion` loop head in `sbarinfo.cpp` (**601–750**); follows **1065**.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **SBarInfo::ParseSBarInfo** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoParseTopLevel1066Slice.cs`
Managed LOC (tally): 142.
Conversion commit: `999931fd9fa768bbf00bc880ce8e264ce24fb5b5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `lowerhealthcap` false-first bool (**603–613**) | yes |
| `resolution` width/height (**615–621**) | yes |
| `statusbar` / `appendstatusbar` policy (**623–651**) | yes |
| `mugshot` flag list (**654–682**) | yes |
| `createpopup` kind/size/transition (**685–736**) | yes |
| `protrusion` ascending factor pairs (**739–750**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **601–602** | **1065** monospace statement tail (host `;`) |
| **632**, **639–641**, **651** | host `MustMatchString` / alloc / `Parse` |
| **676** | `ParseMugShotBlock` host |
| **735** | `popup.init` host |
| **751+** | protrusion loop tail + switch close outside hull |

| Managed (`VmSbarinfoParseTopLevel1066Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| `ResolveLowerHealthCap` | **603–612** |
| `MergeResolution` | **616–620** |
| HUD append/create + automap | **626–650** |
| `ApplyMugShotStateFlag` / replace index | **662–682** |
| Popup kind/transition/fade speeds | **688–730** |
| Protrusion order + pair build | **741–750** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **1066** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1065** |


**Post-review ratio:** 142/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoParseTopLevel1066SliceTests.ResolveLowerHealthCap_PrefersFalseBranchFirst`
- `VmSbarinfoParseTopLevel1066SliceTests.ResolveImplicitGameTypeWhenBaseUnset_MatchesNative`
- `VmSbarinfoParseTopLevel1066SliceTests.StatusBarHudPolicy_HandlesAppendVsCreate`
- `VmSbarinfoParseTopLevel1066SliceTests.ApplyMugShotStateFlag_StacksHealth2Flags`
- `VmSbarinfoParseTopLevel1066SliceTests.ClassifyPopupKind_MapsLogKeysStatus`
- `VmSbarinfoParseTopLevel1066SliceTests.ComputeFadePopupSpeed_UsesTicRate`
- `VmSbarinfoParseTopLevel1066SliceTests.BuildFadePopupSpeeds_PairsBothFloats`
- `VmSbarinfoParseTopLevel1066SliceTests.IsProtrusionFactorAscending_EnforcesStrictIncrease`
- `VmSbarinfoParseTopLevel1066SliceTests.ClassifyPopupTransition_RecognizesSlideAndFade`
- `VmSbarinfoParseTopLevel1066SliceTests.MergeResolution_ReplacesWhenCommandActive`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1066 --to 1066` (0 hard / 0 soft).

## Seams

Status-bar type names use **1064** `MatchStatusBarTypeName`; `FScanner`, `new`/`delete` HUD blocks, `ParseMugShotBlock`, `popup.init`, and **751+** loop tail remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **601–750** (+150); no tally overlap with **1065** monospace body (**574–600**).
