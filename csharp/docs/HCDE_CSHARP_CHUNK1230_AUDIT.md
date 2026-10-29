# Chunk 1230 — Tab-command insert, completion cycling, and the colored completion list from c_tabcomplete.cpp (1-312).

## Scope

Tab-command insert, completion cycling, and the colored completion list from c_tabcomplete.cpp (1-312).

## Native ranges

- `src/common/console/c_tabcomplete.cpp` **C_TabComplete** 1–312 (312 lines)

**Credited native LOC:** 312 (tally `nativeLoc` 312).

## Managed

`VmTab1230Slice.cs`
Managed LOC (tally): 427.
Conversion commit: `9935cd6ca1713b298b811d651097d9ccaa4ec991`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sorted insert and removal (**65–135**) | yes |
| Completion cycle (**137–234**) | yes |
| Colored column list (**236–312**) | yes |
| Band hull (**1–312**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

GIVE increments give. None stays after Remove. A wide column on a 5-column console stays on one line. The second single-match tab does not print the list again.

| Managed (`VmTab1230Slice.cs`) | Native (`c_tabcomplete.cpp`) |
|--------------------------------|------------------------------|
| Insert, cycle, and the column list | **1–312** |
| Band hull (exclusive slice) | **1–312** |

### Chunk **1230** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **312** |
| **Net headline** | **+312** vs post-**1229** |


**Post-review ratio:** 427/312 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmTab1230SliceTests.Add_SortsNamesAndCyclesForward`
- `VmTab1230SliceTests.Complete_FillsThePrefixAndSkipsASecondList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1230 --to 1230` (0 hard / 0 soft).

## Seams

Commands stay sorted by a case-insensitive C compare. A repeated name, including a different ASCII case, bumps the use count and keeps the first spelling. Map is stored as the predefined spelling Map. None is name zero: it can be inserted and Remove leaves it in place. An empty command list returns before the name lookup. A name that was never interned is not removed. The first Tab on a fresh line skips leading spaces, and a blank line returns. A prefix hit records the start and the typed length. When more than one command matches and the list has not been shown, the columns print in a field three wider than the longest name. CVars are green, aliases red, commands light blue, and the header is blue. The column counter is an unsigned subtract of the console width divided by the scale, so a field wider than the console does not wrap. The longest common prefix replaces the typed token when it is longer, keeping the leading spaces, and that fill does not move the caret. The next tab cycles forward or backward and appends a space. Walking off the matches truncates back with the typed length. con_notablist and a list already shown skip the column print. Clearing the commands leaves the tab flags as they were.

## Credit rationale

UNCredited c_tabcomplete.cpp 1-312 (+312). The file ends at 312. A full tally line scan shows no overlap.
