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
Managed LOC (tally): 177.
Conversion commit: `b87f14f481cd2cc9ad07e7e3b43514f4e7005e60`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `BloodSplatsDisabled` / `SkipBleedActor` / `DefaultBleedDistance` / `JitterAngle` / `RandomBleedPitch` / `RandomBleedAngle` | `P_TraceBleed` 5245–5327 (5247: `if (!cl_bloodsplats)`; 5248: `return;`; 5254: `if ((actor->flags & MF_NOBLOOD) ||`) |
| `MissileBleedSkip` / `PitchFromMissileVel` | `P_TraceBleed` 5329–5361 (5344: `if (target == NULL || missile->flags3 & MF3_BLOODLESSIMPACT)`; 5346: `return;`; 5349: `if (missile->Vel.Z != 0)`) |
| `LineTargetBleedSkip` | `P_TraceBleed` 5369–5394 (5371: `if (t->linetarget == NULL || puff->flags3 & MF3_BLOODLESSIMPACT)`; 5373: `return;`; 5388: `if (target != NULL)`) |
| `WallHitCreatesDecal` / `EvaluateRailActorHit` / `PortalCrossing` / `NonActorHitStops` / `RailTraceFlags` / `RailShootZ` / `RailPuffFlags` / `RailStopAtOne` / `RailBleedAllowed` | `ProcessRailHit` 5426–5487 (5429: `if (res.HitType == TRACE_CrossingPortal)`; 5436: `return TRACE_Continue;`; 5439: `if (res.HitType != TRACE_HitActor)`) |
| `P_RailAttack` | `P_RailAttack` 5495–5567 (5500: `if (puffclass == NULL)`; 5513: `if (source->Level->localEventManager->WorldRailgunPreFired(damagetype…`; 5515: `return;`) |

### Out-of-body uncredit (482)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **277** |
| **Out-of-body removed** | **14** |
| **Honest** | **263** |


**Post-review ratio:** 177/263 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTraceBleedTests.SkipBleed_WhenNoBloodFlag`
- `VmPMapTraceBleedTests.PlanBleed_LowDamageCanSkip`
- `VmPMapTraceBleedTests.PlanBleed_HighDamageSmearChance`
- `VmPMapTraceBleedTests.DefaultBleedDistance_Fallback172`
- `VmPMapTraceBleedTests.MissileBleed_SkipsBloodless`
- `VmPMapTraceBleedTests.LineTargetBleed_SkipsNullTarget`
- `VmPMapTraceBleedTests.RailHit_SkipsThruActors`
- `VmPMapTraceBleedTests.RailHit_StopsAtInvuln`
- `VmPMapTraceBleedTests.RailShootZ_AddsAttackOffsetWhenNotCenterZ`
- `VmPMapTraceBleedTests.BuildRailData_ClearsThruWhenNoAllowFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 482 --to 482` (0 hard / 0 soft).

## Seams

`Trace`, `DImpactDecal::StaticCreate`, `P_RailAttack`, and `pr_tracebleed` are not hosted.

## Credit rationale

Credits blood-splat enablement, bleed overload dispatch, and rail trace portal/actor skip and stop rules from `p_map.cpp` (5245–5394, 5426–5487).
