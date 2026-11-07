# Chunk 1437 — Scroller through NextPage in intermission.cpp (778-907 and 915-985).

## Scope

Scroller through NextPage in intermission.cpp (778-907 and 915-985).

## Native ranges

- `src/intermission/intermission.cpp` **DIntermissionScreenScroller::Init** 778–907 (130 lines)
- `src/intermission/intermission.cpp` **DIntermissionController::DIntermissionController** 915–985 (71 lines)

**Credited native LOC:** 201 (tally `nativeLoc` 201).

## Managed

`VmIntermission1437Slice.cs`
Managed LOC (tally): 286.
Conversion commit: `562c3bad4a7363f718af4a73a25bc7497cbba7ed`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Scroller init, responder, and drawer (**778–907**) | yes |
| Controller constructor and NextPage (**915–985**) | yes |
| Band hull (**778–985**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment banners | lines 771-777 and 908-914 stay uncredited |
| Super init | callee |
| GetFullscreenRect | callee, reimplemented for the scroller |

A scroller key returns -1 and jumps the ticker to the end of the scroll unless scroll abort is on, in which case the key is consumed and returns 0. Any other event returns 0. The drawer rounds a 200-tall picture to 240 and a 400-tall picture to 480, then scales a 4:3 viewport. Horizontal motion starts the second picture off the left for a left scroll and the first picture off the left for a right scroll. Vertical motion starts the second picture above for an up scroll. An unknown direction uses the up path. The ticker is clamped to the scroll time, and a zero scroll time produces a non-numeric position. Both pictures are clipped to the fitted rectangle and the background becomes the second picture.

The controller starts at the first action, with advance clear and the first-page flag set. NextPage on the last action with no link returns the end-game flag and leaves the current screen up. A wiper sets the wipe and continues. A title returns false. Any other action copies the previous background, initializes the new screen, clears the first-page flag, and starts a cutscene only when this is a net game and not demo playback. A link deletes the owned descriptor, clears the delete flag, and continues at the linked descriptor. A missing link returns false.

| Managed (`VmIntermission1437Slice.cs`) | Native (`intermission.cpp`) |
|----------------------------------------|------------------------------|
| Scroller and NextPage | **778–907**, **915–985** |
| Band hull (exclusive slice) | **778–985** |

### Chunk **1437** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **201** |
| **Net headline** | **+201** vs the prior chunk |


**Post-review ratio:** 286/201 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmIntermission1437SliceTests.ScrollAdvanceAndHorizontal`
- `VmIntermission1437SliceTests.ScrollVerticalAndAspect`
- `VmIntermission1437SliceTests.NextPageCreatesAndLinks`
- `VmIntermission1437SliceTests.NextPageStops`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1437 --to 1437` (0 hard / 0 soft).

## Seams

Super init is a callee. GetFullscreenRect is reimplemented for the scroller. Controller Start begins at line 987.

## Credit rationale

UNCredited intermission.cpp 778-985 with the comment banner 908-914 left out (+201). This is the scroller and the page advance. The band is shorter than 400 lines and is not sliced.
