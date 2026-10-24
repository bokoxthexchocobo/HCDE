# Chunk 634 — Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Scope

Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Native ranges

- `src/common/engine/i_net.cpp` **JoinGame** 3951–4260 (310 lines)

**Credited native LOC:** 310 (tally `nativeLoc` 310).

## Managed

`VmINetGuestContactHost.cs`, `VmINetJoinGame.cs`
Managed LOC (tally): 193.
Conversion commit: `5627169b4f9d58abf506e730f2d23bc8f8d290f5`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GuestContactHostServicesNative` | `HPS_GAME_INFO` body, roster, bootstrap, start 3951–4087 |
| `GuestContactHostServicesNative` | setup watchdog, flush, outbound setup packets 4090–4177 |
| `JoinGameNative` | `JoinGame` 4182–4260 |

**Post-review ratio:** 193/310 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGuestContactHostTests.GuestContactHostServicesNative_IgnoresUnnegotiatedService`
- `VmINetJoinGameTests.JoinGameNative_FailsWithoutHostArg`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 634 --to 634` (0 hard / 0 soft).

## Seams

Reliable services, `Net_Read*` helpers, and `I_NetLoop` are injected. `JoinGameContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

`Guest_ContactHost` service switch tail and whole `JoinGame`.
