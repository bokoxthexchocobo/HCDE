# Chunk 630 — Pregame host connection loop (`i_net.cpp` 3054–3364)

## Scope

Pregame host connection loop (`i_net.cpp` 3054–3364)

## Native ranges

- `src/common/engine/i_net.cpp` **Host_CheckForConnections** 3054–3364 (311 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmINetHostCheckConnections.cs`
Managed LOC (tally): 347.
Conversion commit: `a7ed3085f545fbd75e7f23426e401c9fa919788e`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `HostCheckForConnectionsNative` | `Host_CheckForConnections` 3054–3364 |

**Post-review ratio:** 347/311 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmINetHostCheckConnectionsTests.HostCheckForConnectionsNative_ReturnsReadyWhenFull`
- `VmINetHostCheckConnectionsTests.HostCheckForConnectionsNative_RemovesKickedClient`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 630 --to 630` (0 hard / 0 soft).

## Seams

`GetConnection`, `TryProcessSetupConnectPacket`, kick/ban UI lists, and master update are injected.

## Credit rationale

Whole `Host_CheckForConnections`.
