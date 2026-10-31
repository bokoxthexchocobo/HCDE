# Chunk 1307 — Fog, gamma, black point, and texture-filter cvars from hw_cvars.cpp (1-203).

## Scope

Fog, gamma, black point, and texture-filter cvars from hw_cvars.cpp (1-203).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_cvars.cpp` **gl_fogmode** 1–203 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmHwCvars1307Slice.cs`
Managed LOC (tally): 196.
Conversion commit: `13ec7927448ba1fd3c7a859d092b7aa8d23a74c3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| gl_fogmode clamp (**32–36**) | yes |
| Portal, mirror, and seamless stores (**40–43**) | yes |
| Portal recursion clamp (**45–49**) | yes |
| Plane reflection mirror (**51–55**) | yes |
| Gamma clamp and fix conversion (**57–78**) | yes |
| vid_gamma sync (**83–99**) | yes |
| Named gamma cvar (**102–116**) | yes |
| vid_fixgamma sync (**118–131**) | yes |
| Contrast and saturation clamps (**133–143**) | yes |
| Black and white points (**145–175**) | yes |
| Saturation formula store (**179**) | yes |
| Texture filter callbacks (**186–195**) | yes |
| Precache store (**197**) | yes |
| Shadowmap filter clamp (**200–203**) | yes |
| Band hull (**1–203**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| screen SetTextureFilterMode | host records the call; this band decides when it runs |

Fog mode clamps to 0..2 and portal recursion clamps to 0..16. The plane-reflection callback copies the bool into the header flag. Gamma clamps to 0.1..3.0. While gamma_syncing is clear, vid_gamma normalizes itself, writes the named gamma cvar, and writes vid_fixgamma. The named gamma cvar writes vid_gamma and vid_fixgamma. vid_fixgamma clamps to the low fix value..1 and writes vid_gamma only. Assigning the same value still runs the callback. The sync flag stops that reentry from looping, and the flag is clear when the outer callback returns. Contrast clamps to 0..5 and saturation clamps to -3..3. Black point squares the clamped 0..1 value and stores the min of the white-point buffer, that square, and 0.8. White point cubes the shifted value and stores the max of the black-point buffer, that shape, and 0.2. The gap subtracted and added around the internals is the double 0.2, then narrowed to float. Anisotropic filtering stores any float and asks for a texture-filter update. The texture filter outside 0..6 becomes 6 and asks twice, because the assignment reenters before the outer call asks. The shadowmap filter outside 0..8 becomes 0 and does not ask. Precache and the saturation formula are plain stores.

| Managed (`VmHwCvars1307Slice.cs`) | Native (`hw_cvars.cpp`) |
|-----------------------------------|-------------------------|
| Fog, gamma, points, and filters | **1–203** |
| Band hull (exclusive slice) | **1–203** |

### Chunk **1307** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **203** |
| **Net headline** | **+203** vs post-**1306** |


**Post-review ratio:** 196/203 managed/native ≈ 97% (src file LOC at HEAD; see tally).

## Tests

- `VmHwCvars1307SliceTests.Gamma_ClampsAndResyncsWithoutLeavingTheGuardSet`
- `VmHwCvars1307SliceTests.BlackWhite_UsesTheOtherInternalAndFiltersReenter`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1307 --to 1307` (0 hard / 0 soft).

## Seams

SetTextureFilterMode is the screen callee. Each anisotropic or texture-filter assignment counts one call, and an out-of-range filter assignment counts a second call from the reentered callback. gamma_syncing is process lifetime and is cleared only at the end of the callback that set it. The virtual black and white internals start at 1 and are written by the archive cvars.

## Credit rationale

UNCredited hw_cvars.cpp 1-203 (+203). A full tally line scan shows no overlap. The file ends at line 203.
