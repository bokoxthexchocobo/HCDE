# Chunk 964 — `FBehavior::GetFunction`, map/function name lookup, array accessors, chunk walk, and string lookup head from `p_acs.cpp` (**3155–3304**).

## Scope

`FBehavior::GetFunction`, map/function name lookup, array accessors, chunk walk, and string lookup head from `p_acs.cpp` (**3155–3304**).

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::GetFunction** 3155–3304 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsBehaviorLookup964Slice.cs`
Managed LOC (tally): 201.
Conversion commit: `d5075db816cf027cc20855945f2032f0420a25ed`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FBehavior::GetFunction` (**3155–3169**) | yes |
| `FindFunctionName` / `FindMapVarName` / `FindMapArray` (**3171–3189**) | yes (chunk lookup via host) |
| `FindStringInChunk` (**3191–3206**) | yes |
| `GetArrayVal` / `SetArrayVal` (**3208–3226**) | yes |
| `CopyStringToArray` (**3228–3245**) | yes |
| `FindChunk` / `NextChunk` (**3247–3275**) | yes |
| `FBehaviorContainer::LookupString` (**3277–3290**) | yes |
| `FBehavior::LookupString` old/enhanced range gates (**3292–3304**) | yes |
| Separator (**3276**, **3291**) | layout only |

| Managed (`VmPAcsBehaviorLookup964Slice.cs`) | Native (`p_acs.cpp`) |
|---------------------------------------------|---------------------|
| `ResolveGetFunction` / `FunctionIndexInRange` | **3155–3169** |
| `FindStringInChunk` | **3191–3206** |
| `ResolveMapArrayIndex` | **3181–3189** |
| `GetArrayVal` / `SetArrayVal` | **3208–3226** |
| `CopyStringToArray` | **3228–3245** |
| `FindChunkOffset` / `NextChunkOffset` | **3247–3275** |
| `LookupUsesStringPool` / `LookupModuleInRange` | **3277–3290** |
| `OldFormatStringInRange` / `EnhancedFormatStringInRange` | **3292–3304** |
| Band hull (exclusive slice) | **3155–3304** |

### Chunk **964** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**963** |


**Post-review ratio:** 201/150 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsBehaviorLookup964SliceTests.ResolveGetFunction_LocalFunction`
- `VmPAcsBehaviorLookup964SliceTests.ResolveGetFunction_ImportRedirect`
- `VmPAcsBehaviorLookup964SliceTests.FindStringInChunk_MatchesName`
- `VmPAcsBehaviorLookup964SliceTests.GetArrayVal_ReturnsZeroWhenOutOfRange`
- `VmPAcsBehaviorLookup964SliceTests.CopyStringToArray_WritesTerminator`
- `VmPAcsBehaviorLookup964SliceTests.FindChunkOffset_WalksLinkedChunks`
- `VmPAcsBehaviorLookup964SliceTests.LookupUsesStringPool_DetectsPoolIndex`
- `VmPAcsBehaviorLookup964SliceTests.OldFormatStringInRange_RequiresValidTable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 964 --to 964` (0 hard / 0 soft).

## Seams

`Imports[]` recursion, `stricmp`, `GlobalACSStrings.GetString`, Hexen localization in `LookupString`, and `StartTypedScripts` tail remain host seams.

## Credit rationale

Honest **150**-line UNCredited band adjacent to chunk **963**; exclusive `VmPAcsBehaviorLookup964Slice.cs` (does not re-credit chunk **568** container load helpers at **1977–2124**).
