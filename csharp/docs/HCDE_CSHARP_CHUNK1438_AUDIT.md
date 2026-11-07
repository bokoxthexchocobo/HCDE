# Chunk 1438 — Controller run loop through measureintermissions in intermission.cpp (987-1077, 1086-1104, and 1113-1205).

## Scope

Controller run loop through measureintermissions in intermission.cpp (987-1077, 1086-1104, and 1113-1205).

## Native ranges

- `src/intermission/intermission.cpp` **DIntermissionController::Start** 987–1077 (91 lines)
- `src/intermission/intermission.cpp` **F_StartIntermission** 1086–1104 (19 lines)
- `src/intermission/intermission.cpp` **F_StartIntermission** 1113–1205 (93 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmIntermission1438Slice.cs`
Managed LOC (tally): 236.
Conversion commit: `35b3889d2ebbad23cc47c18f9a0f304617289615`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Controller start, responder, ticker, drawer, and destroy (**987–1077**) | yes |
| Descriptor intermission start (**1086–1104**) | yes |
| Named start, action wrappers, and measure command (**1113–1205**) | yes |
| Band hull (**987–1205**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment banners | lines 1078-1085 and 1105-1112 stay uncredited |
| NextPage | callee |

A missing screen ignores the event. A console or screenshot binding returns false. Menu and pause open the main menu and return true. Input in the first two tics is ignored. A net game that is not a demo readies a console player inside the 64-player range and always writes command 77. Demo playback falls through to the screen. A screen result of -1 sets advance. The ticker clears the sent flag when an advance is already pending, then advances when the screen ticker returns -1. A failed next page ends the intermission. The drawer clears the screen only when a page exists. Destroy drops the page and deletes an owned descriptor. Starting an intermission stops every channel, sets game action to nothing and the state to the finale, and wipes only for an in-level no-wipe handoff. A next page that fails destroys the controller and returns nothing. A missing name, an empty action list, or a title action does not start. The measure command walks the finale names in every listed language, counts newline rows, and prints a name wider than 360 or taller than 20 rows. An empty language uses the default string table. A two-letter language reads the terminating null as its third character.

| Managed (`VmIntermission1438Slice.cs`) | Native (`intermission.cpp`) |
|----------------------------------------|------------------------------|
| Run loop and measure command | **987–1077**, **1086–1104**, **1113–1205** |
| Band hull (exclusive slice) | **987–1205** |

### Chunk **1438** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **203** |
| **Net headline** | **+203** vs the prior chunk |


**Post-review ratio:** 236/203 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmIntermission1438SliceTests.ControllerInputAndTicker`
- `VmIntermission1438SliceTests.ControllerDrawAndDestroy`
- `VmIntermission1438SliceTests.StartFinale`
- `VmIntermission1438SliceTests.NamedStartAndWrappers`
- `VmIntermission1438SliceTests.MeasureWideRows`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1438 --to 1438` (0 hard / 0 soft).

## Seams

NextPage is a callee. Comment banners 1078-1085 and 1105-1112 stay uncredited.

## Credit rationale

UNCredited intermission.cpp 987-1205 with the two comment banners left out (+203). This is the controller run loop and the finale measure command. The band is shorter than 400 lines and is not sliced.
