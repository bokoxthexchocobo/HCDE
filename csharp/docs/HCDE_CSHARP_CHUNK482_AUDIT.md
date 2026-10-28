# Chunk 482 — P_TraceBleed overloads and ProcessRailHit skip/stop policy

## Scope

P_TraceBleed overloads and ProcessRailHit skip/stop policy

## Native ranges

- `src/playsim/p_map.cpp` **P_TraceBleed** 5245–5327 (83 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5329–5332 (4 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5340–5361 (22 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5369–5378 (10 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5386–5394 (9 lines)
- `src/playsim/p_map.cpp` **ProcessRailHit** 5426–5487 (62 lines)
- `src/playsim/p_map.cpp` **P_RailAttack** 5495–5567 (73 lines)

**Credited native LOC:** 263 (tally `nativeLoc` 263).

## Managed

`VmPMapTraceBleed.cs`
Managed LOC (tally): 366.
Conversion commit: `5eff1ff6b3f96a575884d3be9fc84ad74b373fdd`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `BloodSplatsDisabled` / `SkipBleedActor` / `DefaultBleedDistance` / `JitterAngle` / `RandomBleedPitch` / `RandomBleedAngle` | `P_TraceBleed` 5245–5327 (5247: `if (!cl_bloodsplats)`; 5248: `return;`; 5254: `if ((actor->flags & MF_NOBLOOD) ||`) |
| `MissileBleedSkip` / `PitchFromMissileVel` | `P_TraceBleed` 5329–5361 (5344: `if (target == NULL || missile->flags3 & MF3_BLOODLESSIMPACT)`; 5346: `return;`; 5349: `if (missile->Vel.Z != 0)`) |
| `LineTargetBleedSkip` | `P_TraceBleed` 5369–5394 (5371: `if (t->linetarget == NULL || puff->flags3 & MF3_BLOODLESSIMPACT)`; 5373: `return;`; 5388: `if (target != NULL)`) |
| `WallHitCreatesDecal` / `EvaluateRailActorHit` / `PortalCrossing` / `NonActorHitStops` / `RailTraceFlags` / `RailShootZ` / `RailPuffFlags` / `RailStopAtOne` / `RailBleedAllowed` | `ProcessRailHit` 5426–5487 (5429: `if (res.HitType == TRACE_CrossingPortal)`; 5436: `return TRACE_Continue;`; 5439: `if (res.HitType != TRACE_HitActor)`) |
| `P_RailAttack` | `P_RailAttack` 5495–5567 (5500: `if (puffclass == NULL)`; 5513: `if (source->Level->localEventManager->WorldRailgunPreFired(damagetype…`; 5515: `return;`) |
| `ShouldRunTraceBleed` / `PlanBleedSprayPass` / `BuildBleedTraceIteration` / `TryPrepareWallDecal` / `RunMissileTargetBleed` / `RunLineTargetBleed` / `RunActorOnlyBleed` / `ProcessRailHitPolicy` / `AfterRailActorRecorded` / `BuildRailAttackSetup` / `RunTraceBleedSprayLoop` | bleed spray loop **5294–5326**, overload dispatch **5329–5394**, `ProcessRailHit` **5426–5487**, rail setup **5524–5567** |

### Out-of-body uncredit (482)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **277** |
| **Out-of-body removed** | **14** |
| **Honest** | **263** |


**Post-review ratio:** 366/263 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTraceBleedTests.SkipBleed_WhenNoBloodFlag`
- `VmPMapTraceBleedTests.PlanBleed_LowDamageCanSkip`
- `VmPMapTraceBleedTests.PlanBleed_HighDamageSmearChance`
- `VmPMapTraceBleedTests.DefaultBleedDistance_Fallback172`
- `VmPMapTraceBleedTests.TryPrepareWallDecal_DimsColor`
- `VmPMapTraceBleedTests.MissileBleed_SkipsBloodless`
- `VmPMapTraceBleedTests.RunMissileTargetBleed_ComputesPitch`
- `VmPMapTraceBleedTests.LineTargetBleed_SkipsNullTarget`
- `VmPMapTraceBleedTests.RailHit_SkipsThruActors`
- `VmPMapTraceBleedTests.RailHit_StopsAtInvuln`
- `VmPMapTraceBleedTests.ProcessRailHitPolicy_StopsOnWall`
- `VmPMapTraceBleedTests.RailShootZ_AddsAttackOffsetWhenNotCenterZ`
- `VmPMapTraceBleedTests.BuildRailData_ClearsThruWhenNoAllowFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 482 --to 482` (0 hard / 0 soft).

## Seams

`Trace`, `DImpactDecal::StaticCreate`, `P_RailAttack`, and `pr_tracebleed` are not hosted.

## Credit rationale

Credits blood-splat enablement, bleed overload dispatch, and rail trace portal/actor skip and stop rules from `p_map.cpp` (5245–5394, 5426–5487).
