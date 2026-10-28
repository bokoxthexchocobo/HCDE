# Chunk 954 — `PCD_SETHUDSIZE` through `PCD_GETPLAYERINFO` setup from `p_acs.cpp` (**10287–10436**). Follows **948** corridor end (**≤10049** credited) and avoids **10050–10199**.

## Scope

`PCD_SETHUDSIZE` through `PCD_GETPLAYERINFO` setup from `p_acs.cpp` (**10287–10436**). Follows **948** corridor end (**≤10049** credited) and avoids **10050–10199**.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 10287–10436 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode954Slice.cs`
Managed LOC (tally): 87.
Conversion commit: `8e6924810b4944535dd19f14e7220a7c18235bde`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_SETHUDSIZE` (**10288–10296**) | yes |
| `PCD_GETLEVELINFO` (**10298–10313**) | yes |
| `PCD_CHANGESKY` (**10315–10332**) | yes |
| `PCD_SETCAMERATOTEXTURE` (**10334–10363**) | yes |
| `PCD_SETACTORANGLE` / `SETACTORPITCH` (**10365–10373**) | yes (stack pop) |
| `PCD_SETACTORSTATE` (**10375–10415**) | yes |
| `PCD_PLAYERCLASS` / `PCD_GETPLAYERINFO` head (**10417–10436**) | yes |

| Managed (`VmPAcsVmOpcode954Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `ResolveHudSize` | **10289–10295** |
| `ResolveLevelInfoValue` / `LevelInfoSnapshot` | **10299–10311** |
| `SkyNameApplies` | **10321–10327** |
| `UseActivatorAsCamera` | **10339–10347** |
| `SetActorStateTargetsActivator` / count helpers | **10380–10411** |
| `ResolvePlayerClass` / `ResolveGetPlayerInfoFailure` | **10418–10435** |
| `AimDistToAcs` / `PlayerInfoTeamValue` | **10439+** (aimdist math; band ends at userinfo bind) |

### Chunk **954** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**953** |


**Post-review ratio:** 87/150 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode954SliceTests.ResolveHudSize_NegatesHeightWhenCoveringStatusBar`
- `VmPAcsVmOpcode954SliceTests.ResolveLevelInfoValue_ReturnsParTime`
- `VmPAcsVmOpcode954SliceTests.ResolveLevelInfoValue_UnknownQueryIsZero`
- `VmPAcsVmOpcode954SliceTests.SkyNameApplies_RequiresNonEmptyFirstChar`
- `VmPAcsVmOpcode954SliceTests.SetActorStateTargetsActivator_WhenTidZero`
- `VmPAcsVmOpcode954SliceTests.AccumulateSetActorStateCount_IncrementsOnSuccess`
- `VmPAcsVmOpcode954SliceTests.ResolvePlayerClass_ReturnsNegativeOneWhenUnresolved`
- `VmPAcsVmOpcode954SliceTests.AimDistToAcs_ScalesByNativeFactor`
- `VmPAcsVmOpcode954SliceTests.UseActivatorAsCamera_WhenTidZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 954 --to 954` (0 hard / 0 soft).

## Seams

`DoGetCVar`, `TexMan`, `InitSkyMap`, `SetActorAngle`/`Pitch`, `SetState`, and `HCDE_ResolveACSPlayer` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode954Slice.cs` (`AcsVm` dispatch is not matrix credit).
