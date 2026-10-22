# Chunk 726 — Whole `P_ExplodeMissile` (1958–2096) and `AActor::FloorBounceMissile` (2139–2274) in `p_mobj.cpp`.

## Scope

Whole `P_ExplodeMissile` (1958–2096) and `AActor::FloorBounceMissile` (2139–2274) in `p_mobj.cpp`.

## Native ranges

- `src/playsim/p_mobj.cpp` **P_ExplodeMissile** 1958–2096 (139 lines)
- `src/playsim/p_mobj.cpp` **AActor::FloorBounceMissile** 2139–2274 (136 lines)

**Credited native LOC:** 275 (tally `nativeLoc` 275).

## Managed

`VmPMobjExplodeBounce.cs`
Managed LOC (tally): 328.
Conversion commit: `99a9701790ed973bb6917e9149ecc73a862218af`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `ExplodeMissile` | `P_ExplodeMissile` 1958–2096 |
| `FloorBounceMissile` | `AActor::FloorBounceMissile` 2139–2274 |
| `ExplodeCountGated` | exp countdown 1966–1971 |
| `ApplyDehExplosionStyle` | DEH explosion 2064–2083 |


**Post-review ratio:** 328/275 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjExplodeBounceTests.ExplodeMissile_SkyWithoutSkyExplode_Destroyed`
- `VmPMobjExplodeBounceTests.ExplodeMissile_ClearsMissileFlag`
- `VmPMobjExplodeBounceTests.FloorBounceMissile_BounceCountZero_Explodes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 726 --to 726` (0 hard / 0 soft).

## Seams

`P_ProjectileHitLinedef`, `FindState`, `SetState`, `S_Sound`, `DImpactDecal`, `SpecialBounceHit`, `P_HitFloor`, `CallDie`, and `PlayBounceSound` are injected.

## Credit rationale

Missile death transition and floor/ceiling bounce reflection.
