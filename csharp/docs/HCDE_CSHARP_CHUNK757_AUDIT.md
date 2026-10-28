# Chunk 757 — `P_SpawnParticle`, jitter/fountain helpers, and splash drawers (`p_effect.cpp` 353–660).

## Scope

`P_SpawnParticle`, jitter/fountain helpers, and splash drawers (`p_effect.cpp` 353–660).

## Native ranges

- `src/playsim/p_effect.cpp` **P_SpawnParticle** 353–394 (42 lines)
- `src/playsim/p_effect.cpp` **JitterParticle** 401–425 (25 lines)
- `src/playsim/p_effect.cpp` **MakeFountain** 427–455 (29 lines)
- `src/playsim/p_effect.cpp` **P_DrawSplash** 569–601 (33 lines)
- `src/playsim/p_effect.cpp` **P_DrawSplash2** 603–660 (58 lines)

**Credited native LOC:** 187 (tally `nativeLoc` 187).

## Managed

`VmPEffectJitterSplash.cs`
Managed LOC (tally): 115.
Conversion commit: `c5d92b8720d91f2ea421afb886f14eb9bde11150`.

## Padding review (2026-10-21)

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `SpawnParticleFadeStep` / negative fade flags | `P_SpawnParticle` 367–375 |
| `SpawnParticleLocalAnim` | `P_SpawnParticle` 389–392 |
| `JitterParticleTtlDefault` | `JitterParticle` overload 401–404 |
| `JitterDriftVelocity` / `JitterDriftAcceleration` | drift init 414–418 |
| `ShouldSkipFountainOnOddTic` | `MakeFountain` 431–432 |
| `FountainOutRadius` / `FountainInnerJet` / `FountainInnerZBoost` / `FountainOuterZBoost` / `FountainGravityAccel` | `MakeFountain` 438–446 |
| `PickFountainColor` / `PickFountainSize` | `MakeFountain` 447–453 |
| `SplashKindSupported` / `PickSplashColor` | `P_DrawSplash` 573–591 |
| `Splash2ColorKind` / `Splash2ZSpread` / `Splash2ZAdd` / `Splash2UsesDirectionalVelocity` | `P_DrawSplash2` 608–657 |


**Post-review ratio:** 115/187 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmPEffectJitterSplashTests.JitterDriftVelocity_ScalesRandom`
- `VmPEffectJitterSplashTests.FountainInnerJet_WhenCloseToCenter`
- `VmPEffectJitterSplashTests.SplashKindSupported_OnlySpark`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 757 --to 757` (0 hard / 0 soft).

## Seams

`NewParticle`, `M_Random`, and `ParticleColor` are injected; no hosted particle heap.

## Credit rationale

Credits uncredited particle jitter/splash band before `P_RunEffect` / rail trail cluster.
