# Chunk 963 — `FBehavior::SortScripts`, `UnencryptStrings`, `UnescapeStringTable`, `IsGood`, and `FindScript` from `p_acs.cpp` (**2993–3142**).

## Scope

`FBehavior::SortScripts`, `UnencryptStrings`, `UnescapeStringTable`, `IsGood`, and `FindScript` from `p_acs.cpp` (**2993–3142**).

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::SortScripts** 2993–3142 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsBehaviorStringValidate963Slice.cs`
Managed LOC (tally): 206.
Conversion commit: `0810e58f8a9fe2d20bb9ea7f003a515b8bc0786b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FBehavior::SortScripts` (**2993–2998**) | yes |
| `FBehavior::UnencryptStrings` (**3008–3034**) | yes |
| `FBehavior::UnescapeStringTable` (**3048–3078**) | yes |
| `FBehavior::IsGood` (**3086–3122**) | yes |
| `FBehavior::FindScript` / container scan head (**3124–3142**) | yes |
| Comment/separator lines (**3000–3007**, **3036–3047**, **3080–3085**, **3123**) | layout only |

| Managed (`VmPAcsBehaviorStringValidate963Slice.cs`) | Native (`p_acs.cpp`) |
|-----------------------------------------------------|---------------------|
| `CompareScriptNumbers` / `SortScriptsComparison` | **2993–2998** |
| `UnencryptStringChunks` | **3008–3034** |
| `UnescapeStringTable` / `UnescapeStrBinInPlace` | **3048–3078** |
| `IsGood` | **3086–3122** |
| `FindScriptIndex` / `FindScriptModuleIndex` | **3124–3152** (hull through container loop head) |
| Band hull (exclusive slice) | **2993–3142** |

### Chunk **963** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**962** (retired) |


**Post-review ratio:** 206/150 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsBehaviorStringValidate963SliceTests.CompareScriptNumbers_OrdersAscending`
- `VmPAcsBehaviorStringValidate963SliceTests.UnencryptStringChunks_RetagsStreToStrl`
- `VmPAcsBehaviorStringValidate963SliceTests.UnescapeStringTable_UnpaddedTableDecodesEscapes`
- `VmPAcsBehaviorStringValidate963SliceTests.IsGood_RejectsUnknownFormat`
- `VmPAcsBehaviorStringValidate963SliceTests.IsGood_RejectsMissingImport`
- `VmPAcsBehaviorStringValidate963SliceTests.FindScriptIndex_PrefersEarlierDuplicate`
- `VmPAcsBehaviorStringValidate963SliceTests.FindScriptModuleIndex_ScansModulesInOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 963 --to 963` (0 hard / 0 soft).

## Seams

`FindChunk`, `NextChunk`, `qsort`, `I_Error`, `Printf`, and `BinarySearch` template remain host seams; `VmPAcsBehaviorInit` still injects native string hooks during `Init`.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsBehaviorStringValidate963Slice.cs` (does not re-credit chunk **515** directory load or **752**-class init orchestration).
