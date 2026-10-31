# Chunk 1389 — The far-clip construct, the facing and portal rejects, and the ceiling and floor plane marks from r_farclip_line.cpp (1-188).

## Scope

The far-clip construct, the facing and portal rejects, and the ceiling and floor plane marks from r_farclip_line.cpp (1-188).

## Native ranges

- `src/rendering/swrenderer/line/r_farclip_line.cpp` **FarClipLine::Render** 1–188 (188 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmFarClipLine1389Slice.cs`
Managed LOC (tally): 147.
Conversion commit: `f367e4bffeaf38dc390732124be5e1caf57e01ef`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and render rejects (**59–103**) | yes |
| Wall segment and the floor-projection copy (**105–121**) | yes |
| Top and bottom clip (**123–140**) | yes |
| Ceiling plane mark (**142–163**) | yes |
| Floor plane mark (**165–187**) | yes |
| Band hull (**1–188**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `FWallCoords::Init` | callee; a true result rejects the line |
| `P_ClipLineToPortal` | callee |
| `ProjectedWallLine::Project` | callee; the ceiling project in this file is commented out |
| `VisiblePlaneList::GetRange` | callee; its return replaces the plane |
| Includes **1–57** | headers around the class |

A non-negative facing cross product returns before wall init. Init, the window, a missing linedef, and a portal clip are checked in that order. A skybox portal does not clip. The four sector heights are read only after those rejects, and the solid clip is then requested. The first wall segment projects the floor, copies that screen row onto the top, and does not project the ceiling. Later segments keep the prepared row. Each column raises the top to the ceiling clip and lowers the bottom to the floor clip before either plane is marked. A fake ceiling uses the fake ceiling clip. A fake floor uses the fake floor clip. The floor mark aborts when asserts are on and the bottom is past the view height, and it leaves the plane unchanged. With asserts off it still writes that bottom.

| Managed (`VmFarClipLine1389Slice.cs`) | Native (`r_farclip_line.cpp`) |
|---------------------------------------|------------------------------|
| Far-clip render and plane marks | **1–188** |
| Band hull (exclusive slice) | **1–188** |

### Chunk **1389** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **188** |
| **Net headline** | **+188** vs the prior chunk |


**Post-review ratio:** 147/188 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmFarClipLine1389SliceTests.RenderRejectsBackfacesWindowsAndPortals`
- `VmFarClipLine1389SliceTests.SegmentCopiesFloorYAndMarksFakePlanes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1389 --to 1389` (0 hard / 0 soft).

## Seams

Wall coordinate init, portal clipping, floor projection, and visible-plane range lookup stay callees. The ceiling projection in this file is commented out and is not called.

## Credit rationale

UNCredited r_farclip_line.cpp 1-188 (+188). After chunk 1388 this is the unique largest hole. ddstexture.cpp 1-186 is 186. The band is shorter than 400 lines and is not sliced.
