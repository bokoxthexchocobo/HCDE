# Chunk 1378 — Win32 base video construct, display-device name, and adapter dump from win32basevideo.cpp (1-204).

## Scope

Win32 base video construct, display-device name, and adapter dump from win32basevideo.cpp (1-204).

## Native ranges

- `src/common/platform/win32/win32basevideo.cpp` **Win32BaseVideo::Win32BaseVideo** 1–204 (204 lines)

**Credited native LOC:** 204 (tally `nativeLoc` 204).

## Managed

`VmWin32BaseVideo1378Slice.cs`
Managed LOC (tally): 152.
Conversion commit: `2304073837847cc0ffc82043e9b6730382c86907`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Preamble and includes (**1–41**) | yes |
| `vid_adapter` cvar (**42–42**) | yes |
| `Win32BaseVideo` construct (**50–55**) | yes |
| `GetPrimaryMonitorHandle` (**63–67**) | yes |
| Enum state and device callback (**69–96**) | yes |
| `GetDisplayDeviceName` (**104–139**) | yes |
| Dump state and callback (**147–188**) | yes |
| `DumpAdapters` (**196–204**) | yes |
| Band hull (**1–204**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `win32basevideo.h` | declaration only |
| `ShowGameView` and the monitor APIs | callees |

`vid_adapter` is an int cvar named `vid_adapter`, default 0, flags `CVAR_ARCHIVE` (1) and `CVAR_GLOBALCONFIG` (1<<10). The construct shows the game view, then resolves the display device. Adapter 0 calls `MonitorFromPoint` at (0, 0) with `MONITOR_DEFAULTTOPRIMARY` and does not enumerate. Any other adapter starts the index at 1, calls `GetMonitorInfo` and ignores the result, keeps the first monitor because the found handle starts null, overwrites that handle when the index equals `vid_adapter`, and always returns TRUE so the walk continues. The cvar is read on each callback. A null found handle leaves the name null and the monitor 0. `GetMonitorInfoA` failure does the same, discarding the handle. Success copies `szDevice` through the first NUL into a 32-byte buffer and publishes that buffer. A device of 32 or more characters fails the copy. Dump captures the name pointer before enumeration. A failed query stays bold with an empty detail. A null name bolds only the primary monitor. A non-null name bolds only an ordinal device match, including the empty string. The detail is ` [%ldx%ld @ (%ld,%ld)]` plus ` (Primary)` when `MONITORINFOF_PRIMARY` is set, truncated to 63 characters. `TEXTCOLOR_BOLD` is U+001C `+`.

| Managed (`VmWin32BaseVideo1378Slice.cs`) | Native (`win32basevideo.cpp`) |
|------------------------------------------|------------------------------|
| Construct, device name, and adapter dump | **1–204** |
| Band hull (exclusive slice) | **1–204** |

### Chunk **1378** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **204** |
| **Net headline** | **+204** vs the prior chunk |


**Post-review ratio:** 152/204 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmWin32BaseVideo1378SliceTests.ConstructSelectsPrimaryOrEnumeratedAdapter`
- `VmWin32BaseVideo1378SliceTests.DumpAdaptersMarksTheSelectedMonitor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1378 --to 1378` (0 hard / 0 soft).

## Seams

ShowGameView, MonitorFromPoint, GetMonitorInfo, GetMonitorInfoA, and EnumDisplayMonitors are Win32 callees. vid_adapter is CVAR_Int default 0 with CVAR_ARCHIVE | CVAR_GLOBALCONFIG. MONITOR_DEFAULTTOPRIMARY is 1. MONITORINFOF_PRIMARY is 1. CCHDEVICENAME is 32. TEXTCOLOR_BOLD is U+001C plus. The header is not credited.

## Credit rationale

UNCredited win32basevideo.cpp 1-204 (+204). After chunk 1377 this ties codegen.cpp 9936-10139 and d_net.cpp 4992-5195. Path order takes this file. Gaps longer than 400 lines stay whole and are not sliced.
