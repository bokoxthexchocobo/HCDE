# Chunk 938 — `SF_MobjValue` … `SF_TestLocation` from `fragglescript/t_func.cpp` (**2835–2984**). Follows **937** (**≤2834**); **2985+** deferred toward **3244**.

## Scope

`SF_MobjValue` … `SF_TestLocation` from `fragglescript/t_func.cpp` (**2835–2984**). Follows **937** (**≤2834**); **2985+** deferred toward **3244**.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjValue** 2835–2984 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserTypeLegacy938Slice.cs`
Managed LOC (tally): 87.
Conversion commit: `c966dcebfcca9e9b490623fd9048ae8814ae5ebc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| chunk **938** hull | **2835–2984** |
| `BuildMobjCoerce` / `ReturnTypeMobj` | `SF_MobjValue` **2834–2841** |
| `BuildStringCoerce` / `StringCoerceValue` | `SF_StringValue` **2843–2857** |
| `ReturnTypeInt` | `SF_IntValue` **2859–2866** |
| `ReturnTypeFixed` | `SF_FixedValue` **2868–2875** |
| Legacy section header | **2878–2882** |
| `BuildSpawnExplosion` / `SpawnExplosionZ` | `SF_SpawnExplosion` **2884–2911** |
| `RadiusAttackCanRun` | `SF_RadiusAttack` **2919–2936** |
| `BuildSetObjPosition` | `SF_SetObjPosition` **2944–2959** |
| `TestLocationActor` / `TestLocationReturn` | `SF_TestLocation` **2967–2984** |
| `CoerceRequiresOneArg` | type coercions **2836–2871** |
| spawn death return | **2902–2908** |
| optional Z from floor | **2896–2899** |

### Chunk **938** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**937** |


**Post-review ratio:** 87/150 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserTypeLegacy938SliceTests.StringCoerceValue_UsesDirectString`
- `VmFScriptParserTypeLegacy938SliceTests.SpawnExplosionZ_UsesFloorWhenArgMissing`
- `VmFScriptParserTypeLegacy938SliceTests.BuildSpawnExplosion_ReturnsZeroWhenSpawnFails`
- `VmFScriptParserTypeLegacy938SliceTests.RadiusAttackCanRun_RequiresBothActors`
- `VmFScriptParserTypeLegacy938SliceTests.BuildSetObjPosition_DefaultsYAndZ`
- `VmFScriptParserTypeLegacy938SliceTests.TestLocationActor_FallsBackToTrigger`
- `VmFScriptParserTypeLegacy938SliceTests.TestLocationReturn_ConvertsBool`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 938 --to 938` (0 hard / 0 soft).

## Seams

`actorvalue`/`stringvalue`/`fixedvalue`, `Spawn`, `P_RadiusAttack`, `P_TestMobjLocation`, and `SetOrigin` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **2685–3244** corridor; exclusive `VmFScriptParserTypeLegacy938Slice.cs`.
