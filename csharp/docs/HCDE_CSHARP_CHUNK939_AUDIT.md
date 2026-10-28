# Chunk 939 — `SF_TestLocation` close through `SF_ObjState` argc-2 branch from `fragglescript/t_func.cpp` (**2985–3134**). Follows **938** (**≤2984**); **3135–3244** sub-150 deferred.

## Scope

`SF_TestLocation` close through `SF_ObjState` argc-2 branch from `fragglescript/t_func.cpp` (**2985–3134**). Follows **938** (**≤2984**); **3135–3244** sub-150 deferred.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_HealObj** 2985–3134 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserCombatMap939Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `900a6d963371bcdb29d00963406e412c782a6924`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| chunk **939** hull | **2985–3134** |
| `TestLocationReturnInt` | `SF_TestLocation` tail **2980–2985** |
| `HealModeFromArgc` / `OptionalActor` | `SF_HealObj` **2993–3020** |
| `HealSyncsPlayer` | player health sync **3009**, **3015** |
| `ObjDeadReturn` | `SF_ObjDead` **3029–3047** |
| `SpawnMissileCanRun` | `SF_SpawnMissile` **3055–3069** |
| `BuildMapThingExist` | `SF_MapThingNumExist` **3077–3098** |
| `MapThingsCountReturn` | `SF_MapThings` **3106–3112** |
| `BuildObjState` / `ObjStateNames` | `SF_ObjState` **3121–3138** |
| `ObjStateValid` | state range **3146–3150** (seam) |
| `MapThingIndexInRange` | bounds **3087–3088** |
| invalid heal argc | **3018–3019** |
| corpse flag test | **3043–3044** |

### Chunk **939** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**938** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserCombatMap939SliceTests.HealModeFromArgc_RestoresDefaultWhenSingleArg`
- `VmFScriptParserCombatMap939SliceTests.ObjDeadReturn_DetectsCorpseFlag`
- `VmFScriptParserCombatMap939SliceTests.BuildMapThingExist_RejectsOutOfRange`
- `VmFScriptParserCombatMap939SliceTests.MapThingsCountReturn_UsesSize`
- `VmFScriptParserCombatMap939SliceTests.BuildObjState_UsesTriggerForUnaryForm`
- `VmFScriptParserCombatMap939SliceTests.ObjStateValid_RejectsZero`
- `VmFScriptParserCombatMap939SliceTests.SpawnMissileCanRun_RequiresBoth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 939 --to 939` (0 hard / 0 soft).

## Seams

`P_TestMobjLocation`, `P_SpawnMissile`, `SpawnedThings`, `SetState`/`FindState`, and `script_error` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **2685–3244** corridor; exclusive `VmFScriptParserCombatMap939Slice.cs`.
