# Chunk 1256 — Config section lookup, entry linking, and config load from configfile.cpp (482-639).

## Scope

Config section lookup, entry linking, and config load from configfile.cpp (482-639).

## Native ranges

- `src/common/utility/configfile.cpp` **EnsureValueForKey** 482–639 (158 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmConfigFile1256Slice.cs`
Managed LOC (tally): 150.
Conversion commit: `88703a6acb4d9d608b40e322347af4432961e8f4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| EnsureValueForKey (**492–512**) | yes |
| FindSection (**520–529**) | yes |
| RenameSection (**537–545**) | yes |
| FindEntry (**553–563**) | yes |
| NewConfigSection (**571–587**) | yes |
| NewConfigEntry (**595–611**) | yes |
| LoadConfigFile (**619–632**) | yes |
| Band hull (**482–639**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| File open and the line reader | host steps |
| ReadConfig body | later in the file |

A null current section does not add an entry. An existing key and value, compared without case, is left in place. A different value is appended, and the stored key keeps the case that was passed. Finding a section is case-insensitive and returns the first match. Renaming a missing section does nothing. Creating a section that already exists returns that section and does not clear its entries. A new section is appended. A new entry is appended, then its value is copied. The allocation count is the section object size plus the key length, so an empty key uses only that size. Load clears the existed flag, returns without reading when the path does not open, and stores the read result only after the path opens.

| Managed (`VmConfigFile1256Slice.cs`) | Native (`configfile.cpp`) |
|--------------------------------------|---------------------------|
| Sections, entries, and load | **482–639** |
| Band hull (exclusive slice) | **482–639** |

### Chunk **1256** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **158** |
| **Net headline** | **+158** vs post-**1255** |


**Post-review ratio:** 150/158 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmConfigFile1256SliceTests.Ensure_KeepsAnExistingValueAndReusesTheSection`
- `VmConfigFile1256SliceTests.Load_SetsExistedOnlyAfterASuccessfulRead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1256 --to 1256` (0 hard / 0 soft).

## Seams

Opening the path and reading the file are host steps. Name and value compare is ordinal and case-insensitive. The entry allocation is the LP64 size of the section object, 40 bytes, plus the key length, which is the sizeof expression in NewConfigEntry. SetValue replaces the stored text.

## Credit rationale

UNCredited configfile.cpp 482-639 (+158). A full tally line scan shows no overlap. ReadConfig starts at line 640 and stays outside this band.
