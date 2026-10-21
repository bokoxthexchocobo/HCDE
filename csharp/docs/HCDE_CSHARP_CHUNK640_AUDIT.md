# Chunk 640 — HCDE server mode queries and UI flags (`hcde_servermode.cpp` 219–395)

## Scope

HCDE server mode queries and UI flags (`hcde_servermode.cpp` 219–395)

## Native ranges

- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 219–395 (177 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`VmHcdeServerModeAccessors.cs`
Managed LOC (tally): 103.
Conversion commit: `c3dd07056cf8eda2b875d041aced707c79701a69`.

## Padding review (2026-10-21)

| Managed | Native (`hcde_servermode.cpp`) |
|---------|-------------------------------|
| `HcdeServerModeSetAuthoritySettingsControllerNative` … `HcdeServerModePrintDiagnosticsNative` | 219–315 |
| `HcdeServerModeGuardClientSubsystemNative` … `HcdeServerModeShouldSuppressStartupUiNative` | 317–395 |

**Post-review ratio:** 103/177 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmHcdeServerModeTests.HcdeServerModeShouldSuppressRoomUiNative_DedicatedServer`
- `VmHcdeServerModeTests.HcdeServerModeInitFromArgsNative_ParsesServerFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 640 --to 640` (0 hard / 0 soft).

## Seams

Printf/diagnostics blocks are injected; `HcdeServerModeStateContext` is uncredited in `VmHcdeServerMode.cs`.

## Credit rationale

Authority toggles, diagnostics, guards, and public `HCDE_ServerMode_Is*` accessors.
