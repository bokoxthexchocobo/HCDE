# Chunk 734 — Whole static `Grind`, `DEFINE_ACTION_FUNCTION_NATIVE(AActor, Grind)`, and `AActor::CallGrind` (`p_mobj.cpp` 1554–1705).

## Scope

Whole static `Grind`, `DEFINE_ACTION_FUNCTION_NATIVE(AActor, Grind)`, and `AActor::CallGrind` (`p_mobj.cpp` 1554–1705).

## Native ranges

- `src/playsim/p_mobj.cpp` **Grind** 1554–1685 (132 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, Grind, Grind)** 1687–1692 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallGrind** 1694–1705 (12 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjGrind.cs`
Managed LOC (tally): 130.
Conversion commit: `5ba64b8d5b44a1bdb6a28f6696e51905aedaa7f0`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `Grind` | static `Grind` 1554–1685 |
| (native macro only) | `DEFINE_ACTION_FUNCTION_NATIVE` 1687–1692 |
| `CallGrind` | `AActor::CallGrind` 1694–1705 |

### Out-of-body uncredit (734)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


**Post-review ratio:** 130/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjGrindTests.Grind_CorpseDontCrunch_Shrinks`
- `VmPMobjGrindTests.Grind_Nonsolid_ReturnsFalse`
- `VmPMobjGrindTests.Grind_ItemsFlag_PassedThroughContext`
- `VmPMobjGrindTests.CallGrind_UsesVirtualWhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 734 --to 734` (0 hard / 0 soft).

## Seams

`Spawn`, `A_BossDeath`, `S_Sound`, `P_DamageMobj`, and `VMCall` are injected.

## Credit rationale

Faithful crush/giblet pipeline and virtual grind dispatch; DEFINE macro is native-only (calls static `Grind`).
