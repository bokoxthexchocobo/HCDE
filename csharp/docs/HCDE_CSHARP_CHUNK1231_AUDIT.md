# Chunk 1231 — ENDOOM lump gate, blink timer, dismiss events, and the console ANSI dump from endoom.cpp (1-224).

## Scope

ENDOOM lump gate, blink timer, dismiss events, and the console ANSI dump from endoom.cpp (1-224).

## Native ranges

- `src/common/startscreen/endoom.cpp` **ST_Endoom** 1–224 (224 lines)

**Credited native LOC:** 224 (tally `nativeLoc` 224).

## Managed

`VmEndoom1231Slice.cs`
Managed LOC (tally): 204.
Conversion commit: `7edfcbc8365e555b9184753a37d4404cd331a9c2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| showendoom clamp (**58–62**) | yes |
| Screen construct and blink scan (**102–123**) | yes |
| Blink update (**125–135**) | yes |
| RunEndoom gates and dismiss loop (**146–190**) | yes |
| Console ANSI dump (**194–216**) | yes |
| ST_Endoom exit (**218–224**) | yes |
| Band hull (**1–224**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A screen with no blink attribute still blinks. 5267 ms does not paint and 5268 ms does. The ring wraps at 128. GUI key-up and button-up stay in the loop.

| Managed (`VmEndoom1231Slice.cs`) | Native (`endoom.cpp`) |
|-----------------------------------|------------------------|
| Clamp, blink, dismiss, and the ANSI dump | **1–224** |
| Band hull (exclusive slice) | **1–224** |

### Chunk **1231** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **224** |
| **Net headline** | **+224** vs post-**1230** |


**Post-review ratio:** 204/224 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmEndoom1231SliceTests.Run_ClampsTheCvarAndBlinksPastThePeriod`
- `VmEndoom1231SliceTests.Console_PrintsAnsiAndGuiButtonsDismiss`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1231 --to 1231` (0 hard / 0 soft).

## Seams

showendoom clamps to 0..2. Zero or an empty lump name returns before the lookup. A lump below zero or a length other than 4000 returns before the screen. showendoom 2 returns when the lump's container is the IWAD. Otherwise music stops, the 80x25 text screen is drawn on a 640x416 bitmap, the bottom row is cleared black, and TXT_QUITENDOOM is drawn gray on black. The blink flag starts true and the attribute scan never clears it, so a screen with no high bit still blinks. Update paints only when the uint64 clock is greater than the last paint plus 267, then flips the phase, cleans the texture, and renders. The column counter is not used here. The event ring is 128 entries. Key down and key up dismiss. GUI key down, left button, right button, and middle button dismiss. Other events are consumed and the loop continues. A time equal to the last paint plus 267 does not paint. A uint64 add that wraps still paints. The Linux console path ignores FancyStdOut. consoleendoom off or an empty name returns before the lookup. A bad lump returns after the lookup. A good lump reads 4000 bytes and calls vga_to_ansi. ST_Endoom prints the console dump, runs the screen, and exits with that code.

## Credit rationale

UNCredited endoom.cpp 1-224 (+224). The file ends at 224. A full tally line scan shows no overlap.
