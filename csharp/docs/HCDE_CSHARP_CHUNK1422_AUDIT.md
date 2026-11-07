# Chunk 1422 — D_InitGame startup type, startup screen, and resource load in d_main.cpp (4019-4188).

## Scope

D_InitGame startup type, startup screen, and resource load in d_main.cpp (4019-4188).

## Native ranges

- `src/d_main.cpp` **D_InitGame** 4019–4188 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmDMain1422Slice.cs`
Managed LOC (tally): 236.
Conversion commit: `8e91264f0a17d7014d2019e5fdd33262e06f507e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Startup type, keys, and exec (**4020–4050**) | yes |
| Strings, CPU, and compat mode (**4052–4081**) | yes |
| Startup window and video loader (**4083–4117**) | yes |
| Command line, sound, and mapinfo (**4119–4139**) | yes |
| Textures, fonts, teams, and actors (**4141–4172**) | yes |
| Player classes and wad settings (**4174–4188**) | yes |
| Band hull (**4019–4188**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| D_InitGame boot | ends at line 4018 |
| GL definitions | start at line 4190 |
| Config, string, CPU, texture, sound, and actor bodies | callees |

Default startup becomes Hexen, Heretic, or Strife only for an exact game type. Doom and a combined type stay on the default. A chosen type is left alone. A null config crashes before exec. A live exec runs and is cleared. A dedicated server always clears delayed commands and prints the count when it is above 0, with a plural s for every count other than 1. Strings load for the current language. Unless this is a restart, the machine-state line is shown and the CPU dump is printed. A null dump crashes. Compat mode is parsed in base 10 when the argument is present, including an empty argument. Batch and dedicated runs skip the startup-screen print. A restart window is created with progress 0. A dedicated window uses the texture budget. A client window guards the progress window and uses that same budget. The video loader runs only when a startup screen is wanted and this is not dedicated. It clears the pre-video status, initializes video, compiles one shader, and builds the start screen from the budget times 10 over 9 when per-shader progress is positive, otherwise the budget plus 3. A null start screen is not rendered. Command line, reverb, sound data, map info, skill, episode, message box, endoom, blur, statistics, and MUSINFO follow, with MUSINFO after map info. Texture setup clears sprite frames and runs the progress callback once per added texture. The callback advances the window and, when a start screen exists, the screen. Hacks and build tiles are passed in. The border flat is looked up as a flat and the conback uses true and 0.25. A null border crashes. Fonts, translations, and the generic UI with false follow. Teams, translation parsing, actor init, and jobs follow. The actor and key-binding phases are set only when a start screen exists. No player classes stops with the no-classes message before the final progress tick. Otherwise the window and the start screen advance once more.

| Managed (`VmDMain1422Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Startup screen and resource load | **4019–4188** |
| Band hull (exclusive slice) | **4019–4188** |

### Chunk **1422** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **170** |
| **Net headline** | **+170** vs the prior chunk |


**Post-review ratio:** 236/170 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1422SliceTests.StartupWindowAndVideo`
- `VmDMain1422SliceTests.ResourcesCallbacksAndClasses`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1422 --to 1422` (0 hard / 0 soft).

## Seams

Config, exec, strings, CPU, texture, sound, mapinfo, and actor calls stay callees. The boot phase ends at line 4018. GL definitions start at line 4190.

## Credit rationale

UNCredited d_main.cpp 4019-4188 (+170). This continues D_InitGame after the startup flags and stops after the wad-settings progress. The launch tail stays at line 4189. The band is not sliced further.
