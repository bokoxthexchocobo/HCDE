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
Managed LOC (tally): 316.
Conversion commit: `31f52a8797f91b7a4de001f7c459ef372e67fe0c`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `HitSlideLine` | `FSlide::HitSlideLine` 3036–3160 |
| `SlideTraverse` | `FSlide::SlideTraverse` 3168–3240 |
| `SlideMove` | `FSlide::SlideMove` 3254–3366 |

### Out-of-body uncredit (706)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **311** |
| **Out-of-body removed** | **0** |
| **Honest** | **311** |


**Post-review ratio:** 316/311 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapSlideMoveTests.HitSlideLine_VerticalWallClearsXMove`
- `VmPMapSlideMoveTests.HitSlideLine_HorizontalWallClearsYMove`
- `VmPMapSlideMoveTests.SlideTraverse_PicksBlockingLineWithLowestFrac`
- `VmPMapSlideMoveTests.SlideMove_ReturnsEarlyWhenTryMoveFails`
- `VmPMapSlideMoveTests.SlideMove_SkipsWhenReactionTime`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 706 --to 706` (0 hard / 0 soft).

## Seams

`VmPMapUtlLineOpening.InterceptVector` direct; `SlideTryMove` always `ActorPhysics.TryMove` via required `Sim`+`MapActor`. `SlideLines`/`BlockAllSlideLines` for traverse tests only; production slide host must supply level `SlideLines` from `FPathTraverse` (not matrix-credited here).

## Credit rationale

Slide wall clip cluster; retires recognition `FSlide::SlideMove` band.
