# Chunk 1193 — Command tokens, cvar substitution, and argument shift from c_commandline.cpp (1–195).

## Scope

Command tokens, cvar substitution, and argument shift from c_commandline.cpp (1–195).

## Native ranges

- `src/common/console/c_commandline.cpp` **ParseCommandLine** 1–195 (195 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmCommandLine1193Slice.cs`
Managed LOC (tally): 180.
Conversion commit: `8e275362a730f65c5a6f5d92cdfb9956cae8ac3f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ParseCommandLine (**49–149**) | yes |
| FCommandLine::FCommandLine (**151–157**) | yes |
| FCommandLine::~FCommandLine (**159–165**) | yes |
| FCommandLine::Shift (**167–174**) | yes |
| FCommandLine::argc (**176–183**) | yes |
| FCommandLine::operator[] (**185–195**) | yes |
| Band hull (**1–195**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Quoted text turns \" into a quote, \\ into one backslash, and \c into the text-color escape. With escapes disabled those backslashes stay literal and a quote still ends the string. An unquoted $name is replaced by the cvar string as one argument, including spaces. A missing cvar keeps the typed token. argc caches the count and the buffer size. The indexer fills the vector once. Shift slides the pointers down and leaves the count unchanged, so the last argument is duplicated. Destroying the vector makes the next index parse the original line again.

| Managed (`VmCommandLine1193Slice.cs`) | Native (`c_commandline.cpp`) |
|----------------------------------------|----------------------------|
| Command tokens | **1–195** |
| Band hull (exclusive slice) | **1–195** |

### Chunk **1193** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **195** |
| **Net headline** | **+195** vs post-**1192** |


**Post-review ratio:** 180/195 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmCommandLine1193SliceTests.Parse_ExpandsQuotesEscapesAndCvars`
- `VmCommandLine1193SliceTests.Shift_DuplicatesTheTailUntilTheVectorIsRebuilt`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1193 --to 1193` (0 hard / 0 soft).

## Seams

FindCVarSub is a host lookup. A null result keeps the original token. Character codes 128–255 compare as signed char values, so outside quotes they are whitespace. Shift before the argument vector exists leaves the line unparsed.

## Credit rationale

UNCredited c_commandline.cpp 1–195 (+195). The file ends at 195. A full tally line scan shows no overlap.
