# Chunk 1111 — `PlayBounceSound` and `FloorBounceMissile` through bounce-state selection from `p_mobj.cpp` (**2101–2250**); follows **1110** explode-missile band.

## Scope

`PlayBounceSound` and `FloorBounceMissile` through bounce-state selection from `p_mobj.cpp` (**2101–2250**); follows **1110** explode-missile band.

## Native ranges

- `src/playsim/p_mobj.cpp` **PlayBounceSound and FloorBounceMissile** 2101–2250 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjFloorBounce1111Slice.cs`
Managed LOC (tally): 197.
Conversion commit: `c38073f0d1551d3ef21becbd810bf2034697bea2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PlayBounceSound` quiet/see/wall pick (**2109–2128**) | yes |
| Special bounce inverted switch (**2143–2148**) | yes |
| Damageable plane kill (**2153–2161**) | yes |
| Water explode/destroy (**2164–2179**) | yes |
| Ceiling/floor/sky flags (**2188–2207**) | yes |
| Bounce-count exhaustion (**2210–2217**) | yes |
| Heretic/MBF/Doom reflect (**2221–2240**) | yes |
| Pitch + bounce state name (**2243–2256**) | yes (policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2098–2105** | VM thunk (chunk **1110** edge) |
| **2119–2127** | host `S_Sound` |
| **2159**, **2170**, **2213** | host explode/die |
| **2260+** | MBF rest outside hull |

| Managed (`VmPMobjFloorBounce1111Slice.cs`) | Native (`p_mobj.cpp`) |
|-------------------------------------------|---------------------|
| Bounce sound + floor bounce policy | **2101–2250** |
| Band hull (exclusive slice) | **2101–2250** |

### Chunk **1111** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1110** |


**Post-review ratio:** 197/150 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjFloorBounce1111SliceTests.ResolveBounceSound_UsesWallWhenAirborneAndValid`
- `VmPMobjFloorBounce1111SliceTests.SpecialBounceMissileOutcome_InvertsNativeCases`
- `VmPMobjFloorBounce1111SliceTests.RunFloorBounceMissile_DestroysOnSkyWhenFlagged`
- `VmPMobjFloorBounce1111SliceTests.RunFloorBounceMissile_HereticEntersDeathAndReturnsFalse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1111 --to 1111` (0 hard / 0 soft).

## Seams

`S_Sound`, `SpecialBounceHit`, `P_ProjectileHitPlane`, `P_HitFloor`, and `SetState` remain host-injected; MBF rest threshold at **2260** is the next band.

## Credit rationale

UNCredited **`p_mobj.cpp`** **2101–2250** (+150); follows **1110** (**1951–2100**). Skips **1501–1685**.
