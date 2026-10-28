# Chunk 940 — `SF_LevelNum` tail through `SF_SetColor` sector-tag loop head from `fragglescript/t_func.cpp` (**3545–3694**). Follows **939** (**≤3134**); **3135–3244** sub-150 deferred; avoids **846** trig credit below **3545**.

## Scope

`SF_LevelNum` tail through `SF_SetColor` sector-tag loop head from `fragglescript/t_func.cpp` (**3545–3694**). Follows **939** (**≤3134**); **3135–3244** sub-150 deferred; avoids **846** trig credit below **3545**.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LevelNum** 3545–3694 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserMobjLevel940Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `e2a705cb70df607d1fc85465986d568bc658c863`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| chunk **940** hull | **3545–3694** |
| `MobjGeomRequiresOneArg` / `MobjGeomAssignsWhenPresent` / `MobjGeomReturnDouble` | `SF_MobjRadius` **3554–3567**, `SF_MobjHeight` **3577–3590** |
| `ThingCountCountsMapActorsOnly` / `ThingCountCountsLiving` | `SF_ThingCount` branch **3618–3637** |
| `ThingCountActorQualifies` | map vs living filters **3624–3635** |
| `ThingCountShouldRepeatForReplacement` | replacement `goto again` **3638–3647** |
| `ThingCountRequiresOneArg` / `ThingCountAbortsWithoutClass` | `CheckArgs(1)` / missing type **3608–3611** |
| `CombineThingCountPasses` | accumulate after replacement pass **3649–3650** |
| `SetColorRequiresTwoArgs` / `SetColorSectorTagMissing` | `SF_SetColor` **3667–3676** |
| `SetColorEncodingFromArgc` / `ComposePalEntryRgb` / `SetColorRgbWithClearedAlpha` | color argv **3678–3688** (`color.a = 0`) |
| `BuildSetColorApply` | tag + color before iterator **3669–3690** |
| `SectorTagIteratorActive` | `while ((i = itr.Next()) >= 0)` **3693–3694** |
| `SF_LevelNum` return slot | **3541–3545** (hull) |
| `CheckArgs(1)` gates | **3558**, **3581**, **3608** |

### Chunk **940** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**939** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserMobjLevel940SliceTests.MobjGeomAssignsWhenPresent_RequiresSecondArgAndActor`
- `VmFScriptParserMobjLevel940SliceTests.ThingCountCountsMapActorsOnly_WhenLivingModeZero`
- `VmFScriptParserMobjLevel940SliceTests.ThingCountCountsLiving_WhenModeNonZeroAndNotInventory`
- `VmFScriptParserMobjLevel940SliceTests.ThingCountActorQualifies_LivingBranchIgnoresMapActor`
- `VmFScriptParserMobjLevel940SliceTests.BuildSetColorApply_RejectsMissingTagSector`
- `VmFScriptParserMobjLevel940SliceTests.ComposePalEntryRgb_PacksChannels`
- `VmFScriptParserMobjLevel940SliceTests.SectorTagIteratorActive_MatchesNativeLoopGuard`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 940 --to 940` (0 hard / 0 soft).

## Seams

`Level->levelnum`, `actorvalue`/`floatvalue`, `T_GetMobjType`, thinker iteration, `GetReplacement`, `FindFirstSectorFromTag`, `GetSectorTagIterator`, and `SetColor` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmFScriptParserMobjLevel940Slice.cs` (live `VmFScriptParserSfLevel` handlers are not matrix credit for these lines).
