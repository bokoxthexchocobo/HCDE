# Chunk 1336 — FArg registration, PrintHelpMessage, the FArgs constructors through the single-name CheckParm from m_argv.cpp (1-335).

## Scope

FArg registration, PrintHelpMessage, the FArgs constructors through the single-name CheckParm from m_argv.cpp (1-335).

## Native ranges

- `src/common/utility/m_argv.cpp` **FArg** 1–335 (335 lines)

**Credited native LOC:** 335 (tally `nativeLoc` 335).

## Managed

`VmArgv1336Slice.cs`
Managed LOC (tally): 300.
Conversion commit: `45f1de7630c034ea3c05ce85ee70e0884d27b16b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FArg registration (**41–68**) | yes |
| PrintHelpMessage (**70–226**) | yes |
| Default constructor (**234–236**) | yes |
| Copy constructor (**244–247**) | yes |
| Argv constructors (**255–280**) | yes |
| Copy operator (**288–292**) | yes |
| SetRawArgs (**300–307**) | yes |
| FlushArgs (**315–318**) | yes |
| Single CheckParm (**330–334**) | yes |
| Band hull (**1–335**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| TestArgList | chunk 123, lines 336-344 |
| Array CheckParm | chunk 118, lines 346-356 |
| AppendRawArgs body | later, lines 542-552 |
| Linux isatty and Win32 console width | seam; TerminalColumns is injected |

Section order is the FName index from first registration. An empty section becomes Unknown and forces advanced. Duplicate names throw before insert. Details fall back to the summary when the details string is empty. Short help lowercases the name and right-aligns it in the column. Full help with a narrow width prints the stored name, the dim usage, and the blurb on separate lines. The wrap loop keeps the native space and newline break, including a newline with no earlier space. A negative width never wraps because the length compare uses an unsigned limit. SetRawArgs replaces the list. The FString constructor appends only when the pointer is present and the count is positive. Assign copies the list and leaves the catalog. The single CheckParm builds a one-element list and uses the default start of 1.

| Managed (`VmArgv1336Slice.cs`) | Native (`m_argv.cpp`) |
|----------------------------------|----------------------|
| Register, PrintHelp, constructors, SetRawArgs, FlushArgs, and CheckParm | **1–335** |
| Band hull (exclusive slice) | **1–335** |

### Chunk **1336** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **335** |
| **Net headline** | **+335** vs post-**1335** |


**Post-review ratio:** 300/335 managed/native ≈ 90% (src file LOC at HEAD; see tally).

## Tests

- `VmArgv1336SliceTests.RegisterHelpOrderAndWrapping`
- `VmArgv1336SliceTests.ArgsCopyFlushAndCheckParm`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1336 --to 1336` (0 hard / 0 soft).

## Seams

Terminal column detection on Linux and Win32 stays a seam. The array CheckParm at line 346 stays chunk 118, so the single-name CheckParm calls an injected CheckParmMany. AppendRawArgs at lines 542-552 stays outside this band; the FString constructor performs that append.

## Credit rationale

UNCredited m_argv.cpp 1-335 (+335). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of whole-file s_sound_eternity.cpp 1-331 and voxeldef.cpp 1-331. TestArgList at line 336 stays chunk 123.
