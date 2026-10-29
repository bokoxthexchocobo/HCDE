# Chunk 1189 — DoomEd numbers, spawn types, and spawn-map parsing from g_doomedmap.cpp (250–457).

## Scope

DoomEd numbers, spawn types, and spawn-map parsing from g_doomedmap.cpp (250–457).

## Native ranges

- `src/gamedata/g_doomedmap.cpp` **InitSpawnablesFromMapinfo** 250–457 (208 lines)

**Credited native LOC:** 208 (tally `nativeLoc` 208).

## Managed

`VmGDoomedMap1189Slice.cs`
Managed LOC (tally): 720.
Conversion commit: `96c76bb5af27c46152e5110115a9f0ec15a8560a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| InitActorNumsFromMapinfo (**251–284**) | yes |
| P_GetSpawnableType (**286–305**) | yes |
| Spawn item, sort, dump, and clear (**307–358**) | yes |
| ParseSpawnMap (**361–400**) | yes |
| ParseSpawnNums and ParseConversationIDs (**407–417**) | yes |
| InitClassMap (**420–451**) | yes |
| InitSpawnablesFromMapinfo (**453–457**) | yes |
| Band hull (**250–457**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| error summary (**401–404**) | `}` returns, so this ScriptError never runs |

DoomEdMap is cleared first. Every editor number is inserted, including an unknown class and NAME_None. Special is a short and ArgsDefined is a signed char. The five args are copied. Unknown classes are printed, then I_Error runs before the source list is cleared. A negative spawn number is a name index when that index is in range, and it does not consult the numbered map. A numbered hit returns the stored class, which may be null. Dump lines are sorted by key and printed as `%d %s`. ClearStrifeTypes clears only StrifeTypes. An unknown brace format becomes new when the next token is `{`, and that sets C mode. Old format does not consume a brace. New format requires `{`. ParseSpawnMap warns on a duplicate or a negative number, then still inserts the class. The closing brace returns, so there is no error summary. InitClassMap clears the destination, inserts a null class for an unknown name, removes NAME_None, and I_Error leaves the source list in place. InitSpawnablesFromMapinfo resolves spawn numbers first. A failure there does not resolve conversation ids.

| Managed (`VmGDoomedMap1189Slice.cs`) | Native (`g_doomedmap.cpp`) |
|--------------------------------------|----------------------------|
| Spawnable mapinfo init | **250–457** |
| Band hull (exclusive slice) | **250–457** |

### Chunk **1189** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **208** |
| **Net headline** | **+208** vs post-**1188** |


**Post-review ratio:** 720/208 managed/native ≈ 346% (src file LOC at HEAD; see tally).

## Tests

- `VmGDoomedMap1189SliceTests.InitActorNums_CopiesArgsAndAbortsWithoutClearingTheSource`
- `VmGDoomedMap1189SliceTests.GetSpawnableType_UsesNameIndexesAndSpawnNumbers`
- `VmGDoomedMap1189SliceTests.ParseSpawnMap_WarnsAndStillInsertsUntilTheBrace`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1189 --to 1189` (0 hard / 0 soft).

## Seams

Actor classes and the name table are host data. FName folds None, in any case, to index 0, and an empty class name is that same index. The scanner is the classic or C-mode token reader this band calls.

## Credit rationale

UNCredited g_doomedmap.cpp 250–457 (+208). The file ends at 457. Chunk 210 already credits through line 249. A full tally line scan shows no overlap.
