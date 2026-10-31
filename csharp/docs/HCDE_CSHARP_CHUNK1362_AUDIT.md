# Chunk 1362 — Game cvars from decal spread through cl_scaleweaponfov in g_cvars.cpp (1-244).

## Scope

Game cvars from decal spread through cl_scaleweaponfov in g_cvars.cpp (1-244).

## Native ranges

- `src/g_cvars.cpp` **GameCvars** 1–244 (244 lines)

**Credited native LOC:** 244 (tally `nativeLoc` 244).

## Managed

`VmGameCvars1362Slice.cs`
Managed LOC (tally): 406.
Conversion commit: `551658465ec93291648ceb1065208d3080bef7ba`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner and includes (**1–38**) | yes |
| Declarations and the sync flag (**39–46**) | yes |
| Corpse queue trims (**48–80**) | yes |
| ApplyCorpseFilter (**82–112**) | yes |
| SyncDynamicLightsState (**114–134**) | yes |
| Plain cvars (**136–143**) | yes |
| Turn speeds (**145–160**) | yes |
| gl_lights (**164–173**) | yes |
| Corpse filter and queue size (**176–193**) | yes |
| cl_maxdecals (**195–213**) | yes |
| r_maxparticles (**218–234**) | yes |
| teamdamage (**236–242**) | yes |
| cl_scaleweaponfov (**244–244**) | yes |
| Band hull (**1–244**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| r_draw.cpp r_dynlights | the default lives in another file |
| I_UpdateWindowTitle | a declaration with no body in this file |

`cl_spreaddecals` defaults true. `var_pushers` defaults true. `gl_cachenodes` defaults true. `gl_cachetime` defaults to 0.6. `alwaysapplydmflags` defaults false. `var_friction` defaults true. The turn speeds default to 640, 1280, 320, and 320. `gl_lights` defaults true. `sv_corpsefilter` defaults to 1. `sv_corpsequeuesize` defaults to 64. `cl_maxdecals` defaults to 1024. `r_maxparticles` defaults to 4000. `teamdamage` defaults to 0. `cl_scaleweaponfov` defaults to 1. `r_dynlights` starts true.

`EnableCallbacks` runs only the cvars declared here that lack `CVAR_NOINITCALL`. The turn speeds are already legal, so that init does not store. The corpse, light, decal, particle, and team callbacks are skipped.

A SERVERINFO store calls the sender when callbacks are installed. A denied controller prints `Only setting controllers or server/RCON console can change server CVAR %s` and does not store. Otherwise the sender returns when the game is not in startup and this is not demo playback or a savegame restore, and the store is skipped. `ForceSet` bypasses that sender. `DoSet` still runs the callback when the stored value did not change.

`HCDETrimMonsterCorpseQueue` compares the queue size with the limit as unsigned, so a negative limit does not pop. A null corpse is still deleted. `HCDETrimPlayerBodyQueue` clamps the limit to 0..32, destroys occupied slots from that limit through 31, and wraps `bodyqueslot` when it is past the limit. A non-positive limit zeros the slot.

`HCDEApplyCorpseFilter` returns for `netgame && gametic == 0`, for a game state other than level or title level, and for a null primary level. The walk is every level, and a null entry is skipped. Monster trimming follows bit 0 and player bodies follow bit 1. The queue limit is `max(sv_corpsequeuesize, 0)` and the filter is clamped to 0..3 inside the walk.

`SyncDynamicLightsState` returns while the guard is set. The saved flag on the other path is false, and the guard is cleared to that false. `r_dynlights` is assigned before `gl_lights`. Assigning `gl_lights` runs its callback. The nested `SyncDynamicLightsState` returns immediately, and the callback still recreates or deletes lights. A null level throws. A missing light hook throws `RecreateAllAttachedLights` or `DeleteAllAttachedLights`.

Turn speeds store 1 when the value is less than or equal to 0. That store re-enters once and stops. `gl_lights` syncs, then walks every level with no null skip.

`sv_corpsefilter` assigns `clamp(self, 0, 3)`. On the ForceSet path that assignment stores again, and the unchanged re-entry throws `sv_corpsefilter` before `HCDEApplyCorpseFilter`. During a level the assignment is swallowed, the out-of-range value remains, and apply clamps for the walk. `sv_corpsequeuesize` assigns 0 only when the value is negative. During a level that assignment is swallowed, so the negative value remains and apply sees a zero limit. Both callbacks skip apply when `netgame && gametic == 0`.

`cl_maxdecals` assigns 0 when negative and does not trim on that outer call. The re-entered call sees 0 and trims. A null thinker while the count is still above the limit throws `STAT_AUTODECAL` instead of spinning. `FirstThinker` is asked for stat 2.

`r_maxparticles` maps 0 to 4000, then values above 65535 to 65535, then values below 100 to 100. A correcting store re-enters, and both the inner and outer calls run `P_InitParticles` when the game is not in startup. Startup skips the hook even when it is null. A null hook throws `P_InitParticles` only when a level is visited.

`teamdamage` copies the float onto every level, including negatives, with no null skip. A level-time `Set` is swallowed.

| Managed (VmGameCvars1362Slice.cs) | Native (g_cvars.cpp) |
|-----------------------------------|----------------------|
| Cvars through weapon fov | **1–244** |
| Band hull (exclusive slice) | **1–244** |

### Chunk **1362** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **244** |
| **Net headline** | **+244** vs the prior chunk |


**Post-review ratio:** 406/244 managed/native ≈ 166% (src file LOC at HEAD; see tally).

## Tests

- `VmGameCvars1362SliceTests.DefaultsSyncAndCorpseQueues`
- `VmGameCvars1362SliceTests.DecalsParticlesTeamAndTurnSpeeds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1362 --to 1362` (0 hard / 0 soft).

## Seams

SetGenericRep's serverinfo sender is the seam that decides whether self= stores. r_dynlights is defined in r_draw.cpp and starts true. I_UpdateWindowTitle is only a declaration.

## Credit rationale

UNCredited g_cvars.cpp 1-244 (+244). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. p_spec.cpp 1-242 is 242. Gaps longer than 400 lines stay whole and are not sliced.
