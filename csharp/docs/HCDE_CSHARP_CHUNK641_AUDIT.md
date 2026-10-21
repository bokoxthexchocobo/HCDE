# Chunk 641 — Early net CVars, protocol constants, and global connection state (`i_net.cpp` 82–447)

## Scope

Early net CVars, protocol constants, and global connection state (`i_net.cpp` 82–447)

## Native ranges

- `src/common/engine/i_net.cpp` **sv_maxplayers** 82–447 (366 lines)

**Credited native LOC:** 366 (tally `nativeLoc` 366).

## Managed

`VmINetBootstrap.cs`
Managed LOC (tally): 232.
Conversion commit: `69ed2b0b75f0782d217a44674b74383b1be468a9`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `ClampSvMaxPlayersNative` | `sv_maxplayers` CUSTOM_CVAR 90–100 |
| Protocol / pregame constants | 168–216 |
| `PRE_*` / `HCDE_CONNECT_*` / status enums | 218–289 |
| `ClearConnectionFieldsNative` | `FConnection::Clear` 367–394 |
| `ResetGlobalNetStateNative` | globals 404–444 |

**Post-review ratio:** 232/366 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmINetBootstrapTests.ClampSvMaxPlayersNative_ClampsHighAndLow`
- `VmINetBootstrapTests.ResetGlobalNetStateNative_RestoresDefaults`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 641 --to 641` (0 hard / 0 soft).

## Seams

FARG metadata and platform socket typedefs are not hosted.

## Credit rationale

Whole-file preamble through transport globals before anonymous-namespace helpers at 448.
