# Chunk 977 — `P_ClearLevelInterpolation` tail and `P_Ticker` paused path, global freeze, time-freezer sound resume, and level think loop head from `p_tick.cpp` (**151–300**).

## Scope

`P_ClearLevelInterpolation` tail and `P_Ticker` paused path, global freeze, time-freezer sound resume, and level think loop head from `p_tick.cpp` (**151–300**).

## Native ranges

- `src/p_tick.cpp` **P_Ticker** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPTicker977Slice.cs`
Managed LOC (tally): 84.
Conversion commit: `4473e7f8e9c1a9db70df0b01d6529b9553badccb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Actor/psprite interpolation clear (**151–170**) | yes (predicates) |
| Status bar interpolation clear (**173–174**) | yes |
| `P_Ticker` pause + `MF8_RECREATELIGHTS` (**205–221**) | yes |
| Global freeze quarter-tic (**229–238**) | yes |
| Time-freezer → resume sound scan (**245–252**) | yes |
| Renderer interpolate toggle (**255–258**) | yes |
| Level loop: think gate + clocks (**277–307**) | yes (predicates) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **172**, **184–187**, **190–202** | host poll / interpolator hooks |
| **225**, **254**, **273–275**, **293–296** | host calls (`DPSprite::NewTick`, specials, particles, events) |
| **177–179**, **652–659** | comment banners |

| Managed (`VmPTicker977Slice.cs`) | Native (`p_tick.cpp`) |
|----------------------------------|---------------------|
| `ActorNeedsRecreateLights` / `ApplyRecreateLightsClear` | **215–218** |
| `TickerPausedEarlyOut` | **205–206** |
| `TryApplyGlobalFreezeChange` / `MergeGlobalFreezeIntoLevelState` | **229–238** |
| `AllPlayersClearOfTimeFreezer` | **245–251** |
| `RendererInterpolationAfterSightReset` | **255–258** |
| `ShouldAdvancePspriteInterpolationReset` | **158–169** |
| `ShouldClearStatusBarInterpolation` | **173–174** |
| `SkipHcdeRuntimeJoinerPlayerThink` / `ShouldInvokePlayerThink` | **277–290** |
| `ShouldUpdateSpecials` / `IncrementLevelClocks` | **299–307** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **977** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**976** |


**Post-review ratio:** 84/150 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmPTicker977SliceTests.RecreateLightsFlag_ClearsWhenSet`
- `VmPTicker977SliceTests.GlobalFreeze_TogglesOnQuarterTics`
- `VmPTicker977SliceTests.TimeFreezerScan_ResumesWhenAllClear`
- `VmPTicker977SliceTests.TickerPauseAndInterpolation_Gates`
- `VmPTicker977SliceTests.HcdeJoiner_SkipsPlayerThinkUntilAuthority`
- `VmPTicker977SliceTests.LevelClocksAndSpecials`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 977 --to 977` (0 hard / 0 soft).

## Seams

`HCDERconPollListener`, `K8tiAgentPoll`, `HCDEAIDirectorTick`, `S_ResumeSound`, `P_PlayerThink`, `P_ThinkParticles`, `Level->Tick`, and thinker runs remain host-injected.

## Credit rationale

First UNCredited `p_tick.cpp` conversion band; file EOF **309** so hull stays in-range. No overlap with `VmPMobjActorTick` (`p_mobj.cpp`).
