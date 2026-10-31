# Chunk 1323 — The fatal-error window, the clipboard text, and the log viewer from errorwindow.cpp (1-359).

## Scope

The fatal-error window, the clipboard text, and the log viewer from errorwindow.cpp (1-359).

## Native ranges

- `src/widgets/errorwindow.cpp` **ErrorWindow::ExecModal** 1–359 (359 lines)

**Credited native LOC:** 359 (tally `nativeLoc` 359).

## Managed

`VmErrorWindow1323Slice.cs`
Managed LOC (tally): 720.
Conversion commit: `d9658c1813f075db1bb38338687770e283735bc1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Modal frame and the button row (**32–81**) | yes |
| Clipboard text and the bar remap (**83–149**) | yes |
| Close and the button geometry (**151–181**) | yes |
| Log lines, the abort line, and the scrollbar (**185–263**) | yes |
| Paint, wheel, keys, and the scroll clamp (**265–359**) | yes |
| Band hull (**1–359**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The commented icon and border fills | they do not run |
| UTF-8 and the font-color table | callees, not lines of this file |

The window is 1200 by 700 and centered on the screen. An empty minidump builds the restart button. A minidump builds the save button. The clipboard drops the color escape, keeps the color letter, and turns 0x1D through 0x1F into U+2550. The suffix is the execution line, the message, and a newline. A log line splits on newlines, then an empty line, the abort line, and the message. The scrollbar max is the line count and the position is the last line. Buttons pack from the right with a 10-pixel gap and a 20-pixel inset. Paint fills 38,38,38 and walks upward from the rounded position. Home goes to 0. End goes to the max and clamps to the last line. The report zip stores minidump.dmp and log.txt.

| Managed (`VmErrorWindow1323Slice.cs`) | Native (`errorwindow.cpp`) |
|----------------------------------------|---------------------------|
| Modal, clipboard, layout, log, and the report | **1–359** |
| Band hull (exclusive slice) | **1–359** |

### Chunk **1323** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **359** |
| **Net headline** | **+359** vs post-**1322** |


**Post-review ratio:** 720/359 managed/native ≈ 201% (src file LOC at HEAD; see tally).

## Tests

- `VmErrorWindow1323SliceTests.Modal_CentersAndStripsTheClipboard`
- `VmErrorWindow1323SliceTests.Layout_ScrollsAndSavesTheReport`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1323 --to 1323` (0 hard / 0 soft).

## Seams

GetCharFromString and MakeUTF8 live in utf8.cpp. V_ParseFontColor and the flat log colors live in v_font.cpp and textcolors.txt. The zip writer is miniz. This slice includes those callees so the clipboard, the colored lines, and the crash zip match.

## Credit rationale

UNCredited errorwindow.cpp 1-359 (+359). A full tally line scan shows no overlap. The file ends at line 359.
