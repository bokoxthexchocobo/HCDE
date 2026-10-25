# Chunk 775 — Particle pool lifecycle, init/clear/subsector bucketing, palette init, jitter/splash spawners (`p_effect.cpp` 104–276, 401–660; excludes chunk708 think/run bands).

## Scope

Particle pool lifecycle, init/clear/subsector bucketing, palette init, jitter/splash spawners (`p_effect.cpp` 104–276, 401–660; excludes chunk708 think/run bands).

## Native ranges

- `src/playsim/p_effect.cpp` **FreeParticle** 104–129 (26 lines)
- `src/playsim/p_effect.cpp` **NewParticle** 131–158 (28 lines)
- `src/playsim/p_effect.cpp` **P_InitParticles** 165–181 (17 lines)
- `src/playsim/p_effect.cpp` **P_ClearParticles** 183–197 (15 lines)
- `src/playsim/p_effect.cpp` **P_FindParticleSubsectors** 204–239 (36 lines)
- `src/playsim/p_effect.cpp` **ParticleColor** 243–256 (14 lines)
- `src/playsim/p_effect.cpp` **ParticleColor** 258–261 (4 lines)
- `src/playsim/p_effect.cpp` **P_InitEffects** 263–276 (14 lines)
- `src/playsim/p_effect.cpp` **JitterParticle** 401–425 (25 lines)
- `src/playsim/p_effect.cpp` **MakeFountain** 427–455 (29 lines)
- `src/playsim/p_effect.cpp` **P_DrawSplash** 569–601 (33 lines)
- `src/playsim/p_effect.cpp` **P_DrawSplash2** 603–660 (58 lines)

**Credited native LOC:** 299 (tally `nativeLoc` 299).

## Managed

`VmPEffectParticlePool.cs`, `VmPEffectJitterSplash.cs`
Managed LOC (tally): 434.
Conversion commit: `6092939f0bd2540c558dc26ebbbc6f09a31530c5`.

## Padding review (2026-10-21)

**Trim (2026-10-22):** removed summary XML, `FadeFromTtl` helper, and redundant `JitterParticle` overload.

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `FreeParticle` / `NewParticle` | **104–158** |
| `InitParticles` / `ClearParticles` | **165–197** |
| `FindParticleSubsectors` | **204–239** |
| `ParticleColor` / `InitEffects` | **243–276** |
| `JitterParticle` | **401–425** |
| `MakeFountain` | **427–455** |
| `DrawSplash` | **569–601** |
| `DrawSplash2` | **603–660** |

### Out-of-body uncredit (775)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **300** |
| **Out-of-body removed** | **1** |
| **Honest** | **299** |


**Post-review ratio:** 434/299 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmPEffectParticlePoolTests.FreeParticle_ReturnsToInactiveList`
- `VmPEffectParticlePoolTests.NewParticle_LinksInactiveChain`
- `VmPEffectParticlePoolTests.InitParticles_ClampsCount`
- `VmPEffectParticlePoolTests.FindParticleSubsectors_BucketsActiveParticles`
- `VmPEffectParticlePoolTests.InitEffects_WritesNamedPalette`
- `VmPEffectJitterSplashTests.JitterParticle_SetsTtlAndAlpha`
- `VmPEffectJitterSplashTests.MakeFountain_SkipsOddTics`
- `VmPEffectJitterSplashTests.DrawSplash_SpawnsSparkParticles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 775 --to 775` (0 hard / 0 soft).

## Seams

`Args`/`r_maxparticles`, `ColorMatcher`, `gameinfo`, visual-thinker list, `PointInRenderSubsector`, and `r_particles` gate are injected.

## Credit rationale

Credits uncited `p_effect.cpp` pool/splash tail after chunk708 particle run.
