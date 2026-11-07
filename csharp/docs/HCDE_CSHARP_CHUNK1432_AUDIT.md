# Chunk 1432 — M_FindResponseFile through M_LoadDefaults in m_misc.cpp (156-406).

## Scope

M_FindResponseFile through M_LoadDefaults in m_misc.cpp (156-406).

## Native ranges

- `src/m_misc.cpp` **M_FindResponseFile** 156–406 (251 lines)

**Credited native LOC:** 251 (tally `nativeLoc` 251).

## Managed

`VmMisc1432Slice.cs`
Managed LOC (tally): 233.
Conversion commit: `02c3f8b98137b4ddc10388b41e0930398772f6d6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Response files (**156–244**) | yes |
| Command parsing (**251–332**) | yes |
| Config save and load (**339–406**) | yes |
| Band hull (**156–406**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment stripper | already credited through line 154 |
| PCX screenshot header | starts at line 414 |
| File open and shell folder | callees |

A response file is the argument after @. A missing file is a warning and the argument is removed. A found file is uncommented with empty lines and surrounding whitespace removed, then parsed twice: once for the count and once into the new vector. Arguments before and after the response file are kept. The scan stays on the inserted text, so nested response files expand. The 100th file prints the limit. Later files are ignored and removed. After at least one file, the command line from argument 1 is printed. Parsing skips bytes at or below space, keeps quoted text, turns a backslash quote into a quote, and stops an unquoted word at space or a quote. A null command string crashes. Saving with no config returns success. A requested filename is applied, the global archive always runs, a non-empty game name archives game data, and the same requested filename is applied again after the write. A final save repeats while the write fails and the ini-failed callee asks to retry, then drops the config. writeini with only the command name saves the current path. Failure prints the errno text. Opening the config directory saves, then opens the folder up to the last slash. Loading replaces the config and runs global setup.

| Managed (`VmMisc1432Slice.cs`) | Native (`m_misc.cpp`) |
|---------------------------------|----------------------|
| Response files and config save | **156–406** |
| Band hull (exclusive slice) | **156–406** |

### Chunk **1432** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **251** |
| **Net headline** | **+251** vs the prior chunk |


**Post-review ratio:** 233/251 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmMisc1432SliceTests.ResponseFilesExpandAndStopAtLimit`
- `VmMisc1432SliceTests.ConfigSaveKeepsRequestedPath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1432 --to 1432` (0 hard / 0 soft).

## Seams

The comment stripper above this band is already credited and is called as a callee. The PCX header starts at line 414.

## Credit rationale

UNCredited m_misc.cpp 156-406 (+251). This is response-file expansion and config save/load. The band is shorter than 400 lines and is not sliced.
