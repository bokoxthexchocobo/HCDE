# Chunk 1220 — DeluxePaint large-page lookup and frame decoder from animlib.cpp (1-290).

## Scope

DeluxePaint large-page lookup and frame decoder from animlib.cpp (1-290).

## Native ranges

- `src/common/thirdparty/animlib.cpp` **ANIM_LoadAnim** 1–290 (290 lines)

**Credited native LOC:** 290 (tally `nativeLoc` 290).

## Managed

`VmAnimLib1220Slice.cs`
Managed LOC (tally): 281.
Conversion commit: `7ddc93ac51fc91d1a76cab551bbd3bc451609bb0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| findpage (**45–79**) | yes |
| loadpage (**89–98**) | yes |
| decodeframe (**115–174**) | yes |
| renderframe (**184–203**) | yes |
| drawframe (**213–217**) | yes |
| ANIM_LoadAnim (**220–272**) | yes |
| ANIM_DrawFrame (**275–288**) | yes |
| Band hull (**1–290**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A bad magic or a file shorter than the header, the 128-byte gap, and 768 palette bytes returns -1 before any state change. Too many page descriptors returns -2 after the header fields are stored. Zero records still copies the palette and returns -1. The first request for frame N paints frames 0 through N-1.

| Managed (`VmAnimLib1220Slice.cs`) | Native (`animlib.cpp`) |
|------------------------------------|------------------------|
| Large-page anim | **1–290** |
| Band hull (exclusive slice) | **1–290** |

### Chunk **1220** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **290** |
| **Net headline** | **+290** vs post-**1219** |


**Post-review ratio:** 281/290 managed/native ≈ 97% (src file LOC at HEAD; see tally).

## Tests

- `VmAnimLib1220SliceTests.Load_RejectsAShortFileAndReadsThePalette`
- `VmAnimLib1220SliceTests.DrawFrame_DecodesFromThePreviousFrameAndKeepsSkippedPixels`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1220 --to 1220` (0 hard / 0 soft).

## Seams

LittleShort and LittleLong are identity on this little-endian host, and the load writes those values back into the caller buffer. DrawFrame paints from the stored frame through the frame before the request, then stores the request. A short opcode of 0x80 falls through to the long opcode. Skipped pixels keep the previous frame. An odd preamble length skips one extra byte.

## Credit rationale

UNCredited animlib.cpp 1-290 (+290). The file ends at 290. A full tally line scan shows no overlap.
