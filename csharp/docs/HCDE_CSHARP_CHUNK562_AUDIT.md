# Chunk 562 — Non-overlapping d_net late-join/resync/cutscene helpers (4507–4621) and level-start authority helpers (4993–5579)

## Scope

Non-overlapping d_net late-join/resync/cutscene helpers (4507–4621) and level-start authority helpers (4993–5579)

## Native ranges

- `src/d_net.cpp` **Net_ClearLateJoinSyncPending** 4507–4523 (17 lines)
- `src/d_net.cpp` **Net_RequestRuntimeResync** 4549–4571 (23 lines)
- `src/d_net.cpp` **Net_GetCutsceneReadyHost** 4573–4586 (14 lines)
- `src/d_net.cpp` **Net_IsPlayerReady** 4588–4607 (20 lines)
- `src/d_net.cpp` **Net_CountCutsceneReadyHumanParticipants** 4610–4621 (12 lines)
- `src/d_net.cpp` **Net_SetWaiting** 4993–5006 (14 lines)
- `src/d_net.cpp` **HCDESelectNextServiceAuthoritySlot** 5469–5496 (28 lines)
- `src/d_net.cpp` **IsMapLoaded** 5540–5543 (4 lines)
- `src/d_net.cpp` **LevelStartPlayableMask** 5545–5554 (10 lines)
- `src/d_net.cpp` **Net_AuthorityLevelStartStatusAfterRelease** 5560–5563 (4 lines)
- `src/d_net.cpp` **Net_PlayableNetworkClientCount** 5565–5574 (10 lines)
- `src/d_net.cpp` **Net_HasPlayableNetworkClients** 5576–5579 (4 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmDNetLateJoinCutscene.cs`, `VmDNetLevelStartAuthority.cs`
Managed LOC (tally): 271.
Conversion commit: `508694680c2b0cff768b64dc61fe52f97eae2440`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ClearLateJoinSyncPendingNative` | `Net_ClearLateJoinSyncPending` 4507–4523 |
| `RequestRuntimeResyncNative` | `Net_RequestRuntimeResync` 4549–4571 |
| `GetCutsceneReadyHostNative` | `Net_GetCutsceneReadyHost` 4573–4586 |
| `IsPlayerReadyNative` | `Net_IsPlayerReady` 4588–4607 |
| `CountCutsceneReadyHumanParticipantsNative` | `Net_CountCutsceneReadyHumanParticipants` 4610–4621 |
| `SetWaitingNative` | `Net_SetWaiting` 4993–5006 |
| `SelectNextServiceAuthoritySlotNative` | `HCDESelectNextServiceAuthoritySlot` 5469–5496 |
| `IsMapLoadedNative` | `IsMapLoaded` 5540–5543 |
| `LevelStartPlayableMaskNative` | `LevelStartPlayableMask` 5545–5554 |
| `AuthorityLevelStartStatusAfterReleaseNative` | `Net_AuthorityLevelStartStatusAfterRelease` 5560–5563 |
| `PlayableNetworkClientCountNative` | `Net_PlayableNetworkClientCount` 5565–5574 |
| `HasPlayableNetworkClientsNative` | `Net_HasPlayableNetworkClients` 5576–5579 |

**Post-review ratio:** 271/160 managed/native ≈ 169% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetLateJoinCutsceneTests.ClearLateJoinSyncPendingNative_SetsConsistencyGrace`
- `VmDNetLateJoinCutsceneTests.RequestRuntimeResyncNative_SetsResendFromAck`
- `VmDNetLateJoinCutsceneTests.GetCutsceneReadyHostNative_PrefersAuthority`
- `VmDNetLateJoinCutsceneTests.PlayerReadiedUpToggleNative_ForcesReadyWhenSoloHuman`
- `VmDNetLevelStartAuthorityTests.SetWaitingNative_ResetsLevelStartWhenNetGame`
- `VmDNetLevelStartAuthorityTests.SelectNextServiceAuthoritySlotNative_SkipsReserved`
- `VmDNetLevelStartAuthorityTests.IsMapLoadedNative_MatchesGameState`
- `VmDNetLevelStartAuthorityTests.LevelStartPlayableMaskNative_ExcludesAuthorityAndLateJoin`
- `VmDNetLevelStartAuthorityTests.AuthorityLevelStartStatusAfterReleaseNative_ReturnsReady`
- `VmDNetLevelStartAuthorityTests.PlayableNetworkClientCountNative_ExcludesAuthority`
- `VmDNetLevelStartAuthorityTests.HasPlayableNetworkClientsNative_MatchesCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 562 --to 562` (0 hard / 0 soft).

## Seams

`HCDEBeginActorBaselineRepair`, `DebugTrace`, live `ClientStates`, `CutsceneReady` mutation, `Printf`, and `HSendPacket` are not hosted.

## Credit rationale

Whole-function ports for credited natives; overlapping `Net_SetLateJoinSyncPending`, `Net_BeginRuntimeBootstrap`, and `Net_PlayerReadiedUp` bands remain in chunks 13/12.
