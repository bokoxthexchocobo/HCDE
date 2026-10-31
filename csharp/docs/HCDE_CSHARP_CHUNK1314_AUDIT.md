# Chunk 1314 — Include-line splice, the line directives, and the missing-file error from stb_include.cpp (1-209).

## Scope

Include-line splice, the line directives, and the missing-file error from stb_include.cpp (1-209).

## Native ranges

- `src/common/rendering/stb_include.cpp` **stb_include_string** 1–209 (209 lines)

**Credited native LOC:** 209 (tally `nativeLoc` 209).

## Managed

`VmStbInclude1314Slice.cs`
Managed LOC (tally): 160.
Conversion commit: `edf2ddc25682859b47d02f2829d0d8923f394855`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Whitespace, quotes, and include scan (**63–164**) | yes |
| String splice and file load (**166–209**) | yes |
| Band hull (**1–209**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Virtual filesystem | the host returns the lump text |

The scan starts each line at the first character. Spaces and tabs before the hash are skipped for the keyword and removed with the include line. `#include` needs a space, tab, or newline after the word, then a double quote. An unterminated quote, an angle-bracket name, and a quote glued to the word stay in the text. The recorded span runs from the start of the line through the character before the newline, so the include newline stays for the following text. The resume line number is one past the include line. A file pushes its name and uses the new count as the line-directive index. The directive is a leading newline, `#line`, the numbers, and the name. A nested load failure appends `Error: couldn't load '...'` and the caller returns an empty string. Opening a file for splicing clears the error first. A missing file does not push its name.

| Managed (`VmStbInclude1314Slice.cs`) | Native (`stb_include.cpp`) |
|---------------------------------------|----------------------------|
| Include splice and the load error | **1–209** |
| Band hull (exclusive slice) | **1–209** |

### Chunk **1314** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **209** |
| **Net headline** | **+209** vs post-**1313** |


**Post-review ratio:** 160/209 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmStbInclude1314SliceTests.Expand_SplicesTheIncludeAndResumesTheParentLine`
- `VmStbInclude1314SliceTests.Load_ClearsOnEntryAndAppendsTheMissingName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1314 --to 1314` (0 hard / 0 soft).

## Seams

FindFile and GetStringFromLump are the host file load. The slice receives the file text or a miss.

## Credit rationale

UNCredited stb_include.cpp 1-209 (+209). A full tally line scan shows no overlap. The file ends at line 209.
