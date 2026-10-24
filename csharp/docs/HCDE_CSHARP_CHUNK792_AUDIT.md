# Chunk 792 — ACS script constants, fixed-point conversions, clientside guards, and `DLevelScript` inline helpers (`p_acs.cpp` 472–760).

## Scope

ACS script constants, fixed-point conversions, clientside guards, and `DLevelScript` inline helpers (`p_acs.cpp` 472–760).

## Native ranges

- `src/playsim/p_acs.cpp` **ACS script constants** 472–534 (63 lines)
- `src/playsim/p_acs.cpp` **ACSToDouble** 596–624 (29 lines)
- `src/playsim/p_acs.cpp` **CallReturn** 626–645 (20 lines)
- `src/playsim/p_acs.cpp` **GetScriptOwner** 647–672 (26 lines)
- `src/playsim/p_acs.cpp` **DLevelScript inline helpers** 703–760 (58 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmPAcsScriptHelpers.cs`
Managed LOC (tally): 122.
Conversion commit: `15c77ab7e4ac70e4a9af53584f107991464583eb`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `LineFront` / `LineBack` | enum **472–475** |
| `SideFront` / `SideBack` | enum **476–479** |
| `GameSinglePlayer`–`GameTitleMap` | enum **485–490** |
| `SkillVeryEasy`–`SkillVeryHard` | enum **496–502** |
| `BlockNothing`–`BlockPlayers` | enum **503–509** |
| `LevelinfoParTime`–`LevelinfoSuckTime` | enum **510–521** |
| `PlayerinfoTeam`–`PlayerinfoFviewBob` | enum **522–534** |
| `AcsToDouble` / `AcsToFloat` / `DoubleToAcs` | **596–609** |
| `AcsToAngle` / `AngleToAcs` / `PitchToAcs` | **611–624** |
| `CallReturn` | struct **626–645** |
| `GetScriptOwner` | **647–650** |
| `ShouldIgnoreClientSideScript` | **652–667** |
| `IsClientSideScript` | **669–672** |
| `SetState` / `GetState` / `GetNext` | **703–706** |
| `MarkLocalVarStrings` / `LockLocalVarStrings` | **708–715** |
| `StringBuilderStart` / `StringBuilderFinish` | **740–755** |
| `PickActorIterator` | **757–760** |
| HUD/decal macro block (not hosted) | **535–589** |
| Fixed-point conversion comment | **591–595** |
| `DLevelScript` state enum (mirrored by `ScriptState`) | **681–693** |
| `DLevelScript` field declarations seam | **717–738** |
| `DLevelScript` method declarations seam | **762–808** |


**Post-review ratio:** 122/196 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsScriptHelpersTests.ScriptConstants_MatchNativeValues`
- `VmPAcsScriptHelpersTests.AcsToFloat_Uses65536Scale`
- `VmPAcsScriptHelpersTests.ShouldIgnoreClientSideScript_SuppressesOtherPlayer`
- `VmPAcsScriptHelpersTests.StringBuilderStack_PushesAndPops`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 792 --to 792` (0 hard / 0 soft).

## Seams

`AActor`, `FActorIterator`, and live `GlobalACSStrings` calls are injected.

## Credit rationale

Closes the 1–944 ACS prologue gap after stack/pool chunks 789–790.
