# Chunk 528 — AActor::CheckLOF

## Scope

AActor::CheckLOF

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::CheckLOF** 2358–2516 (159 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmActionCheckLof.cs`
Managed LOC (tally): 134.
Conversion commit: `865297b53c10b2913e3f925e79b889c3b340e3bd`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `AActor::CheckLOF` guard/aim policy **2358–2516** (`p_actionfunctions.cpp`). Excluded `Trace`/`CheckLOFTraceFunc`/`COPY_AAPTR`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `TargetNullWithoutAllow` | **2453–2468** |
| `ScaleHeightOffset` | **2385–2395** |
| `ScaleWidthOffset` / `ScaleOffsets` | **2397–2400**, **2434** |
| `TraceOriginZ` | **2403–2409** |
| `RangeExceeded` / `ResolveRange` | **2414–2420**, **2485–2487** |
| `AdjustPitchRadians` / `VecToAngle` | **2440–2450** |
| `JumpOnMissSuccess` / `TraceSuccess` | **2501–2502** |
| `MinRangeFail` | **2504–2506** |
| `ShouldLinkHitActor` | **2508–2512** |

### Out-of-body uncredit (528)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **0** |
| **Honest** | **159** |


**Post-review ratio:** 134/159 managed/native ≈ 84% (src file LOC at HEAD; see tally).

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
