# Chunk 1259 — Config construction, copy, clear, and destruction from configfile.cpp (1-178).

## Scope

Config construction, copy, clear, and destruction from configfile.cpp (1-178).

## Native ranges

- `src/common/utility/configfile.cpp` **FConfigFile** 1–178 (178 lines)

**Credited native LOC:** 178 (tally `nativeLoc` 178).

## Managed

`VmConfigCtor1259Slice.cs`
Managed LOC (tally): 179.
Conversion commit: `24cbf08fba9dbe291c64276ea8424c328dae340d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FConfigFile (**39–47**) | yes |
| FConfigFile path (**55–63**) | yes |
| FConfigFile copy (**72–80**) | yes |
| ~FConfigFile (**89–108**) | yes |
| operator= (**116–134**) | yes |
| ClearConfig (**145–157**) | yes |
| ChangePathName (**165–168**) | yes |
| Band hull (**1–178**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The file reader | host step |
| Section creation helper | already credited |

A default object is writable, marked as existed, and has an empty path and no sections. A path object stores the path, runs the reader, and then sets both flags on, even when the reader cleared them. Sections the reader added stay. Assignment clears the destination, then copies each section. A repeated name, compared without case, keeps the first section and appends the new entries. The stored key keeps the case that was passed. An empty key uses only the section object size. Assignment does not change the destination path or the two flags. A copy object takes the source path, the copied sections, and the source flags. Destruction frees each entry value, then the entry, then the section. Clear frees the entries, drops the section, and points the tail back at the head.

| Managed (`VmConfigCtor1259Slice.cs`) | Native (`configfile.cpp`) |
|--------------------------------------|------------------------|
| Constructors, copy, clear, destroy | **1–178** |
| Band hull (exclusive slice) | **1–178** |

### Chunk **1259** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **178** |
| **Net headline** | **+178** vs post-**1258** |


**Post-review ratio:** 179/178 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmConfigCtor1259SliceTests.Open_ForcesExistedAfterTheLoad`
- `VmConfigCtor1259SliceTests.Assign_KeepsDestinationFlagsAndMergesSections`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1259 --to 1259` (0 hard / 0 soft).

## Seams

Reading the file is a host step. The path constructor calls that read and then forces the writable and existed flags on. Clearing a section drops its entries before the section itself is dropped. Copying a section reuses a case-insensitive name and appends entries. The entry allocation is the section object size, 40 bytes, plus the key length.

## Credit rationale

UNCredited configfile.cpp 1-178 (+178). A full tally line scan shows no overlap. CreateSectionAtStart starts at line 179 and stays outside this band.
