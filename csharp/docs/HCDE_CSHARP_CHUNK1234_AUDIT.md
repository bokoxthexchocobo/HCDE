# Chunk 1234 — Hexen STARTUP, NOTCH, and NETNOTCH lookup, progress notches, and the net-progress blits from startscreen_hexen.cpp (1-203).

## Scope

Hexen STARTUP, NOTCH, and NETNOTCH lookup, progress notches, and the net-progress blits from startscreen_hexen.cpp (1-203).

## Native ranges

- `src/common/startscreen/startscreen_hexen.cpp` **CreateHexenStartScreen** 1–203 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmHexenStart1234Slice.cs`
Managed LOC (tally): 168.
Conversion commit: `ea28c8f2af9fde89f68a92391f53df378ba8da05`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor lookup, size check, and music (**69–119**) | yes |
| Progress notches before the budget stretch (**129–150**) | yes |
| Net-progress re-entry, blit unreachable (**160–181**) | yes |
| NetDone sound (**191–195**) | yes |
| Create and countdown (**198–203**) | yes |
| Band hull (**1–203**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A lump of -1 falls back to the global namespace. A lump of -2 does not. The background must be 640 by 480 or the constructor errors before any blit, and the countdown does not run. Music uses order 1 and looping with force left false. Notch index ((CurPos + 1) * (512 / width)) / MaxPos is computed while CurPos <= MaxPos, before the base stretch. Each new index is blitted at x = 64 + width * index, y = 441. A backwards notch still cleans and plays StartupTick. At the cap the notch holds. DoNetProgress calls NetProgress, which virtual-calls this override again, so the net blit and misc/netnotch never run and NetCurPos stays put.

| Managed (`VmHexenStart1234Slice.cs`) | Native (`startscreen_hexen.cpp`) |
|---------------------------------------|-----------------------------------|
| Lookup, notches, unreachable net blit, NetDone | **1–203** |
| Band hull (exclusive slice) | **1–203** |

### Chunk **1234** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **203** |
| **Net headline** | **+203** vs post-**1233** |


**Post-review ratio:** 168/203 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmHexenStart1234SliceTests.Create_FallsBackAndPaintsEachNewNotch`
- `VmHexenStart1234SliceTests.Progress_HoldsAtTheCapAndSkipsTheNetBlit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1234 --to 1234` (0 hard / 0 soft).

## Seams

STARTUP, NETNOTCH, and NOTCH are looked up in namespace 15. Only a lump of exactly -1 falls back to the global namespace. GetImage is called with false. The background must be 640 by 480 or the constructor errors before the cache, and BeginCountdown does not run. On success the net, notch, and background bitmaps are cached in that order, the 640 by 480 bitmap is blitted, then music plays unless batchrun is set. S_ChangeMusic(name, true, true) is order 1, looping true, and force false. An empty song uses orb. DoProgress computes the notch while CurPos <= MaxPos, before FStartScreen::DoProgress stretches the budget. A changed notch blits every index from NotchPos up to but not including the new index at x = 64 + width * NotchPos and y = 441, then cleans with full=1 and plays StartupTick even when the loop does not advance. An equal notch holds. DoNetProgress calls FStartScreen::NetProgress, whose virtual DoNetProgress re-enters this override, so the net blit after that call is unreachable and NetCurPos is not updated. NetDone plays PickupWeapon and then the empty base.

## Credit rationale

UNCredited startscreen_hexen.cpp 1-203 (+203). The file ends at 203. A full tally line scan shows no overlap.
