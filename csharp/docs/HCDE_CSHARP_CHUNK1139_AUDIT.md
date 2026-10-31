# Chunk 1139 — Wall color chain, automap rotation, and glyph scale from `am_map.cpp` (**2701–2850**); follows **1138**.

## Scope

Wall color chain, automap rotation, and glyph scale from `am_map.cpp` (**2701–2850**); follows **1138**.

## Native ranges

- `src/am_map.cpp` **Automap wall colors** 2701–2850 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapWallColor1139Slice.cs`
Managed LOC (tally): 159.
Conversion commit: `a2391498f7f2474fed13db0c7629163672128c91`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Secret, teleport, exit, lock, trigger colors (**2701–2747**) | yes |
| One-sided, plane, 3D-floor, two-sided colors (**2748–2769**) | yes |
| Unseen `NotSeenColor` and its dontdraw skip (**2771–2781**) | yes |
| `rotate` / `rotatePoint` (**2794–2831**) | yes |
| Glyph endpoint scale (**2839–2850**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2551–2700** | trigger, lock, and portal pass already credited by **1138** |
| **2851+** | glyph rotation and the second endpoint |

`ML_SECRET` is **0x20** and `ML_REVEALED` is **0x20000000**. A non-negative lock map color is split with `RPART`/`GPART`/`BPART`. Color **0** is black, not the locked fallback. `am_showtriggerlines` is any non-zero value. Floor-plane changes win over ceiling changes. The rotation cache updates only when the angle changes, then applies `x' = x cos - y sin`, `y' = x sin + y cos` around the map center. A glyph scale of **0** leaves the point unchanged.

| Managed (`VmAmMapWallColor1139Slice.cs`) | Native (`am_map.cpp`) |
|-------------------------------------------|---------------------|
| Colors, rotation, glyph scale | **2701–2850** |
| Band hull (exclusive slice) | **2701–2850** |

### Chunk **1139** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1138** |


**Post-review ratio:** 159/150 managed/native ≈ 106% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapWallColor1139SliceTests.ChooseWall_FollowsTheSecretTeleportAndLockOrder`
- `VmAmMapWallColor1139SliceTests.ChooseWall_UsesTriggerThenPlaneChangesThenTheUnseenColor`
- `VmAmMapWallColor1139SliceTests.RotatePoint_SpinsAroundTheMapCenter`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1139 --to 1139` (0 hard / 0 soft).

## Seams

Palette slots, yaw sine, and the character's B endpoint stay host-supplied. Angle application on the glyph begins at **2854**.

## Credit rationale

UNCredited **`am_map.cpp`** **2701–2850** (+150), inside the 3594-line file, with no conversion or recognition overlap.
