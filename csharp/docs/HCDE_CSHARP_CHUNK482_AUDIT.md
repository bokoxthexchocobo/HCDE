# Chunk 482 — P_TraceBleed wall blood decals and missile aim overload

## Scope

P_TraceBleed wall blood decals and missile aim overload

## Native ranges

- `src/playsim/p_map.cpp` **P_TraceBleed** 5245–5327 (83 lines)
- `src/playsim/p_map.cpp` **P_TraceBleed** 5329–5357 (29 lines)

**Credited native LOC:** 112 (tally `nativeLoc` 112).

## Managed

`VmPMapTraceBleed.cs`, `VmPMapTraceBleedTests.cs`
Managed LOC (tally): 69.
Conversion commit: `233e46107a439af4cf1fde7a9d9c2144cbf92a3a`.

## Tests

- `VmPMapTraceBleedTests.SkipBleed_WhenNoBloodFlag`
- `VmPMapTraceBleedTests.PlanBleed_LowDamageCanSkip`
- `VmPMapTraceBleedTests.PlanBleed_HighDamageSmearChance`
- `VmPMapTraceBleedTests.DefaultBleedDistance_Fallback172`
- `VmPMapTraceBleedTests.MissileBleed_SkipsBloodless`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 482 --to 482` (0 hard / 0 soft).

## Seams

`Trace`, `DImpactDecal::StaticCreate`, and `pr_tracebleed` are not hosted.

## Credit rationale

Credits blood-splat enablement, damage-tier spray plans, jittered trace directions, wall decal color dimming, and missile pitch bleed entry from `p_map.cpp` (5245–5357).
