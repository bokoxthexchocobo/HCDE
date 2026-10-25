# Chunk 822 — `DBaseDecal` construction, wall link, stick, and Z helpers from `a_decals.cpp` (79–429).

## Scope

`DBaseDecal` construction, wall link, stick, and Z helpers from `a_decals.cpp` (79–429).

## Native ranges

- `src/playsim/a_decals.cpp` **DBaseDecal::Construct** 79–84 (6 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::Construct** 92–103 (12 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::Construct** 111–124 (14 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::OnDestroy** 132–136 (5 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::Remove** 144–156 (13 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::Serialize** 164–181 (18 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::GetXY** 189–210 (22 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::SetShade** 218–222 (5 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::SetShade** 230–233 (4 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::StickToWall** 241–330 (90 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::GetRealZ** 338–390 (53 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::CalcFracPos** 398–429 (32 lines)

**Credited native LOC:** 274 (tally `nativeLoc` 274).

## Managed

`VmADecalBase.cs`
Managed LOC (tally): 213.
Conversion commit: `ce384c5c62a018282e4887112b6a2c7577227a17`.

## Padding review (2026-10-21)

| Managed | Native (`a_decals.cpp`) |
|---------|------------------------|
| `Construct*` / `RemoveFromWall` | **79–156** |
| `GetXY` / `SetShade*` | **189–233** |
| `StickToWall` | **241–330** |
| `GetRealZ` / `CalcFracPos` | **338–429** |

### Out-of-body uncredit (822)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **274** |
| **Out-of-body removed** | **0** |
| **Honest** | **274** |


**Post-review ratio:** 213/274 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmADecalBaseTests.ConstructFromZ_ClearsPicnum`
- `VmADecalBaseTests.GetXY_InterpolatesAlongWall`
- `VmADecalBaseTests.StickToWall_OneSidedMidTexture`
- `VmADecalBaseTests.GetRealZ_RelUpperAddsCeilingPlane`
- `VmADecalBaseTests.GetRealZ_AbsoluteWhenNoRelFlags`
- `VmADecalBaseTests.CalcFracPos_UsesDominantAxis`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 822 --to 822` (0 hard / 0 soft).

## Seams

`TexMan`, `ColorMatcher`, `FSerializer`, and live `side_t` / `sector_t` graphs are injected.

## Credit rationale

First tally credit for base decal placement on walls and relative Z reconstruction.
