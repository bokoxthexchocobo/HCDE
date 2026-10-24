# Chunk 858 — FraggleScript legacy console emulation from `fragglescript/t_cmd.cpp` (1–186).

## Scope

FraggleScript legacy console emulation from `fragglescript/t_cmd.cpp` (1–186).

## Native ranges

- `src/playsim/fragglescript/t_cmd.cpp` **FS_Gimme** 35–60 (26 lines)
- `src/playsim/fragglescript/t_cmd.cpp` **FS_MapCmd** 69–101 (33 lines)
- `src/playsim/fragglescript/t_cmd.cpp` **FS_EmulateCmd** 109–186 (78 lines)
- `src/playsim/fragglescript/t_cmd.cpp` **t_cmd separators** 1–34 (34 lines)
- `src/playsim/fragglescript/t_cmd.cpp` **t_cmd separators** 61–68 (8 lines)
- `src/playsim/fragglescript/t_cmd.cpp` **t_cmd separators** 102–108 (7 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmFScriptCmd.cs`
Managed LOC (tally): 212.
Conversion commit: `5cb99aac9024861a4a308d866aacb36ef2771354`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `FS_Gimme` | yes |
| `FS_MapCmd` | yes |
| `FS_EmulateCmd` | yes |

| Managed | Native (`t_cmd.cpp`) |
|---------|----------------------|
| `Gimme` | **35–60** |
| `MapCmd` | **69–101** |
| `EmulateCmd` | **109–186** |
| (license / separators) | **1–34**, **61–68**, **102–108** |

### Chunk **858** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` (gross **1–186**) | **186** |
| **Net headline** | **+186** vs post-**853** |

**Post-review ratio:** 212/186 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptCmdTests.Gimme_Health_NormalizesAndGives`
- `VmFScriptCmdTests.MapCmd_ChangeLevelWithFlags`
- `VmFScriptCmdTests.EmulateCmd_GimmeAndGravity`
- `VmFScriptCmdTests.EmulateCmd_AllowJumpTogglesDmflags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 858 --to 858` (0 hard / 0 soft).

## Seams

`FsEmulateCmd` uses `VmFScanner` and host `LevelCmdHost` for `dmflags`, `ChangeLevel`, `AddCommandString`, and per-player view height writes.

## Credit rationale

Isolated `FS_*` console bridge before broader `t_parse` runner chunks.
