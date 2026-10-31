# Chunk 1135 — Subsector projection, textured floor and 3D-floor choice, unseen tint, and sector-fill fallback from `am_map.cpp` (**2101–2250**); follows **1134**.

## Scope

Subsector projection, textured floor and 3D-floor choice, unseen tint, and sector-fill fallback from `am_map.cpp` (**2101–2250**); follows **1134**.

## Native ranges

- `src/am_map.cpp` **Automap subsector fill** 2101–2250 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapSubsectorFill1135Slice.cs`
Managed LOC (tally): 125.
Conversion commit: `b8131d7a39fc8a1eabd37f55d186c5bd642b4eab`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Vertex projection (**2104–2116**) | yes |
| Textured alpha, sample point, scales (**2121–2168**) | yes |
| 3D-floor walk and plane light (**2170–2202**) | yes |
| Sky skip, origin rotation, unseen tint, fade table (**2203–2238**) | yes |
| Sector-fill white texture and alpha (**2239–2250**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2100–2102** | portal skip already credited by **1134** |
| **2251–2253** | `originpt` zeros that finish the same else-if |
| **2255+** | polygon triangulation and shade |

`FF_EXISTS` is **1**, `FF_RENDERPLANES` **8**, `FF_FOG` **0x1000**, `FF_INVERTPLANES` **0x2000**, `FF_THISINSIDE` **0x40000000**. `LEVEL_HASFADETABLE` is **8**. Invert selects floor (**0**); otherwise ceiling (**1**). A rover below the sector floor stops the walk. Unseen tint runs only when the subsector is not drawn and `am_cheat` is below **4**.

| Managed (`VmAmMapSubsectorFill1135Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------------|---------------------|
| Projection, rovers, fill | **2101–2250** |
| Band hull (exclusive slice) | **2101–2250** |

### Chunk **1135** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1134** |


**Post-review ratio:** 125/150 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapSubsectorFill1135SliceTests.Project_MapsWorldToFrameAndSamplesTheCameraSector`
- `VmAmMapSubsectorFill1135SliceTests.SelectRover_TakesTheHighestFloorBelowTheCamera`
- `VmAmMapSubsectorFill1135SliceTests.TintUnseen_AveragesTowardTheCheatPalette`
- `VmAmMapSubsectorFill1135SliceTests.SectorFill_UsesWhiteScaleAndTheSuppliedAlpha`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1135 --to 1135` (0 hard / 0 soft).

## Seams

Texture lookup, plane Z, and yaw rotation stay host-supplied. Polygon draw begins at **2255**.

## Credit rationale

UNCredited **`am_map.cpp`** **2101–2250** (+150), inside the 3594-line file, with no conversion or recognition overlap.
