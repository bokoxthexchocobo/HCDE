# Chunk 634 — Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Scope

Guest HCDE services through `JoinGame` (`i_net.cpp` 3951–4260)

## Native ranges

- `src/common/engine/i_net.cpp` **JoinGame** 3951–4260 (310 lines)

**Credited native LOC:** 310 (tally `nativeLoc` 310).

## Managed

`VmINetGuestContactHost.cs`, `VmINetJoinGame.cs`
Managed LOC (tally): 401.
Conversion commit: `7782c60d590c5b701d63563ba6c3c7dd3c40d9a3`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GuestContactHostServicesNative` | `Guest_ContactHost` 3951–4179 |
| `JoinGameNative` | `JoinGame` 4182–4260 |

**Post-review ratio:** 401/310 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGuestContactHostTests.GuestContactHostServicesNative_IgnoresUnnegotiatedService`
- `VmINetJoinGameTests.JoinGameNative_FailsWithoutHostArg`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 634 --to 634` (0 hard / 0 soft).

## Seams

Reliable services, `Net_Read*` helpers, and `I_NetLoop` are injected. `JoinGameContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

`Guest_ContactHost` service switch tail and whole `JoinGame`.
