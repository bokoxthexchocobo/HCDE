# Chunk 1226 — Notify stacking, fade, and the centered stack suffix from c_notifybuffer.cpp (1-220).

## Scope

Notify stacking, fade, and the centered stack suffix from c_notifybuffer.cpp (1-220).

## Native ranges

- `src/console/c_notifybuffer.cpp` **FNotifyBuffer::AddString** 1–220 (220 lines)

**Credited native LOC:** 220 (tally `nativeLoc` 220).

## Managed

`VmNotify1226Slice.cs`
Managed LOC (tally): 195.
Conversion commit: `415b584417f2fd41b70e4ef6651251d424ae2339`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Notify buffer fields (**35–52**) | yes |
| con_notifytime and center/pulse cvars (**50–52**) | yes |
| con_scaletext clamp (**54–57**) | yes |
| Fade constant and identical count (**59–60**) | yes |
| con_notifylines shift (**62–65**) | yes |
| Clear (**66–80**) | yes |
| AddString (**82–135**) | yes |
| Draw (**137–215**) | yes |
| SetConsoleNotifyBuffer (**217–220**) | yes |
| Band hull (**1–220**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A live duplicate is popped before a swallow can keep it. Level 6 stays untranslated when the table slot is changed. The centered suffix uses the combined width.

| Managed (`VmNotify1226Slice.cs`) | Native (`c_notifybuffer.cpp`) |
|----------------------------------|-------------------------------|
| Stack, fade, and centered suffix | **1–220** |
| Band hull (exclusive slice) | **1–220** |

### Chunk **1226** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **220** |
| **Net headline** | **+220** vs post-**1225** |


**Post-review ratio:** 195/220 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmNotify1226SliceTests.Add_PopsALiveDuplicateAndStillSwallows`
- `VmNotify1226SliceTests.Draw_FadesPulsesAndCentersTheStackSuffix`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1226 --to 1226` (0 hard / 0 soft).

## Seams

Identical text pops the previous line only while its ticker is still below the timeout. A fading copy starts the count over at 1. The pop happens before the status bar can swallow the add, so a swallowed duplicate is still removed. Print flags are masked with 1023 before the base add. con_scaletext below 0 becomes 0. Shift keeps the tail when the line cap shrinks and ignores a negative cap. Fade uses j/6 only while j is below 6. Pulse multiplies by 0.7 plus 0.3 sine of the millisecond clock over 100. A print level at or above 5 uses the untranslated color and does not index the color table. The stack suffix is drawn only on the last row when the count is above 1, and centering divides the drawer width by the scale before subtracting the combined width. If every row is idle or expired, Top snaps to TopGoal.

## Credit rationale

UNCredited c_notifybuffer.cpp 1-220 (+220). The file ends at 220. A full tally line scan shows no overlap.
