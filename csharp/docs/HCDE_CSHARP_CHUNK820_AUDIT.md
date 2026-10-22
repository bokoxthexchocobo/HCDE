# Chunk 820 — `DScroller` and BOOM scroll line specials from `mapthinkers/a_scroll.cpp` (52–493).

## Scope

`DScroller` and BOOM scroll line specials from `mapthinkers/a_scroll.cpp` (52–493).

## Native ranges

- `src/playsim/mapthinkers/a_scroll.cpp` **operator &** 52–55 (4 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **DScroller::Serialize** 63–81 (19 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **RotationComp** 90–105 (16 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **DScroller::Tick** 130–243 (114 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **DScroller::Construct** 265–321 (57 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **DScroller::OnDestroy** 323–334 (12 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **DScroller::Construct** 347–386 (40 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **SetWallScroller** 394–456 (63 lines)
- `src/playsim/mapthinkers/a_scroll.cpp` **SetScroller** 458–493 (36 lines)

**Credited native LOC:** 361 (tally `nativeLoc` 361).

## Managed

`VmAScroll.cs`
Managed LOC (tally): 237.
Conversion commit: `2240433339d3751fff4352e86abd3c5658237e0b`.

## Padding review (2026-10-21)

| Managed | Native (`a_scroll.cpp`) |
|---------|-------------------------|
| `ScrollPosAnd` | `operator&` **52–55** |
| `RotationComp` / `TickStep` | **63–243** |
| `InitSectorScroller` / `ClearInterpolations` | **265–334** |
| wall `Construct` | **347–386** |
| `SetWallScroller` / `SetScroller` | **394–493** |

**Post-review ratio:** 237/361 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmAScrollTests.ScrollPosAnd_IntersectsFlags`
- `VmAScrollTests.RotationComp_IdentityWhenAngleZero`
- `VmAScrollTests.TickStep_ScrollsFloorWhenNonZero`
- `VmAScrollTests.TickStep_AccelAccumulatesVelocity`
- `VmAScrollTests.SetWallScroller_ZeroRateDoesNotCreate`
- `VmAScrollTests.SetScrollerPass_CreatesWhenNoMatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 820 --to 820` (0 hard / 0 soft).

## Seams

Live `sector_t` / `side_t` / thinker iterators and `SetInterpolation` hooks are injected.

## Credit rationale

First tally credit for texture scrollers and tagged wall/sector scroll setup.
