# Chunk 524 — DLevelScript::DoSetActorProperty

## Scope

DLevelScript::DoSetActorProperty

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::DoSetActorProperty** 4166–4406 (241 lines)

**Credited native LOC:** 241 (tally `nativeLoc` 241).

## Managed

`VmAcsDoSetActorProperty.cs`, `VmAcsDoSetActorPropertyTests.cs`

Managed LOC (tally): see `loc_tally.json` after `fix_chunk_tally.py 524`.

Conversion commit: see tally `conversionManagedCommit` (includes padding removal follow-up).

## Padding review (2026-10-20)

The first land of this chunk (`3ddd69fd`) included **ratio padding** that is **not** acceptable:

- Dozens of `Set*Property` methods that always returned `true` or trivial `>= 0` checks with no native mapping.
- Seven `*FromValue` helpers that duplicated `value != 0`.
- Constant-`true` seams such as `MasterTidRequiresLookup()`, `SpeciesUsesStringLookup()`, `PropertyHandled()`.
- A bogus `IsKnownProperty` fallback (`property < 64`) and incorrect `UsesAcsDouble` / `UsesDirectInt` opcode lists.

**Remediation:** Removed all of the above. Managed code now holds:

- Native `APROP_*` ids and `LegacyRenderStyleIndices` table from `p_acs.cpp`.
- Real guards (`ShouldSkipHealthOnDead`, friendly kill-count bookkeeping, flag masks/fields).
- `PlanDispatch` — a case-by-case plan struct mirroring the native `switch (property)` without hosted IO.
- `TryMapLegacyRenderStyle` loop matching the native render-style search.

No filler helpers remain solely to inflate LOC.

## Tests

- `VmAcsDoSetActorPropertyTests.ShouldSkipHealthOnDead_WhenCorpseOrPlayerDead`
- `VmAcsDoSetActorPropertyTests.TryMapLegacyRenderStyle_UsesNativeTable`
- `VmAcsDoSetActorPropertyTests.FriendlyToggle_AdjustsMonsterTotalWhenKillCounted`
- `VmAcsDoSetActorPropertyTests.ToggleFlag_AmbushMatchesNativeMask`
- `VmAcsDoSetActorPropertyTests.RequiresPlayerPawn_ForJumpZOnlyAmongScalars`
- `VmAcsDoSetActorPropertyTests.UsesBehaviorStringLookup_ForSpeciesNotMass`
- `VmAcsDoSetActorPropertyTests.PlanDispatch_HealthMatchesNativeGuards`
- `VmAcsDoSetActorPropertyTests.PlanDispatch_FriendlyAdjustsKillCount`
- `VmAcsDoSetActorPropertyTests.PlanDispatch_UnknownPropertyNoOp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test` filtered `VmAcsDoSetActorPropertyTests`; `check_port_coverage.sh 524` after padding removal.

## Seams

`actor->Die`, `Level->Behaviors.LookupString`, `S_FindSound`, `DoSetMaster`, and direct actor field writes are not hosted; `PlanDispatch` records intent only.

## Credit rationale

Credits ACS `DoSetActorProperty` switch structure, health/friendly/render-style/flag behavior from `p_acs.cpp` (4166–4406). Supersedes native line credit on legacy chunks 30 and 37 (ranges cleared in tally).
