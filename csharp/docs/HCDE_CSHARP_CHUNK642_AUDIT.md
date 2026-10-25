# Chunk 642 — Master host parsing and NMS1 helper utilities (`sv_master.cpp` 121–282) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Master host parsing and NMS1 helper utilities (`sv_master.cpp` 121–282) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3b9c83c45cf033e9c982433ea53d2836a3e81f8f`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `IsMasterNameNative` | 121–130 |
| `ResolveMasterAddressNative` | 132–169 |
| `CreateMasterSocketNative` | 171–178 |
| `GetAdvertisedGamePortNative` | 180–185 |
| `SameMasterAddressNative` | 187–192 |
| `ResetNms1RegistrationNative` | 194–202 |
| `ClearNms1ErrorNative` / `SetNms1ErrorNative` | 204–228 |
| `ElapsedSecondsSinceNative` / `RemainingNms1TtlNative` | 230–243 |
| `NextNms1RequestIdNative` | 245–251 |
| `MakeNms1TextNative` | 253–282 |

### Out-of-body uncredit (642)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **153** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 642 --to 642` (0 hard / 0 soft).

## Seams

`getaddrinfo`, `socket`, and `snprintf` are host callbacks.

## Credit rationale

Whole static helpers from `IsMasterName` through `MakeNms1Text`.
