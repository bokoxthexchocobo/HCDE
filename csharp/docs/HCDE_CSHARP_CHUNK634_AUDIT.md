# Chunk 634 — Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Scope

Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Native ranges

- `src/common/engine/i_net.cpp` **JoinGame** 3951–4087 (137 lines)
- `src/common/engine/i_net.cpp` **JoinGame** 4090–4177 (88 lines)
- `src/common/engine/i_net.cpp` **JoinGame** 4182–4260 (79 lines)

**Credited native LOC:** 304 (tally `nativeLoc` 304).

## Managed

`VmINetGuestContactHost.cs`, `VmINetJoinGame.cs`
Managed LOC (tally): 303.
Conversion commit: `ba66ecd5c1888f9b80922bf8505a22c78b7fde49`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HandleGameInfoService` / `GameInfoPayloadSizes` / `ClampGameInfoTicDup` | `HPS_GAME_INFO` 3951–3968 |
| `TryHandleRosterService` / `RosterPacketLooksValid` / `ValidateRosterClientId` / `ReadBigEndianUInt16` | roster 3970–4047 |
| `TryHandleBootstrapBeginService` | bootstrap 4049–4067 |
| `TryHandleStartGameService` | start 4068–4081 |
| `RunGuestSetupWatchdog` / `SendGuestOutboundSetup` | setup watchdog + outbound 4090–4177 |
| `JoinArgMissing` / `PromoteConnectedClientsToReady` / `JoinGameNative` | `JoinGame` 4182–4260 |

### Out-of-body uncredit (634)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **304** |
| **Out-of-body removed** | **0** |
| **Honest** | **304** |


**Post-review ratio:** 303/304 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGuestContactHostTests.GuestContactHostServicesNative_IgnoresUnnegotiatedService`
- `VmINetJoinGameTests.JoinGameNative_FailsWithoutHostArg`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 634 --to 634` (0 hard / 0 soft).

## Seams

Reliable services, `Net_Read*` helpers, and `I_NetLoop` are injected. `JoinGameContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

`Guest_ContactHost` service switch tail and whole `JoinGame`.
