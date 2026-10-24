# Chunk 639 — HCDE server runtime init from args (`hcde_servermode.cpp` 67–217)

## Scope

HCDE server runtime init from args (`hcde_servermode.cpp` 67–217)

## Native ranges

- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_InitFromArgs** 67–217 (151 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmHcdeServerMode.cs`
Managed LOC (tally): 169.
Conversion commit: `62d9e01d48d87d37c22bcc4551698c0d7c038e49`.

## Padding review (2026-10-21)

| Managed | Native (`hcde_servermode.cpp`) |
|---------|-------------------------------|
| `IsValueArgNative` … `ReadWarpValueNative` | 67–118 |
| `HcdeServerModeInitFromArgsNative` | `HCDE_ServerMode_InitFromArgs` 131–172 |
| `HcdeServerModeSetSelectedIwadNative` … `HcdeServerModeSetAuthorityStateNative` | 174–217 |

**Post-review ratio:** 169/151 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmHcdeServerModeTests.HcdeServerModeInitFromArgsNative_ParsesServerFlag`
- `VmHcdeServerModeTests.HcdeServerModeShouldSuppressRoomUiNative_DedicatedServer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 639 --to 639` (0 hard / 0 soft).

## Seams

`Args`, `DebugTrace`, and `I_FatalError` are injected via `HcdeServerModeStateContext`.

## Credit rationale

Argument helpers and init/network/authority state setters through `HCDE_ServerMode_SetAuthorityState`.
