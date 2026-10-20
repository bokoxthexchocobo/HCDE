# Chunk 227 — G_SkillProperty, G_SkillName, G_VerifySkill, FSkillInfo copy/replacements/text color.

## Scope

G_SkillProperty, G_SkillName, G_VerifySkill, FSkillInfo copy/replacements/text color.

## Native ranges

- `src/gamedata/g_skill.cpp` **G_SkillProperty int** 347–415 (69 lines)
- `src/gamedata/g_skill.cpp` **G_SkillProperty float** 430–469 (40 lines)
- `src/gamedata/g_skill.cpp` **G_SkillName** 487–502 (16 lines)
- `src/gamedata/g_skill.cpp` **G_VerifySkill** 516–522 (7 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo assignment** 530–570 (41 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo GetTextColor** 578–592 (15 lines)
- `src/gamedata/g_skill.cpp` **FSkillInfo replacement maps** 600–638 (39 lines)

**Credited native LOC:** 227 (tally `nativeLoc` 227).

## Managed

`VmGSkillPropertyQuery.cs`, `VmGSkillPropertyQueryTests.cs`, `VmSkillInfoRuntime.cs`, `VmSkillInfoRuntimeTests.cs`, `VmSkillMapinfoParser.cs`
Managed LOC (tally): 195.
Conversion commit: `9a543b4bc1d13e28bd3ff4d700c4bcc49e00b8fe`.

## Tests

- `VmGSkillPropertyQueryTests.QueryInt_FastMonsters_HonorsDmflag`
- `VmGSkillPropertyQueryTests.QueryFloat_AmmoFactor_UsesDoubleWhenFlagSet`
- `VmGSkillPropertyQueryTests.ResolveSkillMenuName_UsesPlayerClassOverride`
- `VmGSkillPropertyQueryTests.VerifyGameSkill_ClampsHighIndex`
- `VmSkillInfoRuntimeTests.Clone_CopiesReplacementMaps`
- `VmSkillInfoRuntimeTests.GetTextColor_EmptyIsUntranslated`
- `VmSkillInfoRuntimeTests.GetReplacedBy_ReturnsMapping`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 227 --to 227` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
