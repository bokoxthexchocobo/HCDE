# Chunk 642 — Master host parsing and NMS1 text/TTL helpers (`sv_master.cpp` 121–282)

## Scope

Master host parsing and NMS1 text/TTL helpers (`sv_master.cpp` 121–282)

## Native ranges

- `src/common/engine/sv_master.cpp` **IsMasterName** 121–282 (162 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmSvMasterResolve.cs`
Managed LOC (tally): 140.
Conversion commit: `9a6b2e939b1a456c363321c5addb5cf77ae849bb`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `IsMasterNameNative` | 121–130 |
| `TryParseMasterEndpointNative` | 132–150 |
| `GetAdvertisedGamePortNative` | 180–185 |
| `SameMasterAddressNative` | 187–192 |
| `ResetNms1RegistrationNative` | 194–202 |
| `NextNms1RequestIdNative` / `MakeNms1TextNative` | 245–282 |

**Post-review ratio:** 140/162 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterResolveTests.IsMasterNameNative_DistinguishesIpFromHostname`
- `VmSvMasterResolveTests.MakeNms1TextNative_UsesFallbackWhenEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 642 --to 642` (0 hard / 0 soft).

## Seams

`getaddrinfo`, `socket`, and `sendto` are injected callbacks in future chunks.

## Credit rationale

Whole static helpers from `IsMasterName` through `MakeNms1Text`.
