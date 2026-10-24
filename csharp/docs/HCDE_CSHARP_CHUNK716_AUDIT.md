# Chunk 716 — Whole `P_UseTraverse` and `P_NoWayTraverse` (`p_map.cpp` 5829–5993).

## Scope

Whole `P_UseTraverse` and `P_NoWayTraverse` (`p_map.cpp` 5829–5993).

## Native ranges

- `src/playsim/p_map.cpp` **P_UseTraverse** 5829–5957 (129 lines)
- `src/playsim/p_map.cpp` **P_NoWayTraverse** 5972–5993 (22 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPMapUseTraverse.cs`
Managed LOC (tally): 176.
Conversion commit: `3363097faaf251f91c9d4e8aed8f828e88f97f6a`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `UseTraverse` / `ThingUseSpecial` / `BlockedUse` | `P_UseTraverse` 5829–5957 |
| `NoWayTraverse` | `P_NoWayTraverse` 5972–5993 |


**Post-review ratio:** 176/151 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUseTraverseTests.UseTraverse_ActivatesUseLine`
- `VmPMapUseTraverseTests.NoWayTraverse_BlocksWhenRangeZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 716 --to 716` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_LineOpening`, `P_ActivateLine`, VM `Used`, and sector actions remain native.

## Credit rationale

Use-line traversal and blocked-use oof path ported; trims recognition overlap.
