# Chunk 737 — Whole `AActor::SetState`, action/native inventory helpers (`p_mobj.cpp` 856–969, 978–1012, 1024–1035).

## Scope

Whole `AActor::SetState`, action/native inventory helpers (`p_mobj.cpp` 856–969, 978–1012, 1024–1035).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetState** 856–961 (106 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetState)** 963–969 (7 lines)
- `src/playsim/p_mobj.cpp` **DestroyAllInventory** 978–1005 (28 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, DestroyAllInventory, DestroyAllInventory)** 1007–1012 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::UseInventory** 1024–1035 (12 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPMobjSetStateUse.cs`
Managed LOC (tally): 133.
Conversion commit: `9e5038a2ea62ad67ae290494cf3d3851453251e5`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SetState` | `AActor::SetState` 856–961 |
| `SetStateAction` | `DEFINE_ACTION_FUNCTION(AActor, SetState)` 963–969 |
| `DestroyAllInventory` | static `DestroyAllInventory` 978–1005 |
| `DestroyAllInventoryAction` | `DEFINE_ACTION_FUNCTION_NATIVE` 1007–1012 |
| `UseInventory` | `AActor::UseInventory` 1024–1035 |


**Post-review ratio:** 133/159 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSetStateUseTests.SetState_NullState_Destroys`
- `VmPMobjSetStateUseTests.SetState_BadUseFlags_Destroys`
- `VmPMobjSetStateUseTests.SetStateAction_DelegatesToMethod`
- `VmPMobjSetStateUseTests.DestroyAllInventory_EmptiesList`
- `VmPMobjSetStateUseTests.DestroyAllInventoryAction_CallsStatic`
- `VmPMobjSetStateUseTests.UseInventory_VirtualPath_Used`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 737 --to 737` (0 hard / 0 soft).

## Seams

`CallAction`, skin tables, `VMCall`, and inventory `Destroy` are injected.

## Credit rationale

SetState method ends at 961; DEFINE/native inventory wrappers credited as separate whole symbols.
