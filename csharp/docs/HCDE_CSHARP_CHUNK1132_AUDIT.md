# Chunk 1132 — Cohen-Sutherland clipMline and the first drawMline screen endpoint from `am_map.cpp` (**1651–1800**); follows **1131**.

## Scope

Cohen-Sutherland clipMline and the first drawMline screen endpoint from `am_map.cpp` (**1651–1800**); follows **1131**.

## Native ranges

- `src/am_map.cpp` **Automap Cohen-Sutherland clip** 1651–1800 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapClipLine1132Slice.cs`
Managed LOC (tally): 138.
Conversion commit: `c773e6f3821f40a5e93df409b0d144bda2682a59`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Outcode bits and map-space reject (**1668–1717**) | yes |
| `CXMTOF` / `CYMTOF` into the clip box (**1719–1729**) | yes |
| Edge clip with `Scale` (**1731–1784**) | yes |
| `drawMline` origin of endpoint A (**1799–1800**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1651–1664** | closing braces and the clip comment |
| **1801+** | endpoint B, thickness, and antialiasing |

`Scale` is the widening `int32` divide from `cmdlib.h`. Map Y uses assignment (`TOP` if `y > m_y2`, `BOTTOM` if `y < m_y`); X is then OR-ed. A shared outcode bit rejects the segment.

| Managed (`VmAmMapClipLine1132Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| clipMline and screen start | **1651–1800** |
| Band hull (exclusive slice) | **1651–1800** |

### Chunk **1132** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1131** |


**Post-review ratio:** 138/150 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapClipLine1132SliceTests.Scale_UsesWideningIntegerDivision`
- `VmAmMapClipLine1132SliceTests.TryClip_RejectsASegmentThatIsEntirelyAboveTheMapWindow`
- `VmAmMapClipLine1132SliceTests.TryClip_CutsAVerticalSegmentToTheTopEdge`
- `VmAmMapClipLine1132SliceTests.TryClip_CutsAHorizontalSegmentToTheLeftEdge`
- `VmAmMapClipLine1132SliceTests.FrameOutcode_MarksTheCornerOutside`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1132 --to 1132` (0 hard / 0 soft).

## Seams

The second endpoint and thickness selection begin at **1801**. Host line drawing stays outside this slice.

## Credit rationale

UNCredited **`am_map.cpp`** **1651–1800** (+150), inside the 3594-line file, with no conversion or recognition overlap.
