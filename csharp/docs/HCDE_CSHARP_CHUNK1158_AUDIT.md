# Chunk 1158 — Proximity and warp from `p_things.cpp` (**566–835**).

## Scope

Proximity and warp from `p_things.cpp` (**566–835**).

## Native ranges

- `src/playsim/p_things.cpp` **CheckProximity and Warp** 566–835 (270 lines)

**Credited native LOC:** 270 (tally `nativeLoc` 270).

## Managed

`VmPThingsWarp1158Slice.cs`
Managed LOC (tally): 436.
Conversion commit: `cf4703bfd660ee07eff4e26afa1d268a14da9464`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_Thing_CheckProximity` (**566–690**) | yes |
| `P_Thing_Warp` (**692–811**) | yes |
| `A_Warp` (**819–835**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **536–565** | `P_Thing_CheckInputNum` is already credited |
| `COPY_AAPTREX` | pointer selection stays host-supplied |
| `GetPortalOffsetPosition` / `P_TryMove` / `P_TestMobjLocation` | portal trace, fit, and slide stay host-supplied |

`CPXF_ANCESTOR` is **1**, `CPXF_LESSOREQUAL` is **2**, `CPXF_NOZ` is **4**, `CPXF_COUNTDEAD` is **8**, `CPXF_DEADONLY` is **16**, `CPXF_EXACT` is **32**, `CPXF_SETTARGET` is **64**, `CPXF_SETMASTER` is **128**, `CPXF_SETTRACER` is **256**, `CPXF_FARTHEST` is **512**, `CPXF_CLOSEST` is **1024**, `CPXF_SETONPTR` is **2048**, and `CPXF_CHECKSIGHT` is **4096**. `WARPF_ABSOLUTEOFFSET` is **1**, `WARPF_ABSOLUTEANGLE` is **2**, `WARPF_USECALLERANGLE` is **4**, `WARPF_NOCHECKPOSITION` is **8**, `WARPF_INTERPOLATE` is **0x10**, `WARPF_WARPINTERPOLATION` is **0x20**, `WARPF_COPYINTERPOLATION` is **0x40**, `WARPF_STOP` is **0x80**, `WARPF_TOFLOOR` is **0x100**, `WARPF_TESTONLY` is **0x200**, `WARPF_ABSOLUTEPOSITION` is **0x400**, `WARPF_BOB` is **0x800**, `WARPF_MOVEPTR` is **0x1000**, `WARPF_COPYVELOCITY` is **0x4000**, and `WARPF_COPYPITCH` is **0x8000**. `MF2_FLOATBOB` is **0x80000**.

A missing reference, a missing class, or a distance at or below **0** returns **0** before any write. The reference actor and unmorphed actors are skipped. Ancestor mode keeps kind-of matches; otherwise the class must be exact. Actors that are not on the map are skipped. A hit needs distance strictly below the limit and, unless `CPXF_NOZ` is set, a vertical gap strictly below that same limit. Sight, dead-only, and count-dead filters run before the pointer choice. Setting a pointer with closest or farthest keeps scanning after the count is passed. Closest replaces only a strictly nearer actor. Farthest replaces only a strictly farther one, so both flags together follow whichever branch matched last. With neither distance flag, the first hit is kept. An early stop sets the result to **0** when less-or-equal or exact is set, and to **1** otherwise. After the scan, an equal count is **1**. A short count is **1** only for less-or-equal without exact. The counting form returns the counter and still writes the pointer. `CPXF_SETONPTR` writes the reference; otherwise the caller is written.

A missing caller or destination returns false. `WARPF_MOVEPTR` swaps the two actors before the move. The Z offset gains reference height times the height factor. Unless the angle is absolute, the caller's or the reference's yaw is added. Sine and cosine then build the radius shift. A relative offset rotates with left and right swapped, then a host line-portal displacement is added to the reference XY. Absolute position uses the raw offsets plus the radius shift and ignores the reference origin. Floor mode replaces Z with the host floor plus the shifted Z. Failure restores the old origin. Test-only does the same and returns true without changing yaw, velocity, or interpolation. Otherwise yaw becomes the final angle. Copied pitch is applied before an extra pitch, and a player is clamped at each step. Copied velocity is applied before stop, so stop wins. Warp interpolation translates the previous origin by the move minus the host group offset. Copy interpolation sets the previous origin from the new position plus the difference of the two host offsets. Any other call without the interpolate flag snaps the previous origin and angles to the current ones. Bob adds `sin((phase + maptime * factor) * 180 / 32) * 8 * strength` when the reference floats. Try-move then replaces XY when the host reports a slide. The action returns **0** when the destination is missing.

| Managed (`VmPThingsWarp1158Slice.cs`) | Native (`p_things.cpp`) |
|---------------------------------------|--------------------------|
| Proximity and warp | **566–835** |
| Band hull (exclusive slice) | **566–835** |

### Chunk **1158** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **270** |
| **Net headline** | **+270** vs post-**1157** |


**Post-review ratio:** 436/270 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmPThingsWarp1158SliceTests.Proximity_SkipsTheReferenceAndStopsOnceTheCountIsPassed`
- `VmPThingsWarp1158SliceTests.Proximity_KeepsSearchingForClosestAndWritesThePointer`
- `VmPThingsWarp1158SliceTests.Warp_RotatesLeftAndRestoresAFailedOrTestOnlyMove`
- `VmPThingsWarp1158SliceTests.Warp_CopiesPitchThenClampsThePlayerAndBobs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1158 --to 1158` (0 hard / 0 soft).

## Seams

Pointer selection, sight, class match, floor after linking, line-portal displacement, and try-move slides stay host-supplied.

## Credit rationale

UNCredited **`p_things.cpp`** **566–835** (+270), through the end of the 835-line file, with no conversion or recognition overlap.
