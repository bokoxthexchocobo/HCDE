# Chunk 941 — `SF_SetColor` loop tail through `SaveCurrentScript` from `fragglescript/t_func.cpp` (**3695–3844**). Follows **940** (**≤3694**); **3135–3244** and other deferred tails untouched.

## Scope

`SF_SetColor` loop tail through `SaveCurrentScript` from `fragglescript/t_func.cpp` (**3695–3844**). Follows **940** (**≤3694**); **3135–3244** and other deferred tails untouched.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetColor** 3695–3844 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserShotLine941Slice.cs`
Managed LOC (tally): 93.
Conversion commit: `ef933eaba4235d235a3b000736353b774a9856a4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| chunk **941** hull | **3695–3844** |
| `SetColorLoopAppliesToSector` | `SF_SetColor` loop **3693–3696** |
| `ResolveSpawnShot2Source` / `SpawnShot2UsesOptionalZ` | `SF_SpawnShot2` **3717–3724** |
| `SpawnShot2AbortsWithoutSource` | early return **3726** |
| `SpawnShot2ClearsMissileOnFailedSpawn` / `SpawnShot2ReturnMobj` | `P_CheckMissileSpawn` / return **3739–3741** |
| `BuildSpawnShot2Invoke` | `SF_SpawnShot2` setup **3717–3728** |
| `KillInSectorShouldMassacre` | `SF_KillInSector` **3761–3763** |
| `SetLineTriggerTagFromArgc` | optional tag **3784** |
| `MergeSetLineTriggerFlags` | line flag merge **3794–3795** |
| `SetLineTriggerIteratorActive` | line id iterator **3786** |
| `FillRunLineSpecialArgs` / `RunLineSpecialFillArg` | `RunLineSpecial` **3813–3818** |
| `RunLineSpecialRequiresMinArgs` | `CheckArgs(spec->min_args)` **3811** |
| `BuildSaveCurrentScript` | `SaveCurrentScript` **3830–3841** |
| `SpawnShot2RequiresTwoArgs` | `CheckArgs(2)` **3717** |
| `KillInSectorRequiresOneArg` | `CheckArgs(1)` **3755** |
| `SetLineTriggerRequiresTwoArgs` | `CheckArgs(2)` **3780** |

### Chunk **941** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**940** |


**Post-review ratio:** 93/150 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserShotLine941SliceTests.ResolveSpawnShot2Source_UsesTriggerWhenSecondArgNegativeInt`
- `VmFScriptParserShotLine941SliceTests.SpawnShot2ClearsMissileOnFailedSpawn_WhenCheckFails`
- `VmFScriptParserShotLine941SliceTests.KillInSectorShouldMassacre_RequiresMonsterAndTag`
- `VmFScriptParserShotLine941SliceTests.SetLineTriggerTagFromArgc_DefaultsZero`
- `VmFScriptParserShotLine941SliceTests.MergeSetLineTriggerFlags_PreservesActivationBits`
- `VmFScriptParserShotLine941SliceTests.FillRunLineSpecialArgs_PadsMissingWithZero`
- `VmFScriptParserShotLine941SliceTests.BuildSaveCurrentScript_ReturnsNullWithoutThinker`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 941 --to 941` (0 hard / 0 soft).

## Seams

`SetColor`, `Spawn`, `S_Sound`, `P_CheckMissileSpawn`, thinker iteration, `TranslateLineDef`, `P_ExecuteSpecial`, and `Create<DRunningScript>` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmFScriptParserShotLine941Slice.cs` (live `VmFScriptParserSfGzd` handlers are not matrix credit for these lines).
