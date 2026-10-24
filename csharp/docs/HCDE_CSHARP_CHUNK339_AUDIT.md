# Chunk 339 — MBF CreateFindTracer through weapon bullet emitters

## Scope

MBF CreateFindTracer through weapon bullet emitters

## Native ranges

- `src/gamedata/d_dehacked.cpp` **CreateFindTracer through CreateWeaponMeleeAttack** 936–1098 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmDehackedMbfPointerEmitters.cs`, `VmDehackedMbfPointerEmittersTests.cs`
Managed LOC (tally): 28.
Conversion commit: `01ed52cec6f788294d93c451194c49174b1e9c65`.

## Tests

- `VmDehackedMbfPointerEmittersTests.FindTracerArgs_DefaultsRange`
- `VmDehackedMbfPointerEmittersTests.SeekerMissileArgs_AppliesDefaults`
- `VmDehackedMbfPointerEmittersTests.Tracer2Angle_UsesDefaultWhenZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 339 --to 339` (0 hard / 0 soft).

## Seams

`MBFParamState` and `FunctionCallEmitter` are not re-hosted; managed code mirrors default-argument behavior from the native emitter thunks.

## Credit rationale

Credits uncredited `d_dehacked.cpp` 936–1098: tracer/seeker/tracer2/jump/flag-set and weapon attack emitter parameter packing including DEH alias defaults (50/10 seeker angles, 19.6875 tracer2, weapon bullet counts).
