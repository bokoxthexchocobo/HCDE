# Chunk 1405 — UTF-16 and IBM437 conversion, the case-fold flags, and UTF-8 validation from unicode.cpp (1-150).

## Scope

UTF-16 and IBM437 conversion, the case-fold flags, and UTF-8 validation from unicode.cpp (1-150).

## Native ranges

- `src/common/filesystem/source/unicode.cpp` **utf16_to_utf8** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmUnicode1405Slice.cs`
Managed LOC (tally): 138.
Conversion commit: `9fdaa2b041fa187f93e4f887acd4464757177515`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| utf8_encode (**34–44**) | yes |
| utf16_to_utf8 (**53–87**) | yes |
| ibm437_to_utf8 (**95–117**) | yes |
| tolower_normalize (**125–130**) | yes |
| unicode_validate (**138–147**) | yes |
| Band hull (**1–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| utf8proc_map | callee; the option bits are the arguments |
| utf8proc_iterate | callee; a negative result is the reject |

utf8_encode replaces a negative, an out-of-range scalar, or a code point from U+D800 through U+DFFE with U+FFFD. U+DFFF is encoded as written. An empty UTF-16 or IBM437 input clears the buffer and does not add a terminator. Any other input encodes through the first zero and then appends one zero byte. A high surrogate consumes the following low surrogate. Any other following unit stays for the next pass. A low surrogate alone becomes U+FFFD. IBM437 bytes from 0x80 use the 128-entry table. The case fold passes null-terminated, stable, compose, and case-fold. Validation returns false when the iterator is negative. A non-negative result does not advance the byte, so the same byte is visited again.

| Managed (`VmUnicode1405Slice.cs`) | Native (`unicode.cpp`) |
|------------------------------------|-----------------------|
| Filename conversion and validation | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1405** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **150** |
| **Net headline** | **+150** vs the prior chunk |


**Post-review ratio:** 138/150 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmUnicode1405SliceTests.Utf16AndIbm437AppendATerminatorAndKeepAnUnpairedTail`
- `VmUnicode1405SliceTests.FoldPassesTheCaseFoldFlagsAndValidateRepeatsALiveByte`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1405 --to 1405` (0 hard / 0 soft).

## Seams

The case-fold map and the UTF-8 iterator stay callees. The file ends at line 150.

## Credit rationale

UNCredited unicode.cpp 1-150 (+150). After chunk 1404 this is the next whole-file hole. codegen.cpp 3778-3933 stays uncredited because the whole functions left in that gap are under 150 lines. The band is shorter than 400 lines and is not sliced. It runs through end of file.
