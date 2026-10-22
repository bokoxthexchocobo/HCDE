# Chunk 825 — `DBaseDecal::CloneSelf`, `DImpactDecal` spawn helpers, and `SprayDecal` from `a_decals.cpp` (630–803, 811–849). Gap **430–436** and **804–810** are comment-only (no whole function). SprayDecal band included so row `nativeLoc` ≥ 150 (impact cluster alone is 139).

## Scope

`DBaseDecal::CloneSelf`, `DImpactDecal` spawn helpers, and `SprayDecal` from `a_decals.cpp` (630–803, 811–849). Gap **430–436** and **804–810** are comment-only (no whole function). SprayDecal band included so row `nativeLoc` ≥ 150 (impact cluster alone is 139).

## Native ranges

- `src/playsim/a_decals.cpp` **DBaseDecal::CloneSelf** 630–649 (20 lines)
- `src/playsim/a_decals.cpp` **DImpactDecal::CheckMax** 657–670 (14 lines)
- `src/playsim/a_decals.cpp` **DImpactDecal::Expired** 678–681 (4 lines)
- `src/playsim/a_decals.cpp` **DImpactDecal::StaticCreate** 689–701 (13 lines)
- `src/playsim/a_decals.cpp` **DImpactDecal::StaticCreate** 709–762 (54 lines)
- `src/playsim/a_decals.cpp` **DImpactDecal::CloneSelf** 770–803 (34 lines)
- `src/playsim/a_decals.cpp` **SprayDecal** 811–849 (39 lines)

**Credited native LOC:** 178 (tally `nativeLoc` 178).

## Managed

`VmADecalImpact.cs`, `VmADecalSpray.cs`
Managed LOC (tally): 314.
Conversion commit: `07c55f63dc5ad6bfa40c73590d822411e6168a40`.

## Padding review (2026-10-21)

| Managed | Native (`a_decals.cpp`) |
|---------|------------------------|
| `CloneSelf` | **630–649** |
| `CheckMax` | **657–670** |
| `Expired` | **678–681** |
| `StaticCreate` (name) | **689–701** |
| `StaticCreate` (template) | **709–762** |
| `DImpactDecal::CloneSelf` | **770–803** |
| `SprayDecal` | **811–849** |

**Post-review ratio:** 314/178 managed/native ≈ 176% (src file LOC at HEAD; see tally).

## Tests

- `VmADecalImpactTests.CloneSelf_CopiesRenderMask`
- `VmADecalImpactTests.CheckMax_TrimsWhenAtCap`
- `VmADecalImpactTests.Expired_DecrementsCount`
- `VmADecalImpactTests.StaticCreate_SpreadDisabled_SkipsSpreadBranch`
- `VmADecalImpactTests.ImpactCloneSelf_RejectsNoAutodecalsWall`
- `VmADecalSprayTests.SprayDecal_NullShooter_NoOp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 825 --to 825` (0 hard / 0 soft).

## Seams

`Level` thinker factory, `DecalLibrary`, `cl_maxdecals` / `cl_spreaddecals`, and live `side_t` graphs are injected. **`StaticCreate` → `VmADecalSpread.Spread` is a direct static call** (no spread seam/forwarder).

## Credit rationale

First tally credit for impact decal cap, static spawn, and clone paths that call spread in native order.
