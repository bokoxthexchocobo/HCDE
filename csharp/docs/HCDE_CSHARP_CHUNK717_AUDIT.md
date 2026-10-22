# Chunk 717 — Whole `P_CheckMusicChange`, `P_CheckEnvironment`, `P_CheckUse`, `FSafePosition::{Update,IsSafe,Serialize}`, and `P_PlayerThink` (`p_user.cpp` 1682–1912).

## Scope

Whole `P_CheckMusicChange`, `P_CheckEnvironment`, `P_CheckUse`, `FSafePosition::{Update,IsSafe,Serialize}`, and `P_PlayerThink` (`p_user.cpp` 1682–1912).

## Native ranges

- `src/playsim/p_user.cpp` **P_CheckMusicChange** 1682–1708 (27 lines)
- `src/playsim/p_user.cpp` **P_CheckEnvironment** 1723–1735 (13 lines)
- `src/playsim/p_user.cpp` **P_CheckUse** 1750–1776 (27 lines)
- `src/playsim/p_user.cpp` **FSafePosition::Update** 1785–1797 (13 lines)
- `src/playsim/p_user.cpp` **FSafePosition::IsSafe** 1799–1806 (8 lines)
- `src/playsim/p_user.cpp` **FSafePosition::Serialize** 1808–1822 (15 lines)
- `src/playsim/p_user.cpp` **P_PlayerThink** 1830–1912 (83 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmPUserPlayerThink.cs`
Managed LOC (tally): 132.
Conversion commit: `004b628a8468a1b7a1c20443ebed693913a47a13`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `MusicInfoTickActive` / `MusicInfoUsesLookup` / `MusicCdTrackChange` / `ShouldSwapMusic` | `P_CheckMusicChange` 1682–1708 |
| `EnvironmentFallingScream` / `EnvironmentTriggersFx` | `P_CheckEnvironment` 1723–1735 |
| `CheckUseSkipsWhenPredicting` / `CheckUseEdge` / `CheckUseRelease` / `UseHoldButton` | `P_CheckUse` 1750–1776 |
| `SafePosLiveUpdate` / `SafePosValidOnFloor` / `SafePosForceCopy` / `SafePosHeightClear` / `SafePosTracksLive` | `FSafePosition::Update` 1785–1797; `IsSafe` 1799–1806 |
| `ApplyAngleOffset` / `ProcessAngleSlot` / `SubtitleTick` / `DecrementSubtitle` / `UpdateSafePosWhenLive` / `SkipBobTimerWhenPredicting` / `BotFrozenSkip` / `DebugLogTick` / `StripPresentationCheats` / `BobWeapon2D` / `BobWeapon3D` / `PlayerThinkVelocitySlot` / `WeaponBobBranch` / `LevelVelocitySlotValid` | `P_PlayerThink` 1830–1912 |


**Post-review ratio:** 132/186 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserPlayerThinkTests.MusicInfo_UsesLookupWhenArgNonZero`
- `VmPUserPlayerThinkTests.Environment_TriggersFallingBand`
- `VmPUserPlayerThinkTests.CheckUse_SkipsDuringPrediction`
- `VmPUserPlayerThinkTests.PlayerThink_SkipsBobWhenPredicting`
- `VmPUserPlayerThinkTests.PlayerThink_BotFrozenReturnsEarly`
- `VmPUserPlayerThinkTests.PlayerThink_RecordsVelocityOffline`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 717 --to 717` (0 hard / 0 soft).

## Seams

`S_ChangeMusic`, `P_UseLines`, `P_TalkFacing`, VM `PlayerThink`, weapon bob natives, and serializer I/O remain native.

## Credit rationale

Player think band credited as conversion (was recognition-only padding in chunk512 audit).
