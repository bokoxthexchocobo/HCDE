# Chunk 1195 — Alternate HUD cvars, class creation, and draw size from shared_hud.cpp (1–186).

## Scope

Alternate HUD cvars, class creation, and draw size from shared_hud.cpp (1–186).

## Native ranges

- `src/g_statusbar/shared_hud.cpp` **DBaseStatusBar::CreateAltHUD** 1–186 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmSharedHud1195Slice.cs`
Managed LOC (tally): 258.
Conversion commit: `d251286198265cec6c9053566d0dc2c1ab427f9b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| HUD cvars (**46–89**) | yes |
| hud_althud_forceinternal (**97–101**) | yes |
| DoCreateAltHUD (**103–130**) | yes |
| DBaseStatusBar::CreateAltHUD (**132–147**) | yes |
| DBaseStatusBar::DrawAltHUD (**159–186**) | yes |
| Band hull (**1–186**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Registration stores archive cvars and does not run the force-internal callback. That callback rebuilds the HUD only when a status bar exists. None skips class lookup. An unknown name or a class outside AltHud is reported in red and the built-in AltHud is used. A previous HUD is destroyed first. Forcing the internal class skips the game class. The old-scale path divides the screen by the UI scale. The new path uses 640 divided by the scale factor, then the screen aspect, and optionally five sixths of that height. Draw clears the console player's inventory timer and calls the AltHud Draw virtual with the global player and the computed size.

| Managed (`VmSharedHud1195Slice.cs`) | Native (`shared_hud.cpp`) |
|--------------------------------------|---------------------------|
| Alternate HUD | **1–186** |
| Band hull (exclusive slice) | **1–186** |

### Chunk **1195** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **186** |
| **Net headline** | **+186** vs post-**1194** |


**Post-review ratio:** 258/186 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmSharedHud1195SliceTests.Reset_RegistersTheArchiveCvarsWithoutCallingTheCallback`
- `VmSharedHud1195SliceTests.CreateAltHUD_FallsBackAndTheCallbackRebuilds`
- `VmSharedHud1195SliceTests.DrawAltHUD_SizesFromTheScalePathAndClearsInventoryTics`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1195 --to 1195` (0 hard / 0 soft).

## Seams

Class lookup and GetUIScale are host results. A missing AltHud class after the fallback aborts. Draw reads the global status bar's player. An empty player table is not indexed.

## Credit rationale

UNCredited shared_hud.cpp 1–186 (+186). The file ends at 186. A full tally line scan shows no overlap.
