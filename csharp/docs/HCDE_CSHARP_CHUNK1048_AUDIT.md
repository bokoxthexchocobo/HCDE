# Chunk 1048 — `G_SkillProperty` int/float tail, `G_SkillName`, `G_VerifySkill`, and `FSkillInfo::operator=` head from `g_skill.cpp` (**391–540**). Continues **1047** (**541–690**).

## Scope

`G_SkillProperty` int/float tail, `G_SkillName`, `G_VerifySkill`, and `FSkillInfo::operator=` head from `g_skill.cpp` (**391–540**). Continues **1047** (**541–690**).

## Native ranges

- `src/gamedata/g_skill.cpp` **G_SkillProperty / G_SkillName / G_VerifySkill** 391–540 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGSkillPropertyExport1048Slice.cs`
Managed LOC (tally): 110.
Conversion commit: `5c718377ad96763d6d9fb3be7058c16eb0544d21`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_SkillProperty` int tail (**392–410**) | yes |
| `G_SkillPropertyInt` export (**417–422**) | yes (export name) |
| `G_SkillProperty` float (**430–469**) | yes |
| `G_SkillPropertyFloat` export (**472–477**) | yes |
| `G_SkillName` (**487–501**) | yes |
| `G_SkillName` export (**504–508**) | yes |
| `G_VerifySkill` (**516–522**) | yes |
| `operator=` head (**530–540**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–390** | parse / property head bands |
| **541–690** | chunk **1047** |
| **691+** | past EOF |
| `VmGSkillPropertyQuery` | uncredited query layer — seam only |

| Managed (`VmGSkillPropertyExport1048Slice.cs`) | Native (`g_skill.cpp`) |
|--------------------------------------------------|------------------------|
| `ResolveSkillPropertyIntTail` / infighting helpers | **392–410** |
| `SkillPropertyIntExportName` | **417–422** |
| `ResolveFloatSkillProperty` | **430–469** |
| `SkillPropertyFloatExportName` | **472–477** |
| `ResolveDisplayedSkillName` | **487–501** |
| `SkillNameExportName` | **504–508** |
| `ClampActiveGameSkill` | **516–522** |
| `ApplySkillInfoOperatorAssignHead` | **532–540** |
| Band hull (exclusive slice) | **391–540** |

### Chunk **1048** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1047** |


**Post-review ratio:** 110/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmGSkillPropertyExport1048SliceTests.ResolveInfightingProperty_TotalAndNone`
- `VmGSkillPropertyExport1048SliceTests.ResolveFloatSkillProperty_DoubleAmmoFlag`
- `VmGSkillPropertyExport1048SliceTests.ResolveDisplayedSkillName_PerClassAndDollar`
- `VmGSkillPropertyExport1048SliceTests.ClampActiveGameSkill_Bounds`
- `VmGSkillPropertyExport1048SliceTests.ApplySkillInfoOperatorAssignHead_CopiesPrefixFields`
- `VmGSkillPropertyExport1048SliceTests.ResolveSkillPropertyIntTail_CoversBandCases`
- `VmGSkillPropertyExport1048SliceTests.ExportNames_MatchDefineActionFunctions`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1048 --to 1048` (0 hard / 0 soft).

## Seams

`DEFINE_ACTION_FUNCTION`, `GStrings`, and `players[consoleplayer]` remain host-injected; uncredited `VmGSkillPropertyQuery` / `VmGSkillNativeExportRecognition` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_skill.cpp`** **391–540** (+150); excludes **541–690** (**1047**). Recognition **227b** retired (no **417–508** overlap).
