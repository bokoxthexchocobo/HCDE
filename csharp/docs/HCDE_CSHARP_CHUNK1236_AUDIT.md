# Chunk 1236 — Event ring, alt-enter fullscreen, delayed key-up, and the UI event view from d_event.cpp (1-273).

## Scope

Event ring, alt-enter fullscreen, delayed key-up, and the UI event view from d_event.cpp (1-273).

## Native ranges

- `src/common/engine/d_event.cpp` **D_ProcessEvents** 1–273 (273 lines)

**Credited native LOC:** 273 (tally `nativeLoc` 273).

## Managed

`VmEvent1236Slice.cs`
Managed LOC (tally): 330.
Conversion commit: `ad3377c4e7ff0c98f45a8f1d7249d7342ae4d270`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Process ring, alt-enter, and delayed key-up (**61–113**) | yes |
| Remove the next GUI character (**129–148**) | yes |
| Post, including the head-slot device drop (**159–171**) | yes |
| Mouse sensitivity and invert (**174–189**) | yes |
| Input event view (**192–217**) | yes |
| UI event view (**219–257**) | yes |
| VM field exports (**259–273**) | yes |
| Band hull (**1–273**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A second device change is stored when the head slot is still empty. The drop looks at the slot about to be written. Alt+Enter flips fullscreen and skips the responders. Escape reaches the game responder only, and only when WantEscape returns true. A key-up is delayed only when that key was eaten earlier in the same pass. GS_INTRO skips the console and the menu. A failed GUI-keydown precondition is logged and does not walk the ring. Mouse coordinates truncate toward zero. A GUI character encodes its uint16 value as UTF-8.

| Managed (`VmEvent1236Slice.cs`) | Native (`d_event.cpp`) |
|----------------------------------|------------------------|
| Ring, mouse, and the two event views | **1–273** |
| Band hull (exclusive slice) | **1–273** |

### Chunk **1236** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **273** |
| **Net headline** | **+273** vs post-**1235** |


**Post-review ratio:** 330/273 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmEvent1236SliceTests.Process_DelaysTheEatenKeyUpAndTogglesFullscreen`
- `VmEvent1236SliceTests.Remove_BlanksGuiCharsAndViewsTheEvent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1236 --to 1236` (0 hard / 0 soft).

## Seams

The ring is 128 slots. D_PostEvent drops a device change only when events[eventhead] is already a device change, before the dispatch callback. Dispatch returning true stores nothing. PostMouseMove scales by the sensitivity cvars, negates Y, then applies invertmousex and invertmouse, and posts only when a component is non-zero. Process on this Linux build toggles fullscreen for alt+Enter and for a GUI key-down of GK_RETURN with alt, then continues. WantEscape is called only for a key-down of KEY_ESCAPE, and a true result skips the console and the menu. GS_INTRO also skips those two. The game responder still runs, and a true result on a key-down marks that key. A later key-up of a marked key in the same pass is posted again after the pass. D_RemoveNextCharEvent requires a GUI key-down at the tail. It blanks GUI key-downs and the following GUI character, and it stops on any other GUI subtype. FInputEvent copies the low 8 bits of data1 into a one-character string, empty when that byte is 0. FUiEvent char events cast data1 to uint16 and encode it with the BMP UTF-8 rules. Mouse view coordinates truncate toward zero. The DEFINE_FIELD exports are the UiEvent and InputEvent names at the end of the file.

## Credit rationale

UNCredited d_event.cpp 1-273 (+273). The file ends at 273. A full tally line scan shows no overlap.
