# Chunk 641 — Early net CVars, protocol constants, and connection state (`i_net.cpp` selected preamble bands) Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Scope

Early net CVars, protocol constants, and connection state (`i_net.cpp` selected preamble bands) Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `b2b9e82ae7f3970cadec04a732f7b44e28e73940`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `ClampSvMaxPlayersNative` | 90–100 |
| Protocol / pregame constants | 168–216 |
| `PRE_*` / `HCDE_CONNECT_*` / status enums | 218–289 |
| `ClearConnectionFieldsNative` | 367–394 |
| `ResetGlobalNetStateNative` | 404–444 |

### Out-of-body uncredit (641)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **201** |
| **Out-of-body removed** | **162** |
| **Honest** | **39** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 641 --to 641` (0 hard / 0 soft).

## Seams

FARG metadata and platform typedefs are not hosted.

## Credit rationale

Mirrors credited preamble bands with managed counterparts (not whole TU).
