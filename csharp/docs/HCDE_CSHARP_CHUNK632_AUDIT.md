# Chunk 632 — Host session loops, game port, verification decode, guest disconnect (`i_net.cpp` 3545–3717)

## Scope

Host session loops, game port, verification decode, guest disconnect (`i_net.cpp` 3545–3717)

## Native ranges

- `src/common/engine/i_net.cpp` **Guest_ContactHost** 3545–3717 (173 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmINetHostGameFinish.cs`
Managed LOC (tally): 157.
Conversion commit: `615543b0bfbf68aabb58b7990ccd0282a4b4c6b1`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HostGameFinishSessionNative` | `HostGame` 3545–3594 |
| `IGetGamePortNative` | `I_GetGamePort` 3596–3599 |
| `ReadVerificationErrorNative` | `ReadVerificationError` 3601–3682 |
| `GuestContactHostDisconnectNative` | `Guest_ContactHost` 3684–3717 |

**Post-review ratio:** 157/173 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmINetHostGameFinishTests.IGetGamePortNative_ReturnsPort`
- `VmINetHostGameFinishTests.ReadVerificationErrorNative_EngineMismatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 632 --to 632` (0 hard / 0 soft).

## Seams

`I_NetLoop`, `SendAbort`, `SV_ShutdownMasters`, and file-system hash maps are injected.

## Credit rationale

Whole `HostGame` tail, `I_GetGamePort`, `ReadVerificationError`, and opening `Guest_ContactHost` disconnect branch.
