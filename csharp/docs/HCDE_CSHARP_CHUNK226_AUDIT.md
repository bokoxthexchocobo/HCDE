# Chunk 226 — MAPINFO ParseSkill extended options through skill block finalize.

## Scope

MAPINFO ParseSkill extended options through skill block finalize.

## Native ranges

- `src/gamedata/g_skill.cpp` **FMapInfoParser::ParseSkill flags and options tail** 121–346 (226 lines)

**Credited native LOC:** 226 (tally `nativeLoc` 226).

## Managed

`VmSkillMapinfoParser.cs`, `VmSkillMapinfoParserTests.cs`
Managed LOC (tally): 120.
Conversion commit: `16bb99eb08d98f76f4cb4e8313320c67d1c9e119`.

## Tests

- `VmSkillMapinfoParserTests.TryApplyBareFlag_SetsNoPain`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 226 --to 226` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
