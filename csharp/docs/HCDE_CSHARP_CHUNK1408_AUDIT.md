# Chunk 1408 — i_timescale, vid_rendermode, fraglimit, wipetype, vid_cursor, the HUD toggle, and D_Render from d_main.cpp (422-649).

## Scope

i_timescale, vid_rendermode, fraglimit, wipetype, vid_cursor, the HUD toggle, and D_Render from d_main.cpp (422-649).

## Native ranges

- `src/d_main.cpp` **i_timescale** 422–649 (228 lines)

**Credited native LOC:** 228 (tally `nativeLoc` 228).

## Managed

`VmDMain1408Slice.cs`
Managed LOC (tally): 213.
Conversion commit: `281475358493deed8294b0787ad08fd671d7f2a8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| i_timescale (**422–439**) | yes |
| vid_rendermode (**444–464**) | yes |
| fraglimit (**466–482**) | yes |
| wipetype (**485–488**) | yes |
| vid_cursor (**490–507**) | yes |
| r_fullbright_overrides (**519–522**) | yes |
| I_FriendlyWindowTitle (**535–538**) | yes |
| D_ToggleHud (**594–613**) | yes |
| togglehud (**614–617**) | yes |
| D_Render (**629–649**) | yes |
| Band hull (**422–649**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| d_main.cpp **1–421** | argument help and prototypes; the first function ends at line 439, so a slice from line 1 through that function is 439 lines |
| dmflags | starts at line 657 |
| I_FreezeTime, I_SetCursor, SendPitchLimits, ExitLevel | callees |

A net game prints the net-game sentence and forces the cvar back to 1 without writing TimeScale. A value of at least 0.05 freezes time, stores TimeScale, then thaws. A smaller value prints the minimum sentence and leaves both the cvar and TimeScale alone. vid_rendermode outside 0 through 4 becomes 4. Modes 2 and 3 become 0 and 1. A user game sends pitch limits, and the texture filter is always applied. fraglimit walks all 64 player slots only in deathmatch when the limit is positive, prints TXT_FRAGLIMIT, exits with (0, false), and stops at the first hit. wipetype outside 0 through 3 becomes melt (1). vid_cursor treats any case of None with a non-empty game cursor as that picture; a failed set falls back to cursor. The fullbright callback records the value and changes nothing else. The friendly title stores the mode and refreshes the window title. D_ToggleHud flips the toggle. The hide path saves the current blocks, sprite, and messages, then forces 12, false, and false, and hides the console and menus. The show path copies the saved cvars back. togglehud calls that function. D_Render marks dynamic lights when hardware mode has gl_lights, or another mode has r_dynlights, or the level has probes, and then only if the level has lights or probes. Interpolation stores the time fraction before particles and the poly link. The action runs next. Restores run only when interpolation was requested.

| Managed (`VmDMain1408Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Startup callbacks and the render wrapper | **422–649** |
| Band hull (exclusive slice) | **422–649** |

### Chunk **1408** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **228** |
| **Net headline** | **+228** vs the prior chunk |


**Post-review ratio:** 213/228 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1408SliceTests.TimeScaleCursorAndRenderModeClamp`
- `VmDMain1408SliceTests.HudToggleFragLimitAndRenderPass`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1408 --to 1408` (0 hard / 0 soft).

## Seams

Time freeze, the cursor texture, pitch limits, and the level interpolator stay callees. dmflags starts at line 657.

## Credit rationale

UNCredited d_main.cpp 422-649 (+228). After chunk 1407 the next real-behavior hole longer than 400 lines is d_main.cpp 1-4657. hq4x and hq4x_asm stay later because they are generated tables. This slice is the first function-aligned 150-400 band of callbacks in that hole. The argument-help table before line 422 has no function boundary inside 150-400 lines. The band is not sliced further.
