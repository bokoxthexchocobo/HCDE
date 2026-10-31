# Chunk 1367 — strbin1, ExpandEnvVars, and NicePath from cmdlib.cpp (738-970).

## Scope

strbin1, ExpandEnvVars, and NicePath from cmdlib.cpp (738-970).

## Native ranges

- `src/common/utility/cmdlib.cpp` **CmdLib** 738–970 (233 lines)

**Credited native LOC:** 233 (tally `nativeLoc` 233).

## Managed

`VmCmdLib1367Slice.cs`
Managed LOC (tally): 254.
Conversion commit: `ee8cda67fa18bc4a80052f9aab6f7a93f1e3ac40`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Blank line and the `strbin1` banner (**738–747**) | yes |
| `strbin1` (**748–839**) | yes |
| `ExpandEnvVars` banner (**840–849**) | yes |
| `ExpandEnvVars` (**850–903**) | yes |
| `NicePath` banner (**904–914**) | yes |
| `NicePath` (**915–962**) | yes |
| Trailing banner (**963–970**) | yes |
| Band hull (**738–970**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| cmdlib.cpp **646–737** | `strbin`, already chunk 122 |
| cmdlib.cpp **971–979** | `IsAbsPath`, already chunk 122 |
| `getenv`, `getpwuid`, `getpwnam` | host lookups stay injected |

`strbin1` copies bytes until a NUL. A trailing backslash is dropped. `\\` stores one backslash. `\n` through `\v`, `\a`, `\b`, `\f`, and `\?` store those controls. `\c` stores byte 0x1C. A backslash followed by a real newline stores nothing. An unknown letter stores that letter. Hex and octal accumulate in an 8-bit `char` and keep at most two extra digits. A non-digit steps the cursor back, so `\xGG` stores a NUL and then `GG`. `\xFF`, `\377`, and `\777` store 0xFF. `\400` stores a NUL. The stored byte is the low 8 bits.

`ExpandEnvVars` of null is empty. A string with no `$` is copied. A name is the run of digits, letters, and `_` after `$`. `progdir` matches without regard to case and appends the program directory, including when that directory is empty, and does not call `getenv`. Any other name calls `getenv`. A null or empty value, after stopping at a NUL, is dropped. A `$` that is not followed by a name character is kept. Text between dollars is kept. The value is not scanned for further dollars.

Unix `NicePath` returns empty for null or empty. A path that does not start with `~` is `ExpandEnvVars`. `~` and `~/...` call `getpwuid(getuid())`. Any other `~name` calls `getpwnam` with the `~` included and does not call `getuid`. A missing passwd entry expands the original path. A null `pw_dir` contributes nothing, then a non-empty slash tail is expanded. Windows returns `.` for an empty path, throws on null, and does not treat `~` as a home directory.

| Managed (`VmCmdLib1367Slice.cs`) | Native (`cmdlib.cpp`) |
|----------------------------------|----------------------|
| `strbin1`, `ExpandEnvVars`, and `NicePath` | **738–970** |
| Band hull (exclusive slice) | **738–970** |

### Chunk **1367** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **233** |
| **Net headline** | **+233** vs the prior chunk |


**Post-review ratio:** 254/233 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmCmdLib1367SliceTests.StrBin1Escapes`
- `VmCmdLib1367SliceTests.ExpandEnvAndNicePath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1367 --to 1367` (0 hard / 0 soft).

## Seams

getenv, getuid, getpwuid, and getpwnam stay injected. strbin through line 737 and IsAbsPath at line 971 stay on chunk 122.

## Credit rationale

UNCredited cmdlib.cpp 738-970 (+233). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. m_png.cpp 655-886 is 232. Gaps longer than 400 lines stay whole and are not sliced. strbin 646-737 and IsAbsPath 971-979 are already chunk 122.
