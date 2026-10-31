# Chunk 1163 — Sight blockers, block iteration, and intercept ordering from `p_sight.cpp` (**385–618**).

## Scope

Sight blockers, block iteration, and intercept ordering from `p_sight.cpp` (**385–618**).

## Native ranges

- `src/playsim/p_sight.cpp` **Sight blockers and intercept order** 385–618 (234 lines)

**Credited native LOC:** 234 (tally `nativeLoc` 234).

## Managed

`VmPSightBlock1163Slice.cs`
Managed LOC (tally): 278.
Conversion commit: `efed1f76c24b14a22ddeb9ca348b1b84d6d1072c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `LineBlocksSight` (**388–422**) | yes |
| `P_SightCheckLine` (**432–465**) | yes |
| `P_SightBlockLinesIterator` (**476–522**) | yes |
| `P_SightTraverseIntercepts` (**534–605**) | yes |
| Band hull (**385–618**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **210–384** | `PTR_SightTraverse` is already credited and stays the visit callback |
| **619** onward | `P_SightPathTraverse` is already credited |

`ML_TWOSIDED` is **4**, `ML_BLOCKEVERYTHING` is **0x8000**, and `ML_BLOCKSIGHT` is **0x04000000**. `SF_SEEPASTSHOOTABLELINES` is **2**, `SF_SEEPASTBLOCKEVERYTHING` is **4**, and `SF_IGNOREWATERBOUNDARY` is **8**. `SPAC_Impact` is **8**. `ACS_Execute` is **80** and `ACS_ExecuteAlways` is **226**. `FF_EXISTS` is **1**, `FF_SOLID` is **2**, and `FF_SEETHROUGH` is **0x2000000**. A rejected fraction is **2147483647**. `EQUAL_EPSILON` is **1/65536**.

A line with no back sector, without the two-sided flag, or with the sight-block flag stops the check. A block-everything line also stops unless the viewer may see past it. With that permission but without the shootable permission, only an impact line whose special is execute or execute-always is open, and argument **1** must be **0** or this map. Any other line does not stop the trivial check.

A line already stamped with the current valid count is skipped. Otherwise it is stamped. Endpoints on the same side of the trace are not crossed, and the trace ends on the same side of the line are not crossed either. When no portal has been found, a trivial blocker rejects the line and does not store it. A portal skips that quick rejection. A stored line counts in slot **3**, is marked as a line, and does not yet have a fraction.

The block walk records `y * width + x`. A linked portal in the block, or a poly link while linked poly portals exist, sticks `portalfound` on before any line is tested. An empty poly object is skipped. A poly already stamped is skipped. A rejected line with no portal aborts the block with **0** and leaves later lines unstamped. The same rejection after a portal is recorded as **-1** and the walk continues. A clean block returns **1**.

Fractions run along the trace. One strictly before the start becomes **2147483647** and is not visited. The rest are visited nearest first, and a tie keeps the earlier entry. A visit that stops ends the walk. A finished visit is moved to **2147483647** so it is not chosen again. The final floor check runs only when the last sector is the target sector and that sector has floors. A floor is skipped when its see-through bit matches the viewer mask, when it does not exist, or when water boundaries are ignored and the floor is not solid. Crossing upward through the floor bottom, or downward through the floor top, stops the walk. Otherwise the walk succeeds.

| Managed (`VmPSightBlock1163Slice.cs`) | Native (`p_sight.cpp`) |
|---------------------------------------|------------------------|
| Blockers and intercept order | **385–618** |
| Band hull (exclusive slice) | **385–618** |

### Chunk **1163** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **234** |
| **Net headline** | **+234** vs post-**1162** |


**Post-review ratio:** 278/234 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmPSightBlock1163SliceTests.Blocks_StopsOneSidedAndLetsAnImpactScriptThrough`
- `VmPSightBlock1163SliceTests.CheckLine_StoresACrossingAndSkipsARepeat`
- `VmPSightBlock1163SliceTests.Block_StopsWithoutAPortalAndKeepsGoingThroughOne`
- `VmPSightBlock1163SliceTests.Traverse_OrdersFractionsAndClipsTheLastFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1163 --to 1163` (0 hard / 0 soft).

## Seams

The blockmap line list, poly links, and `PTR_SightTraverse` stay host-supplied. This slice decides which lines block, which are crossed, and the order they are visited.

## Credit rationale

UNCredited **`p_sight.cpp`** **385–618** (+234), the next whole hole after the opening setup, with no conversion or recognition overlap. `P_SightPathTraverse` starts at **619**.
