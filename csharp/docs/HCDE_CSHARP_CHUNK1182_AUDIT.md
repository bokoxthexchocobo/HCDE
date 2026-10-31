# Chunk 1182 — FraggleScript legacy console commands from t_cmd.cpp (1–186).

## Scope

FraggleScript legacy console commands from t_cmd.cpp (1–186).

## Native ranges

- `src/playsim/fragglescript/t_cmd.cpp` **FS_EmulateCmd** 1–186 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmTCmdEmulate1182Slice.cs`
Managed LOC (tally): 496.
Conversion commit: `f08202ae6002fc57928bcacbabf694656856e419`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FS_Gimme (**35–60**) | yes |
| FS_MapCmd (**69–101**) | yes |
| FS_EmulateCmd (**109–186**) | yes |
| Band hull (**1–186**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Give rewrites a Legacy prefix before the %.72s format. Shotgun is tested before supershotgun, and map is tested before fullmap. Map starts from reset-inventory and reset-health, adds no-monsters only when dmflags has 1<<12, clamps the skill with the host skill count, and copies at most eight map characters. A bare semicolon comments out the rest of the line. A quoted semicolon stops a gimme list or a skip loop. Map reads until the scanner ends, so it consumes a following command. Fog density is integer number*70/400 and does not skip, but a following gravity command is still inside gravity's own skip once gravity runs. A bad float aborts the command.

| Managed (`VmTCmdEmulate1182Slice.cs`) | Native (`t_cmd.cpp`) |
|----------------------------------------|--------------------|
| Gimme, map, and emulate | **1–186** |
| Band hull (exclusive slice) | **1–186** |

### Chunk **1182** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **186** |
| **Net headline** | **+186** vs post-**1181** |


**Post-review ratio:** 496/186 managed/native ≈ 267% (src file LOC at HEAD; see tally).

## Tests

- `VmTCmdEmulate1182SliceTests.Gimme_RewritesLegacyPrefixesInOrder`
- `VmTCmdEmulate1182SliceTests.MapCmd_AppliesSkillMonstersAndTheRealNoMonstersBit`
- `VmTCmdEmulate1182SliceTests.EmulateCmd_CommentsBareSemicolonsAndStillHonorsAQuotedOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1182 --to 1182` (0 hard / 0 soft).

## Seams

OpenMem uses the classic FScanner: a bare semicolon or // comments out the rest of the line, and a quoted semicolon is the token Compare(";") can match. Skill count is the host AllSkills size. DF_NO_MONSTERS is 1<<12. ChangeLevel and give commands are recorded on the host. VmFScriptCmd stays untouched.

## Credit rationale

UNCredited t_cmd.cpp 1–186 (+186). Chunk 858 retired this file with empty native ranges. The file ends at 186. A full tally line scan shows no overlap.
