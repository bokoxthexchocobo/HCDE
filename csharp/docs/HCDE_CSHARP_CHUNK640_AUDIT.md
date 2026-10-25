# Chunk 640 — HCDE server mode queries and UI flags (`hcde_servermode.cpp` 219–395) Merged: [639].

## Scope

HCDE server mode queries and UI flags (`hcde_servermode.cpp` 219–395) Merged: [639].

## Native ranges

- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 219–225 (7 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 227–233 (7 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 235–315 (81 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 317–327 (11 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 329–333 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 335–339 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 341–345 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 347–351 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 353–357 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 359–363 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 365–369 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 371–375 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 377–381 (5 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 383–389 (7 lines)
- `src/common/engine/hcde_servermode.cpp` **HCDE_ServerMode_SetAuthoritySettingsController** 391–395 (5 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmHcdeServerModeAccessors.cs`, `VmHcdeServerMode.cs`
Managed LOC (tally): 272.
Conversion commit: `d81cac6de889537f5233e9f3247b0b02c5782a07`.

## Padding review (2026-10-21)

| Managed | Native (`hcde_servermode.cpp`) |
|---------|-------------------------------|
| `HcdeServerModeSetAuthoritySettingsControllerNative` … `HcdeServerModePrintDiagnosticsNative` | 219–315 |
| `HcdeServerModeGuardClientSubsystemNative` … `HcdeServerModeShouldSuppressStartupUiNative` | 317–395 |

<!-- merged from chunk639 -->
| Managed | Native (`hcde_servermode.cpp`) |
|---------|-------------------------------|
| `IsValueArgNative` … `ReadWarpValueNative` | 67–118 |
| `HcdeServerModeInitFromArgsNative` | `HCDE_ServerMode_InitFromArgs` 131–172 |
| `HcdeServerModeSetSelectedIwadNative` … `HcdeServerModeSetAuthorityStateNative` | 174–217 |

### Out-of-body uncredit (640)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **13** |
| **Honest** | **163** |


**Post-review ratio:** 272/163 managed/native ≈ 167% (src file LOC at HEAD; see tally).

## Tests

- `VmHcdeServerModeTests.HcdeServerModeShouldSuppressRoomUiNative_DedicatedServer`
- `VmHcdeServerModeTests.HcdeServerModeInitFromArgsNative_ParsesServerFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 640 --to 640` (0 hard / 0 soft).

## Seams

Printf/diagnostics blocks are injected; `HcdeServerModeStateContext` is uncredited in `VmHcdeServerMode.cs`.

## Credit rationale

Authority toggles, diagnostics, guards, and public `HCDE_ServerMode_Is*` accessors.
