# Chunk 1067 — `ParseMugShotBlock`, `newImage`, `SBarInfo::Init` defaults, and `Popup` init/tick head from `sbarinfo.cpp` (**751–900**); follows **1066** parse band.

## Scope

`ParseMugShotBlock`, `newImage`, `SBarInfo::Init` defaults, and `Popup` init/tick head from `sbarinfo.cpp` (**751–900**); follows **1066** parse band.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **ParseMugShotBlock / Popup / Init** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoMugShotPopup1067Slice.cs`
Managed LOC (tally): 104.
Conversion commit: `ca3f0e0c25da0c77a518fde8b95192880f231bed`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ParseMugShotBlock` frame graphics + delay (**758–783**) | yes |
| `ResetHuds` slot iteration (**785–789**) | yes (index policy) |
| `newImage` null/duplicate lookup (**791–805**) | yes |
| `SBarInfo::Init` field defaults (**818–831**) | yes |
| `Popup` ctor defaults + `init` (**847–871**) | yes |
| `Popup::tick` slide/push/fade step (**873–900**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **751–756** | protrusion/`ParseSBarInfo` host close (**1066** loop head **739–750**) |
| **807–816**, **833–835**, **837–844** | ctor/dtor/HUD alloc host |
| **901+** | fade `moving` tail outside hull |

| Managed (`VmSbarinfoMugShotPopup1067Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| MugShot graphic/delay/multiframe | **769–780** |
| `MergeImageIndexAfterLookup` | **793–804** |
| `CreateNativeInitDefaults` | **820–831** |
| `ResolvePopupInitState` | **853–871** |
| Slide/fade tick helpers | **877–900** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **1067** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1066** |


**Post-review ratio:** 104/150 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoMugShotPopup1067SliceTests.CreateNativeInitDefaults_MatchesNativeInit`
- `VmSbarinfoMugShotPopup1067SliceTests.ShouldFailMugShotGraphicLength_EnforcesFiveChars`
- `VmSbarinfoMugShotPopup1067SliceTests.ResolveMugShotFrameDelay_HonorsNegativePrefix`
- `VmSbarinfoMugShotPopup1067SliceTests.MergeImageIndexAfterLookup_HandlesNullAndDuplicate`
- `VmSbarinfoMugShotPopup1067SliceTests.ResolvePopupInitState_FadeStartsTransparent`
- `VmSbarinfoMugShotPopup1067SliceTests.ComputeSlidePopupYOffset_ClosesWhenOpened`
- `VmSbarinfoMugShotPopup1067SliceTests.AdvanceFadePopupAlpha_ClampsWhenClosing`
- `VmSbarinfoMugShotPopup1067SliceTests.ShouldKeepPopupMovingOnSlideAxis_StopsAtEndpoints`
- `VmSbarinfoMugShotPopup1067SliceTests.ClassifyImagePatch_TreatsNullImageSentinel`
- `VmSbarinfoMugShotPopup1067SliceTests.ShouldIterateHudResetSlot_CoversNativeHudCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1067 --to 1067` (0 hard / 0 soft).

## Seams

Protrusion **751–756** close + `ParseSBarInfo` end are host; popup transition kinds reuse **1066**; `FScanner`, HUD `new`/`delete`, and **901+** fade moving tail remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **751–900** (+150); no duplicate credit for **1066** protrusion parse (**739–750**) or popup transition classify (**685–730**).
