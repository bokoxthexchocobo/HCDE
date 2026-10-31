# Chunk 1232 — Heretic LOADING screen, thermometer notches, and status text from startscreen_heretic.cpp (1-172).

## Scope

Heretic LOADING screen, thermometer notches, and status text from startscreen_heretic.cpp (1-172).

## Native ranges

- `src/common/startscreen/startscreen_heretic.cpp` **CreateHereticStartScreen** 1–172 (172 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmHereticStart1232Slice.cs`
Managed LOC (tally): 140.
Conversion commit: `adb812dfb7cd5e0a434e58e8f792032848e0b068`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor and version patch (**65–95**) | yes |
| Thermometer progress (**105–122**) | yes |
| Loading status (**132–143**) | yes |
| Status line append (**153–164**) | yes |
| Create and countdown (**167–172**) | yes |
| Band hull (**1–172**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Notch 2 does not paint. Notch 4 paints four pixels at x=112, then notch 8 paints the next four at x=116. The 404-wide budget stretches to 2452 only after the notch is chosen.

| Managed (`VmHereticStart1232Slice.cs`) | Native (`startscreen_heretic.cpp`) |
|------------------------------------------|--------------------------------------|
| Version patch, notches, and status text | **1–172** |
| Band hull (exclusive slice) | **1–172** |

### Chunk **1232** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **172** |
| **Net headline** | **+172** vs post-**1231** |


**Post-review ratio:** 140/172 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmHereticStart1232SliceTests.Create_PatchesTheVersionAndPaintsAlignedNotches`
- `VmHereticStart1232SliceTests.Progress_StretchesAfterTheNotchAndStatusUsesBytes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1232 --to 1232` (0 hard / 0 soft).

## Seams

LOADING must be exactly 4000 bytes or the constructor errors before the bitmap. Column 49 of text row 2 is overwritten with the character 3. The bitmap is 640x400. The thermometer origin is (112, 224), the width is 404, and the height is 16. A notch is computed from CurPos + 1 times that width, divided by MaxPos, before the base progress call. It paints only when the result differs from the previous notch and the low two bits are clear. The paint is the light-green text-mode entry, from the old notch to the new one. The base call stretches the budget only when advance is 1 and the cursor is inside the headroom, and that stretch happens after the notch. Loading status draws each byte at column 17 plus the index on the current message row, then always bumps the row, cleans, and renders with force left false. An empty message still bumps the row. Append draws at the running status column on row 24 with attribute 31 and adds the byte count. A color above 255 is truncated to a byte. When CurPos is already at MaxPos the notch divide is skipped and the base call still runs.

## Credit rationale

UNCredited startscreen_heretic.cpp 1-172 (+172). The file ends at 172. A full tally line scan shows no overlap.
