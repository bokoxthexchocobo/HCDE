# Chunk 219 — MAPINFO ParseSkill defaults and factor options.

## Scope

MAPINFO ParseSkill defaults and factor options.

## Native ranges

- `src/gamedata/g_skill.cpp` **FMapInfoParser::ParseSkill defaults** 46–82 (37 lines)
- `src/gamedata/g_skill.cpp` **FMapInfoParser::ParseSkill options** 90–120 (31 lines)

**Credited native LOC:** 68 (tally `nativeLoc` 68).

## Managed

`VmSkillMapinfoParser.cs`, `VmSkillMapinfoParserTests.cs`
Managed LOC (tally): 68.
Conversion commit: `ad41dd855f4c00823f7aad7de6c5f8473aa8062a`.

## Tests

- `VmSkillMapinfoParserTests.TryApplyOption_SetsAmmoFactor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 219 --to 219` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
