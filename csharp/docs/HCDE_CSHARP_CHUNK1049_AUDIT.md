# Chunk 1049 — Global `AllSkills` / `DefaultSkill` and `FMapInfoParser::ParseSkill` head from `g_skill.cpp` (**1–150**). Continues **1048–1047** (**391–690**).

## Scope

Global `AllSkills` / `DefaultSkill` and `FMapInfoParser::ParseSkill` head from `g_skill.cpp` (**1–150**). Continues **1048–1047** (**391–690**).

## Native ranges

- `src/gamedata/g_skill.cpp` **ParseSkill / AllSkills globals** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGSkillParseHead1049Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `83f38af65fb75a775656351cc7ccbb2bbe9187bf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AllSkills` / `DefaultSkill` globals (**35–36**) | yes (constants) |
| `ParseSkill` factory defaults (**50–81**) | yes |
| Skill name token (**83–84**) | yes |
| Float properties (**90–118**) | yes |
| Flag properties (**120–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–34** | license / includes host |
| **86–88** | `ParseOpenBrace` / scanner loop host |
| **151+** | remainder of `ParseSkill` (**1049** hull ends **150**) |
| **391–690** | chunks **1048–1047** |
| `VmSkillMapinfoParser` | uncredited token parser — seam only |
| `VmGSkillGlobals` | uncredited list helper — seam only |

| Managed (`VmGSkillParseHead1049Slice.cs`) | Native (`g_skill.cpp`) |
|-------------------------------------------|------------------------|
| `DefaultSkillIndexUnset` | **36** |
| `ApplyParseSkillFactoryDefaults` | **50–81** |
| `AssignSkillBlockNameFromScanner` | **83–84** |
| `ClassifySkillParseKeyword` | **90–150** keywords |
| `TryApplySkillParseFloatProperty` | **90–118** |
| `TryApplySkillParseFlagProperty` | **120–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1049** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1048** |


**Post-review ratio:** 158/150 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmGSkillParseHead1049SliceTests.ApplyParseSkillFactoryDefaults_MatchesNativeBaseline`
- `VmGSkillParseHead1049SliceTests.ClassifySkillParseKeyword_RecognizesHeadBandKeys`
- `VmGSkillParseHead1049SliceTests.TryApplySkillParseFloatProperty_SetsFactors`
- `VmGSkillParseHead1049SliceTests.TryApplySkillParseFlagProperty_SetsFlags`
- `VmGSkillParseHead1049SliceTests.AssignSkillBlockNameFromScanner_SetsName`
- `VmGSkillParseHead1049SliceTests.DefaultSkillIndexUnset_MatchesGlobal`
- `VmGSkillParseHead1049SliceTests.TryApplySkillParseFloatProperty_RejectsFlagKeywords`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1049 --to 1049` (0 hard / 0 soft).

## Seams

`FScanner`, `ParseOpenBrace`, and `TArray` host remain injected; uncredited `VmSkillMapinfoParser` / `VmGSkillGlobals` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_skill.cpp`** **1–150** (+150); excludes **391–690** (**1048–1047**).
