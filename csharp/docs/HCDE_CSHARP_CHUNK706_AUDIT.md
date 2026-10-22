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
Managed LOC (tally): 238.
Conversion commit: `3cd3dfcd2ad123cd8ae9558aa03a0476fca5352f`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `HitSlideLine` | `FSlide::HitSlideLine` 3036–3160 |
| `SlideTraverse` | `FSlide::SlideTraverse` 3168–3240 |
| `SlideMove` | `FSlide::SlideMove` 3254–3366 |


**Post-review ratio:** 238/311 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapSlideMoveTests.HitSlideLine_VerticalWallClearsXMove`
- `VmPMapSlideMoveTests.SlideTraverse_PicksBlockingLineWithLowestFrac`
- `VmPMapSlideMoveTests.SlideMove_SkipsWhenReactionTime`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 706 --to 706` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_LineOpening`, `P_IsBlockedByLine`, `P_TestMobjZ`, `P_TryMove`, `P_CheckSlopeWalk`, and `S_Sound` are injected.

## Credit rationale

Slide wall clip cluster; retires recognition `FSlide::SlideMove` band.
