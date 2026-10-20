# Chunk 231 — g_skill/teaminfo uncredited gap lines.

## Scope

g_skill/teaminfo uncredited gap lines.

## Native ranges

- `src/gamedata/g_skill.cpp` **g_skill file header** 1–34 (34 lines)
- `src/gamedata/g_skill.cpp` **ParseSkill name tokens** 88–89 (2 lines)
- `src/gamedata/g_skill.cpp` **G_SkillProperty gap** 416–416 (1 lines)
- `src/gamedata/g_skill.cpp` **G_SkillProperty float preamble** 423–429 (7 lines)
- `src/gamedata/g_skill.cpp` **G_SkillProperty float gap** 470–471 (2 lines)
- `src/gamedata/g_skill.cpp` **G_SkillProperty float tail** 478–486 (9 lines)
- `src/gamedata/g_skill.cpp` **G_SkillName export gap** 503–503 (1 lines)
- `src/gamedata/g_skill.cpp` **G_SkillName preamble** 509–515 (7 lines)
- `src/gamedata/g_skill.cpp` **G_VerifySkill section gaps** 523–529 (7 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo assignment gaps** 571–577 (7 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo replacement gaps** 593–599 (7 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo replacement export gap** 639–639 (1 lines)
- `src/gamedata/g_skill.cpp` **DEFINE_FIELD gap** 680–680 (1 lines)
- `src/gamedata/teaminfo.cpp` **teaminfo header** 1–56 (56 lines)
- `src/gamedata/teaminfo.cpp` **teaminfo section gaps** 98–106 (9 lines)
- `src/gamedata/teaminfo.cpp` **ParseTeamInfo section gaps** 116–122 (7 lines)
- `src/gamedata/teaminfo.cpp` **ParseTeamDefinition gap** 142–154 (13 lines)
- `src/gamedata/teaminfo.cpp` **ClearTeams section gaps** 221–227 (7 lines)
- `src/gamedata/teaminfo.cpp` **IsValid section gaps** 232–238 (7 lines)
- `src/gamedata/teaminfo.cpp` **GetLogo section gaps** 329–335 (7 lines)
- `src/gamedata/teaminfo.cpp` **teamlist gap lines** 345–346 (2 lines)
- `src/gamedata/teaminfo.cpp` **DEFINE_GLOBAL gap** 349–349 (1 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmGSkillNativeExportRecognition.cs`, `VmGSkillNativeExportRecognitionTests.cs`, `VmSkillMapinfoCommit.cs`, `VmSkillMapinfoCommitTests.cs`, `VmTeamInfoRuntime.cs`, `VmTeamInfoRuntimeTests.cs`, `VmTeamNativeExportRecognition.cs`, `VmTeamNativeExportRecognitionTests.cs`
Managed LOC (tally): 120.
Conversion commit: `daa96f1c8f18595984d8055dc3610329e78da441`.

## Tests

- `VmSkillMapinfoCommitTests.ApplyNativeDefaultFields_SetsDropAmmoFactorNegativeOne`
- `VmSkillMapinfoParserTests.AssignSkillNameFromToken_SetsName`
- `VmGSkillPropertyQueryTests.QueryInt_FastMonsters_HonorsDmflag`
- `VmGSkillPropertyQueryTests.QueryFloat_AmmoFactor_UsesDoubleWhenFlagSet`
- `VmGSkillPropertyQueryTests.ResolveSkillMenuName_UsesPlayerClassOverride`
- `VmGSkillPropertyQueryTests.VerifyGameSkill_ClampsHighIndex`
- `VmSkillInfoRuntimeTests.Clone_CopiesReplacementMaps`
- `VmSkillInfoRuntimeTests.GetReplacedBy_ReturnsMapping`
- `VmSkillInfoRuntimeTests.GetTextColor_EmptyIsUntranslated`
- `VmTeamInfoParserTests.ParseTeam_ReadsOptions`
- `VmTeamInfoRuntimeTests.TryChangeTeam_RejectsSinglePlayer`
- `VmTeamInfoRuntimeTests.GetTeamName_ReturnsDefinitionName`
- `VmTeamInfoParserTests.FormatTeamList_IncludesIndices`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 231 --to 231` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
