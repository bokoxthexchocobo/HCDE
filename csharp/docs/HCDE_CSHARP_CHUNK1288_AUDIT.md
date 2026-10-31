# Chunk 1288 — Startup flags, language options, and the settings layout from settingspage.cpp (1-296).

## Scope

Startup flags, language options, and the settings layout from settingspage.cpp (1-296).

## Native ranges

- `src/widgets/settingspage.cpp` **SettingsPage::SettingsPage** 1–296 (296 lines)

**Credited native LOC:** 296 (tally `nativeLoc` 296).

## Managed

`VmSettings1288Slice.cs`
Managed LOC (tally): 516.
Conversion commit: `bf5ad8631a617b98068e4fba24e9c9611d073512`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| File-load table (**33–38**) | yes |
| Constructor (**40–145**) | yes |
| Set values (**147–167**) | yes |
| Language text (**169–197**) | yes |
| Language change (**199–206**) | yes |
| Layout (**208–296**) | yes |
| Band hull (**1–296**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| String table | host lookup returns the key when it is missing |
| File search | host booleans for the three extra wads |
| Text measurement | host preferred widths |

Backends are compiled in. OpenGL is selected only for backend 0. A radio click forces that box on and clears the other box. Setting a box directly does not run that callback, so both can be on, and the saved backend follows the OpenGL box. Dont-ask stores the inverted query flag. Start flags keep bits 1, 2, 4, 8, and 16. File-load values are 0, 3, 7, and -1. An unmatched value stays on custom, and custom does not write the behaviour back. The scan strips a leading UTF-8 mark, folds ASCII case, and keeps the last matching language. A semicolon, a region line, and a block comment are not tokens. A quoted backslash-quote becomes a quote. A missing comma stops the scan, keeps the languages already stored, and hides the language block. An empty scan still shows that block. The language callback runs after the selection changes. The extra column appears only when a search hits, and a short page does not shrink the checkbox rows. The language list height floors at zero.

| Managed (`VmSettings1288Slice.cs`) | Native (`settingspage.cpp`) |
|------------------------------------|------------------------------|
| Flags, languages, and the layout | **1–296** |
| Band hull (exclusive slice) | **1–296** |

### Chunk **1288** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **296** |
| **Net headline** | **+296** vs post-**1287** |


**Post-review ratio:** 516/296 managed/native ≈ 174% (src file LOC at HEAD; see tally).

## Tests

- `VmSettings1288SliceTests.Setup_ParsesTheLastLanguageAndKeepsACustomLoad`
- `VmSettings1288SliceTests.Layout_StacksTheColumnsAndHidesABrokenList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1288 --to 1288` (0 hard / 0 soft).

## Seams

String lookup, file search, and text measurement are host inputs. The language list is the joined display text.

## Credit rationale

UNCredited settingspage.cpp 1-296 (+296). A full tally line scan shows no overlap. The file ends at line 296.
