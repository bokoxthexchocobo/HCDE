# Chunk 1128 — GetPosition, activateNewScale, save/restore scale, addMark, and findMinMaxBoundaries seeds from `am_map.cpp` (**1051–1200**); follows **1127**.

## Scope

GetPosition, activateNewScale, save/restore scale, addMark, and findMinMaxBoundaries seeds from `am_map.cpp` (**1051–1200**); follows **1127**.

## Native ranges

- `src/am_map.cpp` **Automap scale save and marks** 1051–1200 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapScaleMark1128Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `1f08ed7a29a405c9bec1fc0fce85dcc914594d82`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetPosition` window center (**1100–1103**) | yes |
| `activateNewScale` (**1111–1121**) | yes |
| `saveScaleAndLoc` / `restoreScaleAndLoc` (**1129–1163**) | yes |
| `addMark` font gate and wrap (**1171–1184**) | yes |
| `findMinMaxBoundaries` seeds and first X test (**1193–1200**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1051–1098** | method declarations and section comments |
| **1201+** | vertex min/max assignments outside hull |

`DEFAULT_FONT_NAME` compare is case-insensitive. `FIXED_MIN` is signed **0x80000000**. Follow-player restore uses the host camera.

| Managed (`VmAmMapScaleMark1128Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Scale, marks, bound seeds | **1051–1200** |
| Band hull (exclusive slice) | **1051–1200** |

### Chunk **1128** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1127** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapScaleMark1128SliceTests.GetPosition_IsWindowCenter`
- `VmAmMapScaleMark1128SliceTests.ActivateNewScale_RecentersOnFrameToMapSize`
- `VmAmMapScaleMark1128SliceTests.RestoreScaleAndLoc_FollowsCameraOrSavedOrigin`
- `VmAmMapScaleMark1128SliceTests.TryAddMark_WrapsAndRejectsMissingDefaultDigits`
- `VmAmMapScaleMark1128SliceTests.SeedVertexBounds_UsesFloatMaxAndFixedMin`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1128 --to 1128` (0 hard / 0 soft).

## Seams

Camera position and mark-font texture validity stay host-injected. The vertex walk assignment begins at **1201**.

## Credit rationale

UNCredited **`am_map.cpp`** **1051–1200** (+150), inside the 3594-line file, with no conversion or recognition overlap.
