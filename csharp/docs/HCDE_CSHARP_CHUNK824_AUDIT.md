# Chunk 824 — Decal spread helpers from `a_decals.cpp` (437–622).

## Scope

Decal spread helpers from `a_decals.cpp` (437–622).

## Native ranges

- `src/playsim/a_decals.cpp` **GetWallStuff** 437–452 (16 lines)
- `src/playsim/a_decals.cpp` **Length** 460–463 (4 lines)
- `src/playsim/a_decals.cpp` **NextWall** 471–487 (17 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::SpreadLeft** 495–537 (43 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::SpreadRight** 545–583 (39 lines)
- `src/playsim/a_decals.cpp` **DBaseDecal::Spread** 591–622 (32 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151) = **16 + 4 + 17 + 43 + 39 + 32** (inclusive whole-function lines; not padded to clear the 150 floor).

### Adjacent uncredited `a_decals.cpp` (not in this row)

| Symbol | Lines | Why deferred |
| --- | --- | --- |
| `DBaseDecal::CalcFracPos` | 398–429 | Credited in chunk **822** (base decal Z/frac). |
| *(comment / blank only)* | 430–436 | — |
| `DBaseDecal::CloneSelf` | 630–649 | Whole function; shared by spread **and** impact-decal spawn — next band is **impact / static create**, not spread helpers. |
| `DImpactDecal::CheckMax` | 657–670 | Impact-decal cap policy. |
| `DImpactDecal::Expired` | 678–681 | Impact-decal lifetime. |
| `DImpactDecal::StaticCreate` (name overload) | 689–701 | Library lookup entry. |
| `DImpactDecal::StaticCreate` (template overload) | 709–762 | Calls `StickToWall`, `CheckMax`, then **`Spread`** (824) via seam. |
| `DImpactDecal::CloneSelf` | 770–803 | Impact-specific clone. |
| `SprayDecal` | 811+ | Separate weapon spray entry. |

### Boundary on native grounds

- **Lower bound 437:** first symbol of the spread subsystem — static **`GetWallStuff`** used only by **`SpreadLeft` / `SpreadRight` / `Spread`** (not by 822’s wall-stick band).
- **Upper bound 622:** end of **`DBaseDecal::Spread`**; native **`StaticCreate`** at **759** calls `decal->Spread(...)` but the spawn/clone/check-max cluster (**630–762**) is a distinct lifecycle unit.
- **151 vs 150:** smallest whole-function credit that includes all spread walkers + **`Spread`**; dropping any credited symbol breaks whole-function policy; adding **`CloneSelf`** without the impact cluster would split a shared helper awkwardly.

**Regroup verdict:** **No revert** — not floor-tuned; extending to **200–350** belongs on a future row (e.g. **630–762** impact static create + clone) rather than merging unrelated 822 credit.

## Headline gross vs net

| Anchor | `convertedBehaviorNative` |
| --- | ---: |
| After chunk **823** tally (`06c91f34`) | **96,328** |
| After chunk **824** tally (`3b981b7a`) | **96,479** |
| **Net** | **+151** (= row gross **151**) |

## Managed

`VmADecalSpread.cs`
Managed LOC (tally): 76.
Conversion commit: `f52611f984b3574569e68dbaa97e9f7a1d8f2565`.

## Padding review (2026-10-21)

| Managed | Native (`a_decals.cpp`) |
|---------|------------------------|
| `GetWallStuff` / `Length` / `NextWall` | **437–487** |
| `SpreadLeft` / `SpreadRight` / `Spread` | **495–622** |

**Post-review ratio:** 76/151 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmADecalSpreadTests.GetWallStuff_FlipsDeltaWhenNotSide0`
- `VmADecalSpreadTests.Length_MatchesHypot`
- `VmADecalSpreadTests.Spread_CallsLeftAndRight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 824 --to 824` (0 hard / 0 soft).

## Seams

`TexMan`, `CloneSelf`, and `side_t` adjacency are injected.

## Credit rationale

First tally credit for blood/decal spread along wall chains.
