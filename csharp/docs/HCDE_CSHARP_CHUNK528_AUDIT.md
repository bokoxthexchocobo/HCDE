# Chunk 528 — AActor::CheckLOF

## Scope

AActor::CheckLOF

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::CheckLOF** 2358–2517 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`csharp/src/HCDE.Playsim/VmActionCheckLof.cs` (src-only credit).

Managed LOC (tally): 134.

## Tests

- `VmActionCheckLofTests.TargetNullWithoutAllow_FailsWhenStrict`
- `VmActionCheckLofTests.RangeExceeded_IgnoresPartialFlag`
- `VmActionCheckLofTests.ResolveRange_PlayerUsesPlayerMissileRange`
- `VmActionCheckLofTests.AdjustPitch_AimVertNoOffset_UsesHeightInDelta`
- `VmActionCheckLofTests.TraceSuccess_JumpOnMiss_AllowsNonActorHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 528 --to 528` (0 hard / 0 soft).

## Seams

`Trace`, `CheckLOFTraceFunc`, `COPY_AAPTR`, and `ACTION_RETURN_BOOL` are not hosted.

## Credit rationale

Credits CheckLOF offset scaling, range gates, aim flags, trace success policy, and actor link flags from `p_actionfunctions.cpp` (2358–2517).

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `ScaleHeightOffset`, `ScaleWidthOffset`, `TraceOriginZ` | 2385–2410 |
| `AimFromTarget`, `AimAllowNullTarget` | 2412–2464 |
| `RangeExceeded`, `ResolveRange` | 2414–2420, 2485–2488 |
| `VelocityFromPitch`, `TraceSuccess`, `MinRangeFail`, `ShouldLinkHitActor` | 2471–2514 |

**Removed:** Entire `EvaluateCheckLofTrace` block (ports `CheckLOFTraceFunc` **2286–2355**, outside this chunk’s credited range); extra CLOF constants only used by trace callback; `ShouldSetTarget`/`Master`/`Tracer` single-bit helpers; `ResolveHorizontalAngleRadians`, `ZeroRangeUsesDefaults`, `SuccessPath`.

**Ratio ~84%:** Managed > native because aim orchestration is split into `AimFromTarget`/`AimAllowNullTarget` with explicit parameters vs. inlined `DAngle`/`DVector` temporaries in C++.
