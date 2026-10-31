# Chunk 1131 — Responder tail, changeWindowScale, doFollowPlayer, Ticker, and clearFB background policy from `am_map.cpp` (**1501–1650**); follows **1130**.

## Scope

Responder tail, changeWindowScale, doFollowPlayer, Ticker, and clearFB background policy from `am_map.cpp` (**1501–1650**); follows **1130**.

## Native ranges

- `src/am_map.cpp` **Automap zoom follow and background** 1501–1650 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapZoomFollow1131Slice.cs`
Managed LOC (tally): 114.
Conversion commit: `570db24c88c8633f5d87889199dc4346be677c91`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Key-up release handoff and `return res` (**1503–1513**) | yes |
| `changeWindowScale` (**1523–1558**) | yes |
| `doFollowPlayer` (**1566–1594**) | yes |
| `Ticker` clock (**1603–1608**) | yes |
| `clearFB` background gate and tile origins (**1618–1650**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1492–1500** | follow `+am_pan` gate already credited by **1130** |
| **1572** | unused view-height delta |
| **1651+** | remainder of `clearFB` |

`M_OLDZOOMIN` is **1.02** and `M_OLDZOOMOUT` is **1/1.02**. `am_zoomdir` is cleared after the step. `am_drawmapback == 2` still draws Raven (`colorset` 3, or -1 with `GAME_Raven` **6**) when the mod set is undefined. A non-positive patch size is not tiled.

| Managed (`VmAmMapZoomFollow1131Slice.cs`) | Native (`am_map.cpp`) |
|-------------------------------------------|---------------------|
| Zoom, follow, ticker, background | **1501–1650** |
| Band hull (exclusive slice) | **1501–1650** |

### Chunk **1131** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1130** |


**Post-review ratio:** 114/150 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapZoomFollow1131SliceTests.ResponderResult_ReleasesBareKeysAndKeepsPlusBinds`
- `VmAmMapZoomFollow1131SliceTests.ApplyZoom_UsesLegacyDirectionThenClearsIt`
- `VmAmMapZoomFollow1131SliceTests.FollowPlayer_ScrollsWhenTheCameraMoves`
- `VmAmMapZoomFollow1131SliceTests.ShouldDrawMapBackground_RavenModeTwoStillBlits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1131 --to 1131` (0 hard / 0 soft).

## Seams

`C_DoKey` and texture draw calls stay host-injected. `clearFB` continues after the blit loops at **1651**.

## Credit rationale

UNCredited **`am_map.cpp`** **1501–1650** (+150), inside the 3594-line file, with no conversion or recognition overlap.
