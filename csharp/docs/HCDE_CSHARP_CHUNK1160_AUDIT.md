# Chunk 1160 — Rough search, side tests, and BSP walks from `p_maputl.cpp` (**1820–2106**).

## Scope

Rough search, side tests, and BSP walks from `p_maputl.cpp` (**1820–2106**).

## Native ranges

- `src/playsim/p_maputl.cpp` **Rough search and point on side** 1820–2106 (287 lines)

**Credited native LOC:** 287 (tally `nativeLoc` 287).

## Managed

`VmPMaputlSide1160Slice.cs`
Managed LOC (tally): 280.
Conversion commit: `3c5bab435d6b99a57d668f84d9dae9c7eb6e06fd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_RoughMonsterSearch` (**1821–1835**) | yes |
| `R_PointOnSideSlow` (**1846–1896**) | yes |
| `P_VanillaPointOnLineSide` (**1906–1938**) | yes |
| `PointInSubsector` (**1946–1962**) | yes |
| `PointInSectorBuggy` (**1971–1991**) | yes |
| `PointInRenderSubsector` (**1999–2018**) | yes |
| `BoxOnLineSide` and the three side actions (**2030–2106**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1788–1819** | `RoughBlockCheck` is already credited |
| `P_BlockmapSearch` | the spiral search stays host-supplied |

`ML_COMPATSIDE` is **0x800**. `EQUAL_EPSILON` is **1/65536**. A front-only search stores the actor origin and a front line of `-sin(yaw)`, `-cos(yaw)`. Any other search leaves that line at zero and still returns the host actor. A missing actor finds nothing.

`FloatToFixed` rounds half to even at **16** bits. A zero `dx` compares the fixed X and returns the sign of `dy`. A zero `dy` compares the fixed Y and returns the sign of `dx`. Otherwise the sign bits of `dy ^ dx ^ pointDx ^ pointDy` pick a side before any multiply. The remaining points compare `(double)dy * pointDx` with `(double)pointDy * dx`, and an equal product is the back side. The precise node test is `DMulScale(y-node.y, dx, node.x-x, dy, 32) > 0`, so an equal product is the front side. Vanilla diagonals use `MulScale((int)(delta * 256), fixed, 16)` and the same back-side tie. The precise point test is the cross product above **1/65536**. A compat line uses vanilla unless the action forces precise.

An empty game tree returns subsector **0**, or the supplied empty sector. Otherwise the precise walk converts the point once and follows children. The low bit marks a subsector. The buggy sector walk uses the slow test and returns that subsector's sector. An empty render tree returns **0**. A render walk consumes fixed coordinates and the precise test. A vertical box compares both edges with `<`. A horizontal box compares both edges with `>`. A negative delta flips those bits. A positive diagonal tests the left-top and right-bottom corners. A negative diagonal tests the other two. Matching corners return that side; otherwise the box crosses and the result is **-1**. An actor box is the center expanded by its radius.

| Managed (`VmPMaputlSide1160Slice.cs`) | Native (`p_maputl.cpp`) |
|---------------------------------------|--------------------------|
| Search, sides, and BSP walks | **1820–2106** |
| Band hull (exclusive slice) | **1820–2106** |

### Chunk **1160** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **287** |
| **Net headline** | **+287** vs post-**1159** |


**Post-review ratio:** 280/287 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmPMaputlSide1160SliceTests.Search_BuildsTheFrontLineAndReturnsTheHostActor`
- `VmPMaputlSide1160SliceTests.SlowAndVanilla_SplitOnTheLineAndKeepTheSignShortcut`
- `VmPMaputlSide1160SliceTests.Walk_UsesPreciseForGameNodesAndSlowForTheBuggySector`
- `VmPMaputlSide1160SliceTests.Box_CrossesAVerticalAndUsesCompatOnlyWhenImprecise`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1160 --to 1160` (0 hard / 0 soft).

## Seams

The blockmap spiral and RoughBlockCheck stay host-supplied. This slice builds the front line and classifies points.

## Credit rationale

UNCredited **`p_maputl.cpp`** **1820–2106** (+287), through the end of the 2106-line file, with no conversion or recognition overlap.
