# Chunk 1133 — drawMline thickness and antialias offsets, grid block spacing, and the AM_FakeFlat early return from `am_map.cpp` (**1801–1950**); follows **1132**.

## Scope

drawMline thickness and antialias offsets, grid block spacing, and the AM_FakeFlat early return from `am_map.cpp` (**1801–1950**); follows **1132**.

## Native ranges

- `src/am_map.cpp` **Automap line strokes and grid** 1801–1950 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapDrawLine1133Slice.cs`
Managed LOC (tally): 143.
Conversion commit: `ccd02c6f3d58c3cbf8ddbed4c13afc5522e4a406`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Endpoint B and thickness (**1801–1804**) | yes |
| Antialias divisors and stroke offsets (**1806–1862**) | yes |
| Color-index overload is the palette lookup around **1866–1868** | yes |
| `drawGrid` cover length and block starts (**1877–1936**) | yes |
| `AM_FakeFlat` null height-sector return (**1947–1950**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1793–1800** | clip gate and endpoint A already credited by **1132** |
| **1951+** | viewer position and height transfer |

The thick antialias list repeats offset `(0,-1)` as its last sample, matching the native bottom-center call. `MAPBLOCKUNITS` is **128**. Alpha is `lineAlpha * 255 / divisor` truncated to a byte. Grid rotation is mode 1, or mode 2 while the view is active.

| Managed (`VmAmMapDrawLine1133Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Strokes, grid, fake-flat gate | **1801–1950** |
| Band hull (exclusive slice) | **1801–1950** |

### Chunk **1133** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1132** |


**Post-review ratio:** 143/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapDrawLine1133SliceTests.DrawThickness_PrefersAPositiveRequestAndSplitsAntialiasAlpha`
- `VmAmMapDrawLine1133SliceTests.AntialiasOffsets_RepeatTheTopCenterOnTheLastThickStroke`
- `VmAmMapDrawLine1133SliceTests.VerticalGridLines_StepByMapBlocksFromTheCeiledOrigin`
- `VmAmMapDrawLine1133SliceTests.KeepOriginalSector_WhenThereIsNoHeightSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1133 --to 1133` (0 hard / 0 soft).

## Seams

Host `AddThickLine` / `AddLine` stay outside. `AM_FakeFlat` height transfer begins at **1951**.

## Credit rationale

UNCredited **`am_map.cpp`** **1801–1950** (+150), inside the 3594-line file, with no conversion or recognition overlap.
