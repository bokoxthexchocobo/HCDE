# Chunk 965 — `FBehavior::LookupString` Hexen localization tail, `StartTypedScripts`, `StopMyScripts`, and `DACSThinker` serialize head from `p_acs.cpp` (**3305–3454**).

## Scope

`FBehavior::LookupString` Hexen localization tail, `StartTypedScripts`, `StopMyScripts`, and `DACSThinker` serialize head from `p_acs.cpp` (**3305–3454**).

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::LookupString** 3305–3454 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsBehaviorLookup965Slice.cs`
Managed LOC (tally): 185.
Conversion commit: `ccc0efe9ac37939dde6f670b0aa85246eac21000`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FBehavior::LookupString` old pointer + Hexen `TXT_ACS_*` (**3305–3323**) | yes |
| `FBehavior::LookupString` enhanced resolve (**3325–3332**) | yes |
| `FBehaviorContainer::StartTypedScripts` (**3335–3363**) | yes (type name + module fan-out plan) |
| `FBehavior::StartTypedScripts` (**3365–3383**) | yes (match type, flags, optional run-now) |
| `FBehaviorContainer::StopMyScripts` (**3391–3405**) | yes |
| `SavingRunningscript` + serialize helper (**3428–3443**) | yes |
| `DACSThinker::Serialize` write head (**3445–3454**) | yes |
| `IMPLEMENT_CLASS` / destructor / comment hull (**3408–3426**, **3334**, **3385–3389**) | layout only |

| Managed (`VmPAcsBehaviorLookup965Slice.cs`) | Native (`p_acs.cpp`) |
|---------------------------------------------|---------------------|
| `OldFormatStringDataOffset` / `ResolveOldLookupString` | **3305–3323** |
| `NormalizeHexenPrintToken` / `BuildHexenLocalizationLabel` | **3310–3318** |
| `EnhancedFormatStringDataOffset` / `ResolveEnhancedLookupString` | **3325–3332** |
| `ResolveTypedScriptTypeName` / `EnumerateModuleTypedScriptStarts` | **3335–3383** |
| `CollectStopMyScriptThinkers` | **3391–3405** |
| `SavingRunningscript` / `CollectRunningScriptsForWrite` | **3428–3443** |
| `ShouldWriteRunningScriptsArray` / field keys | **3445–3454** |
| Band hull (exclusive slice) | **3305–3454** |

### Chunk **965** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**964** |


**Post-review ratio:** 185/150 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsBehaviorLookup965SliceTests.NormalizeHexenPrintToken_StripsPunctuationAndSpaces`
- `VmPAcsBehaviorLookup965SliceTests.BuildHexenLocalizationLabel_MatchesNativePattern`
- `VmPAcsBehaviorLookup965SliceTests.ResolveOldLookupString_UsesTranslationWhenPresent`
- `VmPAcsBehaviorLookup965SliceTests.ResolveEnhancedLookupString_ReadsRelativeOffset`
- `VmPAcsBehaviorLookup965SliceTests.ResolveTypedScriptTypeName_FallsBackLikeNative`
- `VmPAcsBehaviorLookup965SliceTests.EnumerateModuleTypedScriptStarts_MatchesTypeAndFlags`
- `VmPAcsBehaviorLookup965SliceTests.CollectStopMyScriptThinkers_OrdersServerThenClient`
- `VmPAcsBehaviorLookup965SliceTests.ShouldWriteRunningScriptsArray_OnlyWhenWritingWithEntries`
- `VmPAcsBehaviorLookup965SliceTests.CollectRunningScriptsForWrite_SkipsNullValues`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 965 --to 965` (0 hard / 0 soft).

## Seams

`GStrings.CheckString`, `P_GetScriptGoing`, `RunScript`, `DACSThinker::StopScriptsFor`, live `FSerializer`, and `IMPLEMENT_*` macros remain host seams.

## Credit rationale

Honest **150**-line UNCredited band adjacent to chunk **964**; does not re-credit chunk **964** range gates (**3292–3304**) or off-tally `VmAcsLevelScriptList` (**3584–3672**).
