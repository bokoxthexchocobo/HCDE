# Chunk 1251 — Scale-mode table, crop, minimum UI size, and the vid_scale commands from r_videoscale.cpp (1-346), through end of file.

## Scope

Scale-mode table, crop, minimum UI size, and the vid_scale commands from r_videoscale.cpp (1-346), through end of file.

## Native ranges

- `src/common/rendering/r_videoscale.cpp` **vid_scale_customwidth** 1–346 (346 lines)

**Credited native LOC:** 346 (tally `nativeLoc` 346).

## Managed

`VmVideoScale1251Slice.cs`
Managed LOC (tally): 517.
Conversion commit: `9cc592f3cd3d2bf11153f05f8efb4d1fa6b79811`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Custom size and pixel aspect (**42–60**) | yes |
| Fill ratios (**70–120**) | yes |
| refresh_minimums (**122–148**) | yes |
| Scale table and bounds (**150–168**) | yes |
| Factor, mode, crop, and the video menu (**171–198**) | yes |
| Linear scale and scaled size (**200–260**) | yes |
| Show and the vid_scale commands (**262–346**) | yes |
| Band hull (**1–346**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| ActiveRatio body | other file |
| Screen object | host client size |

A zero-size fill whose cached inputs are still the initial zeros returns the initial result. A non-positive axis returns 1 and leaves that cache unchanged. The 1.2 fill multiplies by 1.2 once inside the helper and the mode-7 width multiplies by 1.2 again. Refresh runs from the width query. The first time the special-UI callback is absent, the minimum becomes 640 by 400. Height queries do not refresh. Crop updates width before height in the width query and height before width in the height query. A video-options menu returns the source size, aspect 1, and linear false. A scale factor other than 1 prints the current resolution. An invalid mode becomes 0. Custom width below 320 and height below 200 are raised. Linear does not mark the size dirty.

| Managed (`VmVideoScale1251Slice.cs`) | Native (`r_videoscale.cpp`) |
|--------------------------------------|------------------------------|
| Modes, crop, and vid_scale commands | **1–346** |
| Band hull (exclusive slice) | **1–346** |

### Chunk **1251** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **346** |
| **Net headline** | **+346** vs post-**1250** |


**Post-review ratio:** 517/346 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmVideoScale1251SliceTests.Scale_CachesTheFillRatioAndRaisesTheUiMinimum`
- `VmVideoScale1251SliceTests.Scale_ClampsTheCvarsAndAppliesTheConsoleCommands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1251 --to 1251` (0 hard / 0 soft).

## Seams

Screen size, the menu class name, and the special-UI callback are host inputs. ActiveRatio's forced-aspect table is the caller used by the crop path. Comparisons against 0.05, 0.2, 2, and 5 use the wider floating type.

## Credit rationale

UNCredited r_videoscale.cpp 1-346 (+346). A full tally line scan shows no overlap. The file ends at line 346.
