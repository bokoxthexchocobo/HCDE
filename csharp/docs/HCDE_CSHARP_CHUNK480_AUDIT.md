# Chunk 480 — P_LineAttack name overload, CheckLineTrace, P_LineTrace, P_LinePickActor

## Scope

P_LineAttack name overload, CheckLineTrace, P_LineTrace, P_LinePickActor

## Native ranges

- `src/playsim/p_map.cpp` **P_LineAttack** 5052–5067 (16 lines)
- `src/playsim/p_map.cpp` **CheckLineTrace** 5083–5100 (18 lines)
- `src/playsim/p_map.cpp` **P_LineTrace** 5102–5200 (99 lines)
- `src/playsim/p_map.cpp` **P_LinePickActor** 5208–5237 (30 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPMapLineTrace.cs`, `VmPMapLineTraceTests.cs`
Managed LOC (tally): 147.
Conversion commit: `996b2d683031bfcd15972efd99154bf77615529e`.

## Tests

- `VmPMapLineTraceTests.PuffTypeMissing_WhenNotFound`
- `VmPMapLineTraceTests.CheckLineTrace_SkipsPortal`
- `VmPMapLineTraceTests.CheckLineTrace_SkipsThruSpecies`
- `VmPMapLineTraceTests.BuildLineFlags_ClearsBlockOnThrublock`
- `VmPMapLineTraceTests.PortalCount_SubtractsTwo`
- `VmPMapLineTraceTests.LinePick_ReturnsActorOnHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 480 --to 480` (0 hard / 0 soft).

## Seams

`Trace`, `PClass::FindActor`, and live sector queries are not hosted.

## Credit rationale

Credits puff-type lookup wrapper, portal-crossing trace filter, line-trace flag/start/outdata fill, and line-pick actor selection from `p_map.cpp` (5052–5237; class `P_LineAttack` body remains recognition).
