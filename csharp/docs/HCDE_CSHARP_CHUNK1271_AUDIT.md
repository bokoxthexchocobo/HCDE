# Chunk 1271 — Portal sprite columns, the masked midtexture pass, and the 3D-floor height walk from r_translucent_pass.cpp (1-229).

## Scope

Portal sprite columns, the masked midtexture pass, and the 3D-floor height walk from r_translucent_pass.cpp (1-229).

## Native ranges

- `src/rendering/swrenderer/scene/r_translucent_pass.cpp` **RenderTranslucentPass** 1–229 (229 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmTrans1271Slice.cs`
Managed LOC (tally): 264.
Conversion commit: `5b30abb24e23a3174f568e3041c08d13fe1152c7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RenderTranslucentPass (**56–59**) | yes |
| Deinit (**61–64**) | yes |
| Clear (**66–69**) | yes |
| CollectPortals (**71–102**) | yes |
| ClipSpriteColumnWithPortals (**104–130**) | yes |
| DrawMaskedSingle (**132–167**) | yes |
| Render (**169–228**) | yes |
| Band hull (**1–229**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Sprite sort | host list |
| Segment groups | host list |
| Voxel shutdown | host renderer |

A missing thread faults. Clearing drops the sprites. Collecting keeps a mirror line and a visible portal on the front side, and it drops a missing line, a missing segment, the back side, and a portal that is not visible. A skybox column is not clipped. A column clips only when the sprite is strictly behind the line and the x coordinate is inside the half-open span. A cross product equal to the epsilon stays in front. Sprites draw from the newest entry and only for the current portal. Masked segments draw in list order when they have a translucent midtexture, a 3D-floor wall, or a fog boundary. A renew copies the backup afterward. With no height list the pass runs once and does not delete heights. Ceilings start at the current height and walk toward the previous node while the height is at or above the view, then the plane is drawn. The upward preamble clips to the top height. Floors then walk toward the next node only while the height is below the view, and the plane is drawn before the sprites. A height equal to the view is a ceiling. Deleting the height list happens after that walk. The profiler ticks only on the main thread, and only when it is active.

| Managed (`VmTrans1271Slice.cs`) | Native (`r_translucent_pass.cpp`) |
|----------------------------------|----------------------------------|
| Columns, masked pass, and heights | **1–229** |
| Band hull (exclusive slice) | **1–229** |

### Chunk **1271** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **229** |
| **Net headline** | **+229** vs post-**1270** |


**Post-review ratio:** 264/229 managed/native ≈ 115% (src file LOC at HEAD; see tally).

## Tests

- `VmTrans1271SliceTests.Columns_SkipTheFrontAndAMissingPortal`
- `VmTrans1271SliceTests.Floors_DrawCeilingsBeforeTheUpwardPass`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1271 --to 1271` (0 hard / 0 soft).

## Seams

The draw-segment order, the sprite list, and the height links are host inputs. The slice keeps the portal filter, the strict side test, the reverse sprite walk, and the ceiling-then-floor order.

## Credit rationale

UNCredited r_translucent_pass.cpp 1-229 (+229). A full tally line scan shows no overlap. The file ends at line 229.
