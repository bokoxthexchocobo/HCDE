# Chunk 1047 — `FSkillInfo::operator=`, `GetTextColor`, replacement maps, `DEFINE_FIELD` exports, and native `GetTextColor` action from `g_skill.cpp` (**541–690**).

## Scope

`FSkillInfo::operator=`, `GetTextColor`, replacement maps, `DEFINE_FIELD` exports, and native `GetTextColor` action from `g_skill.cpp` (**541–690**).

## Native ranges

- `src/gamedata/g_skill.cpp` **FSkillInfo assign / VM export** 541–690 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGSkillInfoAssignExport1047Slice.cs`
Managed LOC (tally): 110.
Conversion commit: `633ef75dccb5a405b7339b90e466bdc0935ac59d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FSkillInfo::operator=` (**530–569**, hull **541–569**) | yes |
| `FSkillInfo::GetTextColor` (**578–592**) | yes |
| `SetReplacement` / `GetReplacement` (**600–615**) | yes |
| `SetReplacedBy` / `GetReplacedBy` (**623–638**) | yes |
| `DEFINE_GLOBAL(AllSkills)` (**640**) | yes (export name constant) |
| `DEFINE_FIELD` list (**643–679**) | yes (field registry) |
| Native `GetTextColor` thunk (**681–690**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–540** | prior `g_skill` conversion / parse bands |
| **691+** | past EOF |
| `VmSkillInfoRuntime` | uncredited parallel — seam only |
| `VmGSkillNativeExportRecognition` | recognition catalog — seam only |

| Managed (`VmGSkillInfoAssignExport1047Slice.cs`) | Native (`g_skill.cpp`) |
|---------------------------------------------------|------------------------|
| `ApplySkillInfoOperatorAssign` | **532–568** (hull **541–569**) |
| `ResolveSkillInfoTextColor` / format helper | **578–592** |
| `SetSkillReplacement` / `GetSkillReplacement` | **600–615** |
| `SetSkillReplacedBy` / `GetSkillReplacedBy` | **623–638** |
| `AllSkillsGlobalExportName` | **640** |
| `DefineFieldExportNames` / `IsExportedSkillInfoField` | **643–679** |
| `InvokeNativeGetTextColorAction` | **681–690** |
| Band hull (exclusive slice) | **541–690** |

### Chunk **1047** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1046** |


**Post-review ratio:** 110/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmGSkillInfoAssignExport1047SliceTests.ApplySkillInfoOperatorAssign_CopiesTailFieldsAndMaps`
- `VmGSkillInfoAssignExport1047SliceTests.ResolveSkillInfoTextColor_EmptyUsesUntranslated`
- `VmGSkillInfoAssignExport1047SliceTests.ResolveSkillInfoTextColor_LogsUndefined`
- `VmGSkillInfoAssignExport1047SliceTests.ReplacementMaps_RoundTrip`
- `VmGSkillInfoAssignExport1047SliceTests.DefineFieldExportNames_IncludesAmmoFactor`
- `VmGSkillInfoAssignExport1047SliceTests.InvokeNativeGetTextColorAction_MatchesInstanceMethod`
- `VmGSkillInfoAssignExport1047SliceTests.AllSkillsGlobalExportName_MatchesDefineGlobal`
- `VmGSkillInfoAssignExport1047SliceTests.FormatUndefinedSkillTextColorMessage_IncludesSkillName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1047 --to 1047` (0 hard / 0 soft).

## Seams

`DEFINE_GLOBAL` / `DEFINE_FIELD` / `DEFINE_ACTION_FUNCTION_NATIVE` macro expansion and `V_ParseFontColor` remain host-injected; uncredited `VmSkillInfoRuntime` / `VmGSkillNativeExportRecognition` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_skill.cpp`** **541–690** (+150); excludes other credited `g_skill` bands in tally recognition rows only.
