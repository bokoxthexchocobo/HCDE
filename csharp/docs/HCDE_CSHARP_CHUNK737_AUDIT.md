# Chunk 737 — Whole `AActor::SetState`, static `DestroyAllInventory`, and `UseInventory` (`p_mobj.cpp` 856–1035).

## Scope

Whole `AActor::SetState`, static `DestroyAllInventory`, and `UseInventory` (`p_mobj.cpp` 856–1035).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetState** 856–969 (114 lines)
- `src/playsim/p_mobj.cpp` **DestroyAllInventory** 978–1005 (28 lines)
- `src/playsim/p_mobj.cpp` **AActor::UseInventory** 1024–1035 (12 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmPMobjSetStateUse.cs`
Managed LOC (tally): 127.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SetState` | `AActor::SetState` 856–961 |
| `DestroyAllInventory` | static `DestroyAllInventory` 978–1005 |
| `UseInventory` | `AActor::UseInventory` 1024–1035 |


**Post-review ratio:** 127/154 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSetStateUseTests.SetState_NullState_Destroys`
- `VmPMobjSetStateUseTests.DestroyAllInventory_EmptiesList`
- `VmPMobjSetStateUseTests.UseInventory_VirtualPath_Used`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 737 --to 737` (0 hard / 0 soft).

## Seams

`CallAction`, skin tables, `VMCall`, and inventory `Destroy` are injected.

## Credit rationale

State transition loop plus inventory teardown/use virtual dispatch in one mobj TU band.
