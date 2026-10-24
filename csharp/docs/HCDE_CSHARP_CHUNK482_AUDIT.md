# Chunk 482 — P_TraceBleed overloads and ProcessRailHit skip/stop policy

## Scope

P_TraceBleed overloads and ProcessRailHit skip/stop policy

## Native ranges

- `src/playsim/p_map.cpp` **P_TraceBleed** 5245–5327 (83 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5329–5361 (33 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5369–5394 (26 lines)
- `src/playsim/p_map.cpp` **ProcessRailHit** 5426–5487 (62 lines)
- `src/playsim/p_map.cpp` **P_RailAttack** 5495–5567 (73 lines)

**Credited native LOC:** 277 (tally `nativeLoc` 277).

## Managed

(see conversion commit file list)
Managed LOC (tally): 126.
Conversion commit: `6d3cdb179be87ec22525d377b882e9b18b1bbbe2`.

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
