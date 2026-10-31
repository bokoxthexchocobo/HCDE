# Chunk 1261 — Section cursor, clear, delete, and key lookup from configfile.cpp (214-465).

## Scope

Section cursor, clear, delete, and key lookup from configfile.cpp (214-465).

## Native ranges

- `src/common/utility/configfile.cpp` **FConfigFile::SetSection** 214–465 (252 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmConfigCursor1261Slice.cs`
Managed LOC (tally): 240.
Conversion commit: `aa69fad61b3145a0ad0120de1f9eb968520c639e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SetSection (**226–240**) | yes |
| SetFirstSection (**251–260**) | yes |
| SetNextSection (**271–283**) | yes |
| GetCurrentSection (**293–300**) | yes |
| ClearCurrentSection (**310–327**) | yes |
| DeleteCurrentSection (**339–363**) | yes |
| ClearKey (**374–397**) | yes |
| SectionIsEmpty (**408–411**) | yes |
| NextInSection (**423–434**) | yes |
| GetValueForKey (**445–454**) | yes |
| Band hull (**214–465**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Section and entry allocators | already credited |
| The setter after this band | already credited |

A missing name without create leaves the current section and the entry cursor unchanged. Create appends a section and selects it, with the entry cursor on its first entry. A repeated name, compared without case, selects the existing section and does not add another. First and next selection copy that section's first entry into the cursor. Advancing past the last section clears the section cursor and leaves the entry cursor where it was. An empty file does the same for first selection. Clearing removes every entry and points the entry tail at the head, and it does not move the entry cursor. Deleting a later section clears it, unlinks it, and selects the following section without moving the entry cursor. Deleting the last section points the section tail at the predecessor. Deleting the first section clears it and then faults, leaving it linked. Key removal compares without case, drops only the first match, and moves the entry tail when that match was last. An empty section returns before the search. Value lookup returns the first case-insensitive match and does not move the cursor. A section with no current section, or with no entries, is empty.

| Managed (`VmConfigCursor1261Slice.cs`) | Native (`configfile.cpp`) |
|----------------------------------------|------------------------|
| Cursor, clear, delete, key lookup | **214–465** |
| Band hull (exclusive slice) | **214–465** |

### Chunk **1261** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **252** |
| **Net headline** | **+252** vs post-**1260** |


**Post-review ratio:** 240/252 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmConfigCursor1261SliceTests.Walk_ResetsTheCursorAndLeavesItOnAFailedAdvance`
- `VmConfigCursor1261SliceTests.Delete_UnlinksTheMiddleAndClearKeyDropsTheFirstMatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1261 --to 1261` (0 hard / 0 soft).

## Seams

Finding and creating a section reuse the already-credited helpers. A null current section faults on key removal and on value lookup. Deleting the first section clears it, then faults because the predecessor walk ends on a null pointer. The entry cursor is left in place by clear and by delete.

## Credit rationale

UNCredited configfile.cpp 214-465 (+252). A full tally line scan shows no overlap. The setter that follows this band stays outside it.
