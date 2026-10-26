# Chunk 858 — FraggleScript legacy console emulation from `fragglescript/t_cmd.cpp` (1–186). Retired (2026-10-23): out-of-body credit removed; honest **137** < **150**.

## Scope

FraggleScript legacy console emulation from `fragglescript/t_cmd.cpp` (1–186). Retired (2026-10-23): out-of-body credit removed; honest **137** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `505e6013b4e2334529948cc3a12b841dd4c51605`.

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

### Out-of-body uncredit (858)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **186** |
| **Out-of-body removed** | **49** |
| **Honest** | **137** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 858 --to 858` (0 hard / 0 soft).

## Seams

`FsEmulateCmd` uses `VmFScanner` and host `LevelCmdHost` for `dmflags`, `ChangeLevel`, `AddCommandString`, and per-player view height writes.

## Credit rationale

Isolated `FS_*` console bridge before broader `t_parse` runner chunks.
