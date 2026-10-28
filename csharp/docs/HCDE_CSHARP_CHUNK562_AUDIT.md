# Chunk 562 — Non-overlapping d_net late-join/resync/cutscene helpers (4507–4621) and level-start authority helpers (4993–5579) Retired (2026-10-24): `--band` hull overlaps existing credit (d-net-reset-commands-chunk12 src/d_net.cpp 4623-4660).

## Scope

Non-overlapping d_net late-join/resync/cutscene helpers (4507–4621) and level-start authority helpers (4993–5579) Retired (2026-10-24): `--band` hull overlaps existing credit (d-net-reset-commands-chunk12 src/d_net.cpp 4623-4660).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8333d0412dcbeea67c897df190f7d767e2f31761`.

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

### Out-of-body uncredit (562)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 562 --to 562` (0 hard / 0 soft).

## Seams

`HCDEBeginActorBaselineRepair`, `DebugTrace`, live `ClientStates`, `CutsceneReady` mutation, `Printf`, and `HSendPacket` are not hosted.

## Credit rationale

Whole-function ports for credited natives; overlapping `Net_SetLateJoinSyncPending`, `Net_BeginRuntimeBootstrap`, and `Net_PlayerReadiedUp` bands remain in chunks 13/12.
