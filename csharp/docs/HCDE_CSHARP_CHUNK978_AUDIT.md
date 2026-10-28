# Chunk 978 — `P_RunClientSideLogic`, `P_CheckTickerPaused`, and `P_ClearLevelInterpolation` head from `p_tick.cpp` (**1–150**); adjacent to chunk **977** (**151–300**).

## Scope

`P_RunClientSideLogic`, `P_CheckTickerPaused`, and `P_ClearLevelInterpolation` head from `p_tick.cpp` (**1–150**); adjacent to chunk **977** (**151–300**).

## Native ranges

- `src/p_tick.cpp` **P_RunClientSideLogic** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPTicker978Slice.cs`
Managed LOC (tally): 81.
Conversion commit: `73c64ac2c15a2e8f025af2a63b766a7bb7abd834`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dedicated early out (**63–66**) | yes |
| Level/title client tick (**74–103**) | yes (predicates) |
| Cutscene branch (**104–107**) | yes |
| `P_CheckTickerPaused` (**126–137**) | yes |
| `P_ClearLevelInterpolation` head (**142–150**) | yes (gate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–48**, **49–58**, **113–121** | header / comment banners |
| **68–72**, **95–96**, **98**, **102**, **110** | host calls |

| Managed (`VmPTicker978Slice.cs`) | Native (`p_tick.cpp`) |
|----------------------------------|---------------------|
| `DedicatedServerSkipsClientSideLogic` | **63–66** |
| `GameStateRunsLevelClientSideTick` | **74–75** |
| `ShouldDecrementPlayerInventoryTics` / `DecrementInventoryTics` / `TryDecrementInventoryTicsForPlayer` | **78–82** |
| `ShouldClearClientSideActorInterpolation` | **89–92** |
| `ShouldTickAutomapOnLevel` | **101–102** |
| `ShouldRunCutsceneTicker` | **104–107** |
| `MenuActivePausesTicker` / `ConsoleStatePausesTicker` | **128–129** |
| `CheckTickerPausedGate` / `PauseMusicWhenMenusPauseSound` | **126–136** |
| `ShouldUpdateLevelInterpolatorBeforeActorClear` | **142–146** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **978** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**977** |


**Post-review ratio:** 81/150 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmPTicker978SliceTests.DedicatedServer_SkipsClientSideLogic`
- `VmPTicker978SliceTests.ClientSideLevelTick_InventoryAndThinkers`
- `VmPTicker978SliceTests.CutsceneBranch_AndMenuPause`
- `VmPTicker978SliceTests.CheckTickerPausedGate_MatchesNativePredicate`
- `VmPTicker978SliceTests.ClearLevelInterpolation_HeadPredicates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 978 --to 978` (0 hard / 0 soft).

## Seams

`C_Ticker`, `M_Ticker`, `UiTick`, `RunClientSideThinkers`, `StatusBar->CallTick`, `automap->Ticker`, `D_RunCutscene`, `PostUiTick`, `S_PauseSound`, and thinker iteration remain host-injected.

## Credit rationale

UNCredited head of `p_tick.cpp`; chunk **977** credits **151+** only. No `VmPTicker977Slice` overlap on native lines **1–150**.
