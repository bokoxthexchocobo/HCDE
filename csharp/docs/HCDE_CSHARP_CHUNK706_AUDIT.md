# Chunk 706 — Whole `FSlide::HitSlideLine`, `SlideTraverse`, `SlideMove` (`p_map.cpp` 3036–3366).

## Scope

Whole `FSlide::HitSlideLine`, `SlideTraverse`, `SlideMove` (`p_map.cpp` 3036–3366).

## Native ranges

- `src/playsim/p_map.cpp` **FSlide::HitSlideLine** 3036–3160 (125 lines)
- `src/playsim/p_map.cpp` **FSlide::SlideTraverse** 3168–3240 (73 lines)
- `src/playsim/p_map.cpp` **FSlide::SlideMove** 3254–3366 (113 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmPMapSlideMove.cs`
Managed LOC (tally): 291.
Conversion commit: `25084d77eedd7c5f1bbbc3f45baf658b5f422a69`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `HitSlideLine` | `FSlide::HitSlideLine` 3036–3160 |
| `SlideTraverse` | `FSlide::SlideTraverse` 3168–3240 |
| `SlideMove` | `FSlide::SlideMove` 3254–3366 |


**Post-review ratio:** 291/311 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapSlideMoveTests.HitSlideLine_VerticalWallClearsXMove`
- `VmPMapSlideMoveTests.HitSlideLine_HorizontalWallClearsYMove`
- `VmPMapSlideMoveTests.SlideTraverse_PicksBlockingLineWithLowestFrac`
- `VmPMapSlideMoveTests.SlideMove_ReturnsEarlyWhenTryMoveFails`
- `VmPMapSlideMoveTests.SlideMove_SkipsWhenReactionTime`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 706 --to 706` (0 hard / 0 soft).

## Seams

Post-883 fix: `VmPMapUtlLineOpening.InterceptVector` direct; `SlideTryMove` calls `ActorPhysics.TryMove` when `Sim`+`MapActor` set; `SlideLines`/`BlockAllSlideLines` for traverse tests. Full `FPathTraverse`/`P_LineOpening` on level geometry: **no** until slide host binds sim.

## Credit rationale

Slide wall clip cluster; retires recognition `FSlide::SlideMove` band.
