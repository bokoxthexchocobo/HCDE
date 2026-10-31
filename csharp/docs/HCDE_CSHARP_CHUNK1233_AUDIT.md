# Chunk 1233 — Strife startup picture lookup, even laser notches, and the bot and peasant blit from startscreen_strife.cpp (1-186).

## Scope

Strife startup picture lookup, even laser notches, and the bot and peasant blit from startscreen_strife.cpp (1-186).

## Native ranges

- `src/common/startscreen/startscreen_strife.cpp` **CreateStrifeStartScreen** 1–186 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmStrifeStart1233Slice.cs`
Managed LOC (tally): 132.
Conversion commit: `f8ca3be06837c3405af0b133044990e6c510545f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Picture lookup and asset check (**94–121**) | yes |
| Even laser progress (**131–146**) | yes |
| Background, laser, bot, and peasant blit (**158–178**) | yes |
| Create and countdown (**181–186**) | yes |
| Band hull (**1–186**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

STRTPD1 with a null image is not cached. Notch 1 does not paint. Notch 2 puts the laser at x=62. Laser 8 raises the bot to y=140. A 319-wide STARTUP0 errors.

| Managed (`VmStrifeStart1233Slice.cs`) | Native (`startscreen_strife.cpp`) |
|----------------------------------------|-------------------------------------|
| Lookup, even notches, and the blits | **1–186** |
| Band hull (exclusive slice) | **1–186** |

### Chunk **1233** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **186** |
| **Net headline** | **+186** vs post-**1232** |


**Post-review ratio:** 132/186 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmStrifeStart1233SliceTests.Create_RejectsABadBackgroundAndPaintsEvenNotches`
- `VmStrifeStart1233SliceTests.Draw_IgnoresTheOldLaserAndHopsTheBot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1233 --to 1233` (0 hard / 0 soft).

## Seams

Each of the eight picture names is looked up in namespace 15 and, only on a miss, in the global namespace. A found lump asks for an image with the second argument false and caches style 0. A null image leaves that slot empty. STARTUP0 must be 320 by 200 or the constructor errors before any blit, and the countdown does not run. A good screen blits the background, the even laser frame, the bot, and peasant frame 0, then sets scale 2. The notch is (CurPos + 1) times 184 divided by MaxPos, computed before the base budget stretch. It redraws only when the value changed and is even. DrawStuff does not read the old laser. The background blit is pic 7 at the origin. The laser is pic 4 or 5 at x = 60 plus the new laser and y = 156. The bot hop is max(0, (laser >> 1) % 5 - 2) added to y = 138. The peasant frame is (laser >> 1) & 3 at (262, 136). An odd notch does not draw. A cursor already at MaxPos skips the divide and the base call still runs.

## Credit rationale

UNCredited startscreen_strife.cpp 1-186 (+186). The file ends at 186. A full tally line scan shows no overlap.
