# Chunk 633 — Guest setup replies through connect ack (`i_net.cpp` 3718–3950)

## Scope

Guest setup replies through connect ack (`i_net.cpp` 3718–3950)

## Native ranges

- `src/common/engine/i_net.cpp` **Guest_ContactHost** 3718–3950 (233 lines)

**Credited native LOC:** 233 (tally `nativeLoc` 233).

## Managed

`VmINetGuestContactHost.cs`
Managed LOC (tally): 356.
Conversion commit: `91da045b718680031e6af3d71c49f5687109e559`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `GuestContactHostRejectionsNative` | `Guest_ContactHost` 3718–3950 |

**Post-review ratio:** 356/233 managed/native ≈ 153% (src file LOC at HEAD; see tally).

## Tests

- `VmINetGuestContactHostTests.GuestContactHostRejectionsNative_RejectsFull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 633 --to 633` (0 hard / 0 soft).

## Seams

`GetConnection`, reliable services, and `ReadVerificationError` are injected. `GuestContactHostContext` is uncredited in `VmINetQueryModels.cs`.

## Credit rationale

`Guest_ContactHost` rejection and `PRE_CONNECT_ACK` handling through early HCDE services.
