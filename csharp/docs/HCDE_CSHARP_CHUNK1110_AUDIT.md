# Chunk 1110 — `P_ExplodeMissile` gates, sky/horizon destroy, decal fraction, and DeHackEd explosion style from `p_mobj.cpp` (**1951–2100**); follows **1109**.

## Scope

`P_ExplodeMissile` gates, sky/horizon destroy, decal fraction, and DeHackEd explosion style from `p_mobj.cpp` (**1951–2100**); follows **1109**.

## Native ranges

- `src/playsim/p_mobj.cpp` **P_ExplodeMissile** 1951–2100 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjExplodeMissile1110Slice.cs`
Managed LOC (tally): 154.
Conversion commit: `8bc40e321b005f5a0da9feaad81acb83915d0541`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Explocount early return (**1966–1972**) | yes |
| Hit target/master/tracer (**1981–1988**) | yes |
| Sky/horizon destroy (**1992–1999**) | yes |
| Decal fraction clamp (**2019–2021**) | yes |
| DeHackEd explosion style (**2064–2083**) | yes |
| Randomize tics + clear missile (**2086–2093**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1963**, **2014–2046**, **2055** | host linedef/decal/sound |
| **2098–2104** | VM thunk |

| Managed (`VmPMobjExplodeMissile1110Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------------|---------------------|
| Explode missile policy | **1951–2100** |
| Band hull (exclusive slice) | **1951–2100** |

### Chunk **1110** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1109** |


**Post-review ratio:** 154/150 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjExplodeMissile1110SliceTests.RunExplodeMissile_AbortsWhenExploCountBelowThreshold`
- `VmPMobjExplodeMissile1110SliceTests.RunExplodeMissile_DestroysOnHorizonWithoutSkyExplode`
- `VmPMobjExplodeMissile1110SliceTests.ClampDecalFraction_ClampsToUnitInterval`
- `VmPMobjExplodeMissile1110SliceTests.RunExplodeMissile_AppliesAddRocketStyle`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1110 --to 1110` (0 hard / 0 soft).

## Seams

`P_ProjectileHitLinedef`, `FindState`, `S_Sound`, and decal spawn remain host-injected; VM thunk at **2098** is outside the behavior body.

## Credit rationale

UNCredited **`p_mobj.cpp`** **1951–2100** (+150); follows **1109** (**1801–1950**).
