# Chunk 734 — Whole static `Grind`, native action wrapper, and `CallGrind` (`p_mobj.cpp` 1554–1705).

## Scope

Whole static `Grind`, native action wrapper, and `CallGrind` (`p_mobj.cpp` 1554–1705).

## Native ranges

- `src/playsim/p_mobj.cpp` **Grind** 1554–1685 (132 lines)
- `src/playsim/p_mobj.cpp` **Grind** 1687–1705 (19 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPMobjGrind.cs`
Managed LOC (tally): 130.
Conversion commit: `35161b20a8d10d9a994d29fc40900a1a86bbff90`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `Grind` | static `Grind` 1554–1685 |
| `CallGrind` | `AActor::CallGrind` 1694–1705 |
| `Grind` action gate | `DEFINE_ACTION_FUNCTION_NATIVE` 1687–1692 |


**Post-review ratio:** 130/151 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjGrindTests.Grind_CorpseDontCrunch_Shrinks`
- `VmPMobjGrindTests.Grind_Nonsolid_ReturnsFalse`
- `VmPMobjGrindTests.CallGrind_UsesVirtualWhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 734 --to 734` (0 hard / 0 soft).

## Seams

`Spawn`, `A_BossDeath`, `S_Sound`, `P_DamageMobj`, and `VMCall` are injected.

## Credit rationale

Faithful crush/giblet pipeline and virtual grind dispatch in one playsim TU band.
