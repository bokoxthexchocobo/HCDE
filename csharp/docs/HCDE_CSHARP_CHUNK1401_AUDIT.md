# Chunk 1401 — Windows, Linux, and macOS theme detection from system_theme.cpp (1-162).

## Scope

Windows, Linux, and macOS theme detection from system_theme.cpp (1-162).

## Native ranges

- `src/utility/system_theme.cpp` **GetSystemTheme** 1–162 (162 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmSystemTheme1401Slice.cs`
Managed LOC (tally): 133.
Conversion commit: `ee4e22bdc924d54c77403fc56bf51ee8784f3e0f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| pipe_read and gdbus_read (**68–90**) | yes |
| GetSystemTheme (**100–162**) | yes |
| Band hull (**1–162**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| RegGetValueW | callee |
| SystemParametersInfoW | callee |
| popen and fgets | the pipe text is supplied |
| CoreFoundation preference copy | the preference result is supplied |

The preprocessor keeps one platform. The port runs the Windows, Linux, or macOS branch the caller selects, and the other branch returns the default theme. Windows reads AppsUseLightTheme and treats 0 as dark. Any other successful dword is light. A failed read leaves the theme at the default. High contrast is OR-ed only when the contrast call succeeds and the on flag is set. Linux runs the portal read for color-scheme and then contrast. The text uint32 1 is dark and wins over uint32 2. Contrast uint32 1 ORs the high-contrast flag. A closed pipe returns an empty string and is not closed. macOS sets dark only when AppleInterfaceStyle compares equal to Dark, then ORs high contrast from the boolean. A present preference is released after the type check. A missing preference is not released.

| Managed (`VmSystemTheme1401Slice.cs`) | Native (`system_theme.cpp`) |
|---------------------------------------|-----------------------------|
| Theme detection | **1–162** |
| Band hull (exclusive slice) | **1–162** |

### Chunk **1401** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **162** |
| **Net headline** | **+162** vs the prior chunk |


**Post-review ratio:** 133/162 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmSystemTheme1401SliceTests.WindowsAndLinuxThemeFollowTheRegistryAndThePortalText`
- `VmSystemTheme1401SliceTests.MacReleasesPreferencesAndOtherPlatformsStayDefault`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1401 --to 1401` (0 hard / 0 soft).

## Seams

The file is the whole translation unit. Registry, gdbus, and CoreFoundation stay callees. The file ends at line 162.

## Credit rationale

UNCredited system_theme.cpp 1-162 (+162). After chunk 1400 this is the unique largest hole. StackFrameStateNode.cpp 1-161 is 161. The band is shorter than 400 lines and is not sliced. It runs through end of file.
