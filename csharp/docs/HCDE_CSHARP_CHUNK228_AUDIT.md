# Chunk 228 — g_skill globals/name assign; FTeam IsValid accessors and teamlist.

## Scope

g_skill globals/name assign; FTeam IsValid accessors and teamlist.

## Native ranges

- `src/gamedata/g_skill.cpp` **AllSkills and DefaultSkill** 35–45 (11 lines)
- `src/gamedata/g_skill.cpp` **ParseSkill name assignment** 83–87 (5 lines)
- `src/gamedata/teaminfo.cpp` **TeamInfoOptions table** 57–97 (41 lines)
- `src/gamedata/teaminfo.cpp` **FTeam default constructor** 107–115 (9 lines)
- `src/gamedata/teaminfo.cpp` **FTeam IsValid and accessors** 239–328 (90 lines)
- `src/gamedata/teaminfo.cpp` **CCMD teamlist** 336–344 (9 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmGSkillGlobals.cs`, `VmGSkillGlobalsTests.cs`, `VmSkillMapinfoParser.cs`, `VmSkillMapinfoParserTests.cs`, `VmTeamInfoParser.cs`, `VmTeamInfoParserTests.cs`
Managed LOC (tally): 55.
Conversion commit: `524c57aec6a349af74bd68f3efe5d1b4b995530c`.

## Tests

- `VmGSkillGlobalsTests.CreateEmptySkillList_StartsEmpty`
- `VmSkillMapinfoParserTests.AssignSkillNameFromToken_SetsName`
- `VmTeamInfoParserTests.ParseTeam_ReadsOptions`
- `VmTeamInfoParserTests.FormatTeamList_IncludesIndices`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 228 --to 228` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
