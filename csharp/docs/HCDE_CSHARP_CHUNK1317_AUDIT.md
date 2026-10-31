# Chunk 1317 — Visplane hash, the fake-plane keep, and the height redraw from r_visibleplanelist.cpp (1-392).

## Scope

Visplane hash, the fake-plane keep, and the height redraw from r_visibleplanelist.cpp (1-392).

## Native ranges

- `src/rendering/swrenderer/plane/r_visibleplanelist.cpp` **VisiblePlaneList::FindPlane** 1–392 (392 lines)

**Credited native LOC:** 392 (tally `nativeLoc` 392).

## Managed

`VmVisiblePlanes1317Slice.cs`
Managed LOC (tally): 521.
Conversion commit: `6f103e3fa8a68dc51726b18889131e239eed3af3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Add, clear, and the fake-plane walk (**54–97**) | yes |
| FindPlane hash and the skybox match (**99–228**) | yes |
| Range split and portal pop (**230–328**) | yes |
| Render and the height redraw (**330–391**) | yes |
| Band hull (**1–392**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The plane pixel drawer | the host records the draw arguments |

Sky flats force light to zero, a unit transform, and a flat plane whose C is the input C. The hash still uses the original plane distance. A portal that is not the current skybox occupies slot 128. Fake floors store the alpha in the sign bit of sky and opaque alpha, then clear the portal pointer. A non-sky match compares the camera position with the stacked view position stored on the plane. An empty column span unions the range. A touched span copies the plane, including the light pointer, and leaves the old edges. Clear keeps a negative sky and does not walk slot 128. Render draws only a non-negative sky in the matching portal and skybox. RenderHeight draws a negative sky whose origin height matches, replaces the portal mirror flags, and restores the camera position and yaw only.

| Managed (`VmVisiblePlanes1317Slice.cs`) | Native (`r_visibleplanelist.cpp`) |
|------------------------------------------|-------------------------------------|
| Visplane hash and the height redraw | **1–392** |
| Band hull (exclusive slice) | **1–392** |

### Chunk **1317** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **392** |
| **Net headline** | **+392** vs post-**1316** |


**Post-review ratio:** 521/392 managed/native ≈ 133% (src file LOC at HEAD; see tally).

## Tests

- `VmVisiblePlanes1317SliceTests.Find_HashesTheOriginalDistanceAndRewritesSkyFlats`
- `VmVisiblePlanes1317SliceTests.Range_KeepsFakesAndRestoresTheCameraAfterHeightRedraw`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1317 --to 1317` (0 hard / 0 soft).

## Seams

Plane draw, skybox membership, and the viewpoint are host state. The hash lives in the list header and is what FindPlane calls.

## Credit rationale

UNCredited r_visibleplanelist.cpp 1-392 (+392). A full tally line scan shows no overlap. The file ends at line 392.
