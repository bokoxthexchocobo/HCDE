# Chunk 848 — `FParser::SF_SpawnMissile` … `SF_PlayerAddFrag` from `fragglescript/t_func.cpp` (3048–3237).

## Scope

`FParser::SF_SpawnMissile` … `SF_PlayerAddFrag` from `fragglescript/t_func.cpp` (3048–3237).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SpawnMissile** 3055–3069 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MapThingNumExist** 3077–3098 (22 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MapThings** 3106–3112 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjState** 3121–3156 (36 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LineFlag** 3166–3200 (35 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerAddFrag** 3209–3237 (29 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3048–3054 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3070–3076 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3099–3105 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3113–3120 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3157–3165 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF legacy separators** 3201–3208 (8 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmFScriptParserSfLegacy.cs`
Managed LOC (tally): 156.
Conversion commit: `8a8e1476fc1e700f4acfd2c3c93d8111c4cd6714`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SpawnMissile` | yes |
| `SF_MapThingNumExist` | yes |
| `SF_MapThings` | yes |
| `SF_ObjState` | yes |
| `SF_LineFlag` | yes |
| `SF_PlayerAddFrag` | yes |

| Managed | Native (`t_func.cpp` 3048–3237) |
|---------|-------------------------|
| `SfSpawnMissile` | **3055–3069** |
| `SfMapThingNumExist` | **3077–3098** |
| `SfMapThings` | **3106–3112** |
| `SfObjState` | **3121–3156** |
| `SfLineFlag` | **3166–3200** |
| `SfPlayerAddFrag` | **3209–3237** |
| (separators) | **3048–3054**, **3070–3076**, **3099–3105**, **3113–3120**, **3157–3165**, **3201–3208** |

**Post-review ratio:** 156/190 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfLegacyTests.SfSpawnMissile_SpawnsAtTarget`
- `VmFScriptParserSfLegacyTests.SfMapThingNumExist_ValidIndex`
- `VmFScriptParserSfLegacyTests.SfMapThings_ReturnsCount`
- `VmFScriptParserSfLegacyTests.SfObjState_SetsSeeState`
- `VmFScriptParserSfLegacyTests.SfLineFlag_ReadWriteBlocking`
- `VmFScriptParserSfLegacyTests.SfPlayerAddFrag_IncrementsFragCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 848 --to 848` (0 hard / 0 soft).

## Seams

Uses `AuthoritySimulation.SpawnProjectile`, `FraggleScriptSpawnedThings`, `LevelLine.Flags`, and `PlayerPawn` frag fields. **3259–3316** combat deferred to **851**.

## Credit rationale

Legacy map/missile band between **845** act tail and **846** stub/trig; **1381–2241** and **3454–3472** remain deferred.
