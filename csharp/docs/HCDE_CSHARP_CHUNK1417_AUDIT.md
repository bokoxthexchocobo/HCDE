# Chunk 1417 — Startup status, episode and skill selection, and IWAD sprite renames in d_main.cpp (2802-3083).

## Scope

Startup status, episode and skill selection, and IWAD sprite renames in d_main.cpp (2802-3083).

## Native ranges

- `src/d_main.cpp` **StartupStatusLine** 2802–3083 (282 lines)

**Credited native LOC:** 282 (tally `nativeLoc` 282).

## Managed

`VmDMain1417Slice.cs`
Managed LOC (tally): 268.
Conversion commit: `3ab940a954386ff0cf11ac5f40f1f6faac3c37a9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| StartupStatusLine (**2802–2812**) | yes |
| StartupPreVideoStatus (**2814–2821**) | yes |
| StartupClearPreVideoStatus (**2823–2826**) | yes |
| D_StartupProgress (**2828–2842**) | yes |
| CheckEpisodeCmd (**2846–2891**) | yes |
| CheckDefaultSkill (**2893–2908**) | yes |
| NewFailure (**2910–2913**) | yes |
| RenameSprites (**2926–3083**) | yes |
| Band hull (**2802–3083**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CheckCmdLine | ends at line 2800 |
| RenameNerve | starts at line 3096 |
| Startup show, clear, progress, episode list, skill cvar, fatal error, file names, and namespaces | callees |

StartupStatusLine prints the text unless the run is a batch, then tells the start screen the loading phase. The color and centering arguments are unused. Pre-video status is shown only when the run is not a batch and not a dedicated server, and the trace is recorded either way, with a null string becoming empty. Clearing the pre-video status always runs. Progress does nothing for a non-positive advance and otherwise advances the window and the screen independently. CheckEpisodeCmd reads -episode. A value whose episode index is outside the list prints the invalid line and leaves the selection unset. A selected episode replaces the map and sets autostart even when a map was already chosen. Without a selection, an existing map is kept, except a launcher-default map is replaced by the first episode map when that map is non-empty and differs. An unset map with no selection uses the first episode map and does not set autostart. CheckDefaultSkill records a non-negative default and then applies a non-negative -skill value. NewFailure reports the system-heap allocation failure. RenameSprites keeps Doom sprite names. Heretic, Hexen, and Strife each have their own four-character rename list, applied to IWAD sprites or to every sprite when -oldsprites or nospriterename is set. Hexen also rewrites an ARTIINVU name in that same case. A missing MNTRZ sprite shifts MNTR frames F through K at the two frame slots for every sprite. Blood is renamed to BLUD unless the game has the Doom or Chex bit. Global lumps in the IWAD container range that are listed for deletion are blanked, and that blanking wins over the big-font rename. Otherwise the game's big font is renamed to BIGFONT: Strife, Raven games, or the Doom name.

| Managed (`VmDMain1417Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Startup status, episode, skill, and sprite renames | **2802–3083** |
| Band hull (exclusive slice) | **2802–3083** |

### Chunk **1417** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **282** |
| **Net headline** | **+282** vs the prior chunk |


**Post-review ratio:** 268/282 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1417SliceTests.StatusEpisodeAndSkill`
- `VmDMain1417SliceTests.SpriteAndFontRenames`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1417 --to 1417` (0 hard / 0 soft).

## Seams

Startup show, clear, progress, the episode list, the skill cvar, the fatal error, file-name checks, and namespaces stay callees. RenameNerve starts at line 3096.

## Credit rationale

UNCredited d_main.cpp 2802-3083 (+282). StartupStatusLine through RenameSprites are whole functions. The comment banners between them sit inside the span. The functions before RenameSprites are under 150 lines together, so they share this band. RenameNerve starts at line 3096. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
