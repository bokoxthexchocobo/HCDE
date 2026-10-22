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

**Credited native LOC:** 151 (tally `nativeLoc` 151).

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
