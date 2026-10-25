# Chunk 633 — Guest setup replies through early HCDE services (`i_net.cpp` 3718–3950)

## Scope

Guest setup replies through early HCDE services (`i_net.cpp` 3718–3950)

## Native ranges

- `src/common/engine/i_net.cpp` **Guest_ContactHost** 3718–3786 (69 lines)
- `src/common/engine/i_net.cpp` **Guest_ContactHost** 3787–3838 (52 lines)
- `src/common/engine/i_net.cpp` **Guest_ContactHost** 3840–3950 (111 lines)

**Credited native LOC:** 232 (tally `nativeLoc` 232).

## Managed

`VmINetGuestContactReject.cs`
Managed LOC (tally): 213.
Conversion commit: `e7591d867d552122210129db0f34d3b9a4a17f53`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GuestContactHostRejectionsNative` | `Guest_ContactHost` `PRE_FULL` … `PRE_SETUP_TIMEOUT` 3718–3786 |
| `GuestContactHostRejectionsNative` | `PRE_IN_PROGRESS` / dedicated late-join retry 3722–3756 |
| `GuestContactHostRejectionsNative` | `PRE_CONNECT_ACK` 3787–3838 |
| `GuestContactHostRejectionsNative` | `PRE_HCDE_SERVICE` through `HPS_GAME_INFO` guard 3840–3950 |

### Out-of-body uncredit (633)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **232** |
| **Out-of-body removed** | **0** |
| **Honest** | **232** |


**Post-review ratio:** 213/232 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGuestContactHostTests.GuestContactHostRejectionsNative_RejectsFull`
- `VmINetGuestContactHostTests.GuestContactHostRejectionsNative_DedicatedLateJoinRetry`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 633 --to 633` (0 hard / 0 soft).

## Seams

`GetConnection`, `CheckHCDEPregameService`, reliable commit helpers, and `ReadVerificationError` are injected. `GuestContactHostContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

Single `GuestContactHostRejectionsNative` mirrors the native `else if` chain for setup replies and the opening `PRE_HCDE_SERVICE` switch through `HPS_GAME_INFO` (ends at native line 3950).
