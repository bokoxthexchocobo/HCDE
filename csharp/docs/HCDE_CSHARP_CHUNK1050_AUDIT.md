# Chunk 1050 — `FMapInfoParser::ParseSkill` continuation from `g_skill.cpp` (**151–300**). Follows **1049** (**1–150**).

## Scope

`FMapInfoParser::ParseSkill` continuation from `g_skill.cpp` (**151–300**). Follows **1049** (**1–150**).

## Native ranges

- `src/gamedata/g_skill.cpp` **ParseSkill continuation** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGSkillParseMid1050Slice.cs`
Managed LOC (tally): 218.
Conversion commit: `40cd3b0849dd7d365e2a3c7fcd3461d271cb36d5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `respawntime` / `respawnlimit` (**152–163**) | yes |
| `Aggressiveness` / `SpawnFilter` (**164–186**) | yes |
| spawn / reaction flags (**187–198**) | yes |
| `ACSReturn` / `ReplaceActor` (**199–216**) | yes |
| menu / pic / confirm / key / color (**217–268**) | yes |
| health / armor factors (**269–296**) | yes |
| `NoInfighting` (**297–300**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–150** | chunk **1049** |
| **151** | closing brace from prior property |
| **301+** | `TotalInfighting`, finalize, push (**301–390** deferred) |
| **391–690** | chunks **1048–1047** |
| `VmSkillMapinfoParser` | uncredited token parser — seam only |

| Managed (`VmGSkillParseMid1050Slice.cs`) | Native (`g_skill.cpp`) |
|----------------------------------------|------------------------|
| `ConvertRespawnTimeSecondsToTics` / limit | **152–163** |
| `ConvertAggressivenessInput` / spawn filter helpers | **164–186** |
| `TryApplySkillParseMidFlag` spawn/reaction flags | **187–198**, **281–300** |
| `TryApplySkillParseMidProperty` ACS/replace/menu | **199–268** |
| health/armor float properties | **269–296** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1050** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1049** |


**Post-review ratio:** 218/150 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmGSkillParseMid1050SliceTests.ConvertRespawnTimeSecondsToTics_UsesTicRate`
- `VmGSkillParseMid1050SliceTests.ConvertAggressivenessInput_InvertsClamped`
- `VmGSkillParseMid1050SliceTests.ApplySpawnFilterWord_AddsDifficultyBits`
- `VmGSkillParseMid1050SliceTests.ApplyReplaceActorPair_RegistersBothMaps`
- `VmGSkillParseMid1050SliceTests.TryApplySkillParseMidProperty_SetsAcsReturnFlag`
- `VmGSkillParseMid1050SliceTests.FormatSkillTextColorToken_WrapsBrackets`
- `VmGSkillParseMid1050SliceTests.TryApplySkillParseMidFlag_NoInfighting`
- `VmGSkillParseMid1050SliceTests.ClassifySkillParseMidKeyword_ReplaceActor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1050 --to 1050` (0 hard / 0 soft).

## Seams

`FScanner`, `ParseAssign`, `strbin1`, and `ParseCloseBrace` remain host-injected; uncredited `VmSkillMapinfoParser` / **1047** replacement helpers used as seams only.

## Credit rationale

File-scoped UNCredited **`g_skill.cpp`** **151–300** (+150); excludes **1–150** (**1049**) and **391–690** (**1048–1047**). Hull ends at **NoInfighting** (**300**).
