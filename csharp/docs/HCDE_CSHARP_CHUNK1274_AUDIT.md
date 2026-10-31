# Chunk 1274 — Wall-sprite projection and the portal column clip from r_wallsprite.cpp (1-196).

## Scope

Wall-sprite projection and the portal column clip from r_wallsprite.cpp (1-196).

## Native ranges

- `src/rendering/swrenderer/things/r_wallsprite.cpp` **RenderWallSprite** 1–196 (196 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmWallSprite1274Slice.cs`
Managed LOC (tally): 209.
Conversion commit: `ac5f50600cd752bc3b4a92e7a08010888dc8eaf1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Project (**72–160**) | yes |
| Render (**162–195**) | yes |
| Band hull (**1–196**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Wall projection | host wall setup |
| Drawer pixels | host picture |

A missing picture faults before the view is read. A missing thread faults when the edges are placed. The yaw is increased by 90 degrees, then the scaled left offset and width are laid along that direction. A rejected projection returns. A span that starts at or past the right window, or ends at or before the left window, returns. The depth is the view-space dot of the position. The top and bottom use the scaled offsets, then both are stored as floats. The screen span is pulled in to the window, and a right edge that lands on the window is included. A bright actor sets the fullbright bit on the sprite copy and leaves the caller's flags alone. Fullbright is off when the view is foggy. A bright actor stays bright even when the texture disables it. A fullbright flag does not, once the texture disables it. A null texture neither disables nor forces it. The override cvar and a fullbright texture force it only together. An additive style with a nonzero fade swaps in a black-fade colormap. Overlay and source invert cancel. Fade-to-black is its own bit. The mirror assignment stays unused, and the fake-flat side stays centered with no fake planes. Render intersects the clamped span with the projected span and returns before the style when that is empty. A hidden style is asked and then returns before the column loop. A portal column copies the ceiling into the floor sample. The draw uses the stored top and the stored float scale, not a later position change.

| Managed (`VmWallSprite1274Slice.cs`) | Native (`r_wallsprite.cpp`) |
|--------------------------------------|-----------------------------|
| Projection, light, and column clip | **1–196** |
| Band hull (exclusive slice) | **1–196** |

### Chunk **1274** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **196** |
| **Net headline** | **+196** vs post-**1273** |


**Post-review ratio:** 209/196 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmWallSprite1274SliceTests.Project_TurnsTheYawAndClampsTheWindow`
- `VmWallSprite1274SliceTests.Columns_SkipAHiddenStyleAndCopyPortalCeilings`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1274 --to 1274` (0 hard / 0 soft).

## Seams

Wall projection and the drawer are host inputs. The slice keeps the 90-degree edge turn, the window clamp, the fullbright formula, and the portal floor swap.

## Credit rationale

UNCredited r_wallsprite.cpp 1-196 (+196). A full tally line scan shows no overlap. The file ends at line 196.
