# Chunk 804 — Whole `P_CheckSwitchRange` and `P_StartButton` (`p_switch.cpp` 71–88, 98–234).

## Scope

Whole `P_CheckSwitchRange` and `P_StartButton` (`p_switch.cpp` 71–88, 98–234).

## Native ranges

- `src/playsim/p_switch.cpp` **P_CheckSwitchRange** 98–234 (137 lines)
- `src/playsim/p_switch.cpp` **P_StartButton** 71–88 (18 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPSwitchCheckRange.cs`
Managed LOC (tally): 135.
Conversion commit: `bf6498e9c78eedb7da9d0a7227d65eecc677ba6b`.

## Padding review (2026-10-21)

| Managed | Native (`p_switch.cpp`) |
|---------|----------------------|
| `CheckSwitchRange` | `P_CheckSwitchRange` **98–234** |
| `StartButton` | `P_StartButton` **71–88** |
| `RunCheckSwitchRangePipeline` / `EffectiveSwitchFlags` / `FindActiveButton` | reachability phases **117–233**, button scan **77–84** |

### Out-of-body uncredit (804)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **0** |
| **Honest** | **155** |


**Post-review ratio:** 135/155 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmPSwitchCheckRangeTests.CheckSwitchRange_SkipsWhenNoFlags`
- `VmPSwitchCheckRangeTests.EffectiveSwitchFlags_ClearsMidtexWhenNoTexture`
- `VmPSwitchCheckRangeTests.RunCheckSwitchRangePipeline_UsesTopSwitchPath`
- `SwitchCheckRangeTests.TopSwitchRequiresReachableHeight`
- `VmPSwitchCheckRangeTests.StartButton_RepressSetsTimer`
- `SwitchTextureTests.RePressingActiveButtonAdvancesTimer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 804 --to 804` (0 hard / 0 soft).

## Seams

`P_MakeDivline`, `P_LineOpening`, `P_GetMidTexturePosition`, `TexAnim::FindSwitch`, and thinker spawn are injected.

## Credit rationale

Faithful switch reachability band; retires `P_CheckSwitchRange` credit from `p-teleport-switch-range-chunk6`.
