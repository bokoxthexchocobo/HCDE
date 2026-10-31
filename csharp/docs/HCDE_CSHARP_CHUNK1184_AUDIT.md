# Chunk 1184 — FraggleScript comment blanking, brace scan, dry run, and include from t_prepro.cpp (213–425).

## Scope

FraggleScript comment blanking, brace scan, dry run, and include from t_prepro.cpp (213–425).

## Native ranges

- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::ProcessFindChar** 213–425 (213 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmTPreproScan1184Slice.cs`
Managed LOC (tally): 243.
Conversion commit: `1fcbbb6a55af7f0c2c3afdd0ad75161f9682121e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ProcessFindChar (**213–303**) | yes |
| DryRunScript (**328–374**) | yes |
| Preprocess (**382–387**) | yes |
| ParseInclude (**401–425**) | yes |
| Band hull (**213–425**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–212** | section registration and hash lookup stay chunk 1183 |

A block comment is blanked, including its delimiters. A line comment is blanked up to, and not through, the newline. A quoted span hides a closing brace, and a backslash skips the next character. A colon stores a label at the separator before the name. An empty name still creates the label and prints the native typo. Script number -1 skips labels. A brace pushes a section, scans for its closer, and stores that index. A missing closer or an open comment is fatal. Dry run sets if, elseif, else, and loop types only for a function token, and while or for also store the line start. Preprocess sets the length to one less than the buffer and then scans. Include reads the lump, blanks it, and runs that span.

| Managed (`VmTPreproScan1184Slice.cs`) | Native (`t_prepro.cpp`) |
|----------------------------------------|------------------------|
| Scan, dry run, and include | **213–425** |
| Band hull (exclusive slice) | **213–425** |

### Chunk **1184** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **213** |
| **Net headline** | **+213** vs post-**1183** |


**Post-review ratio:** 243/213 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmTPreproScan1184SliceTests.Scan_BlanksCommentsHidesQuotedBracesAndStoresLabels`
- `VmTPreproScan1184SliceTests.Braces_NestAndFatalErrorsStopTheScan`
- `VmTPreproScan1184SliceTests.DryRun_TypesSectionsAndIncludeBlanksTheLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1184 --to 1184` (0 hard / 0 soft).

## Seams

Section create and hash lookup are chunk 1183. GetTokens, lump lookup, and Run are host callbacks. script_error throws. A missing include lump records the fatal and returns.

## Credit rationale

UNCredited t_prepro.cpp 213–425 (+213). ProcessFindChar starts at 213 and ParseInclude ends at 425, which is the end of the file. The section table 1–212 is chunk 1183. A full tally line scan shows no overlap.
