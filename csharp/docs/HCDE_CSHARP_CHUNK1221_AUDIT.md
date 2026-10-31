# Chunk 1221 — Lexer and include-aware lump parse from parsecontext.cpp (1-340).

## Scope

Lexer and include-aware lump parse from parsecontext.cpp (1-340).

## Native ranges

- `src/gamedata/xlat/parsecontext.cpp` **FParseContext::GetToken** 1–340 (340 lines)

**Credited native LOC:** 340 (tally `nativeLoc` 340).

## Managed

`VmParseContext1221Slice.cs`
Managed LOC (tally): 633.
Conversion commit: `f6aad87e843fd5cf2caeaa03cfcab071037296bb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AddSym (**37–44**) | yes |
| FindSym (**51–62**) | yes |
| GetToken (**70–276**) | yes |
| PrintError (**283–290**) | yes |
| ParseLump (**299–340**) | yes |
| Band hull (**1–340**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Numbers use strtoll, then the low 32 bits. A dot continues with strtod. Keywords come from FindToken before symbols and line specials. An include whose next token is not a string prints the error and returns without restoring the saved line or file.

| Managed (`VmParseContext1221Slice.cs`) | Native (`parsecontext.cpp`) |
|----------------------------------------|-----------------------------|
| Lexer and lump include | **1–340** |
| Band hull (exclusive slice) | **1–340** |

### Chunk **1221** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **340** |
| **Net headline** | **+340** vs post-**1220** |


**Post-review ratio:** 633/340 managed/native ≈ 186% (src file LOC at HEAD; see tally).

## Tests

- `VmParseContext1221SliceTests.GetToken_LexesNumbersCommentsAndTheShiftBugs`
- `VmParseContext1221SliceTests.ParseLump_ResolvesIncludesAndKeepsTheCallerLine`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1221 --to 1221` (0 hard / 0 soft).

## Seams

strtoll and strtod are the host libc conversions, and the int cast keeps the low 32 bits. isspace, isdigit, isalpha, and isalnum are C-locale ASCII. strnicmp is strncasecmp. A single < or > consumes the next character and returns 0. << or >> without = returns 0 and ungets the following character. A 79-character identifier whose last stored character is _ is ungot. #include and #define match 7 and 6 characters with no token boundary. SourceFile for an include is the token union, so the next token that writes the union replaces that name. A bad include returns without restoring SourceLine or SourceFile.

## Credit rationale

UNCredited parsecontext.cpp 1-340 (+340). The file ends at 340. A full tally line scan shows no overlap.
