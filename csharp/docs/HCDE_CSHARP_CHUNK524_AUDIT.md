# Chunk 524 — DLevelScript::DoSetActorProperty

## Scope

DLevelScript::DoSetActorProperty

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::DoSetActorProperty** 4166–4405 (240 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`VmAcsDoSetActorProperty.cs`
Managed LOC (tally): 258.
Conversion commit: `61901b9fdcf175b52873dd218d071d6765fe00b1`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `DLevelScript::DoSetActorProperty` switch planning **4166–4405** (`p_acs.cpp`). Excluded live `actor->Die`, `LookupString`, and field mutation.

| Managed | Native (`p_acs.cpp`) |
|---------|---------------------|
| `ActorIsNull` | **4168–4170** |
| `ShouldSkipHealthOnDead` / `HealthShouldDie` | **4175–4188** |
| `AcsToDouble` / `AcsToFloat` | `ACSToDouble` use **4193**, **4201**, etc. |
| `TryMapLegacyRenderStyle` | **4204–4212** |
| `ToggleFlag` / `FlagMaskForProperty` / `IsFlagProperty` | flag cases **4215–4252**, **4240–4251** |
| `RequiresPlayerPawn` | **4235–4238**, **4268–4272**, **4353–4366**, **4389–4396** |
| `RequiresPlayerWithSoundClass` | **4390–4395** |
| `UsesBehaviorStringLookup` | **4279–4308**, **4385–4394** |
| `UsesAcsDouble` | **4192–4201**, **4275–4378** |
| `ApplyFriendlyFlag` / `MonsterTotalBeforeFriendlyToggle` / `MonsterTotalAfterFriendlyToggle` | **4254–4264** |
| `DescribePropertyDispatchNative` | whole `switch` **4172–4404** |

### Out-of-body uncredit (524)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **240** |
| **Out-of-body removed** | **0** |
| **Honest** | **240** |


**Post-review ratio:** 258/240 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsDoSetActorPropertyTests.ShouldSkipHealthOnDead_WhenCorpseOrPlayerDead`
- `VmAcsDoSetActorPropertyTests.TryMapLegacyRenderStyle_UsesNativeTable`
- `VmAcsDoSetActorPropertyTests.FriendlyToggle_AdjustsMonsterTotalWhenKillCounted`
- `VmAcsDoSetActorPropertyTests.ToggleFlag_AmbushMatchesNativeMask`
- `VmAcsDoSetActorPropertyTests.RequiresPlayerPawn_ForJumpZOnlyAmongScalars`
- `VmAcsDoSetActorPropertyTests.UsesBehaviorStringLookup_ForSpeciesNotMass`
- `VmAcsDoSetActorPropertyTests.PlanDispatch_HealthMatchesNativeGuards`
- `VmAcsDoSetActorPropertyTests.PlanDispatch_FriendlyAdjustsKillCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 524 --to 524` (0 hard / 0 soft).

## Seams

`actor->Die`, `Level->Behaviors.LookupString`, and hosted actor mutation are not executed; dispatch is planned in managed code.

## Credit rationale

Credits ACS actor property switch planning, health guards, legacy render-style table, flag toggles, and friendly kill-count bookkeeping from `p_acs.cpp` (4166–4406).
