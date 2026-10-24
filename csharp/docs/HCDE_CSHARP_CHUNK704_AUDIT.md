# Chunk 704 — Whole `P_CheckOnmobj` and `P_TestMobjZ` (`p_map.cpp` 2077–2247).

## Scope

Whole `P_CheckOnmobj` and `P_TestMobjZ` (`p_map.cpp` 2077–2247).

## Native ranges

- `src/playsim/p_map.cpp` **P_CheckOnmobj** 2077–2096 (20 lines)
- `src/playsim/p_map.cpp` **P_TestMobjZ** 2104–2247 (144 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmPMapTestMobjZ.cs`
Managed LOC (tally): 198.
Conversion commit: `274c1c5c2d5581206689d66b4ec2c9d5e216970a`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `CheckOnmobj` | `P_CheckOnmobj` 2077–2096 |
| `TestMobjZ` | `P_TestMobjZ` 2104–2247 |


**Post-review ratio:** 198/164 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTestMobjZTests.CheckOnmobj_RestoresZAndCollides`
- `VmPMapTestMobjZTests.TestMobjZ_NoclipPasses`
- `VmPMapTestMobjZTests.TestMobjZ_BlocksOverlappingSolid`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 704 --to 704` (0 hard / 0 soft).

## Seams

`P_FakeZMovement`, `FMultiBlockThingsIterator`, `P_CollidedWith`, `P_ShouldPassThroughPlayer`, `P_CanCollideWith`, and `CheckRipLevel` are injected.

## Credit rationale

Vertical mobj overlap probe used by movement/fake-Z.
