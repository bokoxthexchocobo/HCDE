# Chunk 932 — `LS_Autosave` through `LS_GlassBreak` spawn midpoint (`p_lnspec.cpp` **3155–3304**). Pivot from **2869–3018** (overlaps `VmPLnspecSetPlayerProperty` **2911–3128**); **3019–3129** sub-150 deferred.

## Scope

`LS_Autosave` through `LS_GlassBreak` spawn midpoint (`p_lnspec.cpp` **3155–3304**). Pivot from **2869–3018** (overlaps `VmPLnspecSetPlayerProperty` **2911–3128**); **3019–3129** sub-150 deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Autosave** 3155–3304 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecAutosaveCommunicator932Slice.cs`
Managed LOC (tally): 147.
Conversion commit: `705a3e1061d442f68f1126d5f7775bc1f431f1a4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `BuildAutosave` / `AutosaveShouldQueue` / `AutosaveClearHintFlags` | `LS_Autosave` **3155–3163** |
| `NextSkillFromArg` | `LS_ChangeSkill` **3165–3176** |
| `NoiseAlertTarget` / `NoiseAlertEmitter` | `LS_NoiseAlert` **3178–3209** |
| `CommunicatorRejectBackside` / `CommunicatorRequiresInventory` / `BuildCommunicator` | `LS_SendToCommunicator` **3211–3247** |
| `CommunicatorSoundName` / `CommunicatorSubtitleKey` | **3220–3242** |
| `BuildForceField` / damage/thrust helpers | `LS_ForceField` **3249–3258** |
| sector tag loop seam | `LS_ClearForceField` **3260–3275** |
| `BuildGlassBreak` / `GlassBreakStripBlocking` / `GlassBreakSpawnPoint` | `LS_GlassBreak` **3277–3304** |
| `GlassBreakAnyTextureChanged` / `GlassBreakShouldSpawnJunk` | **3284–3297** |

### Chunk **932** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**931** |


**Post-review ratio:** 147/150 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecAutosaveCommunicator932SliceTests.BuildAutosave_QueuesWhenNotSaving`
- `VmPLnspecAutosaveCommunicator932SliceTests.BuildAutosave_SkipsDuringSaveGame`
- `VmPLnspecAutosaveCommunicator932SliceTests.NextSkillFromArg_ClampsPastSkillCount`
- `VmPLnspecAutosaveCommunicator932SliceTests.NoiseAlertEmitter_ReusesTargetWhenTidsMatch`
- `VmPLnspecAutosaveCommunicator932SliceTests.BuildCommunicator_RejectsBacksideWhenFrontOnly`
- `VmPLnspecAutosaveCommunicator932SliceTests.BuildCommunicator_FormatsSoundAndSubtitle`
- `VmPLnspecAutosaveCommunicator932SliceTests.BuildGlassBreak_OffsetsSpawnAlongNormal`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 932 --to 932` (0 hard / 0 soft).

## Seams

`P_NoiseAlert`, `P_DamageMobj`, `P_ChangeSwitchTexture`, `Spawn`, `Net_WriteInt8`, and sector iterators remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmPLnspecAutosaveCommunicator932Slice.cs` without duplicating SetPlayerProperty credit.
