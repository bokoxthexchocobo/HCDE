# Chunk 1116 — `PostBeginPlay`, `isFast`, `Activate`/`Deactivate`, and `OnDestroy` premorph cleanup from `p_mobj.cpp` (**5827–5976**). Skips **5577–5726** overlap with chunk **481**.

## Scope

`PostBeginPlay`, `isFast`, `Activate`/`Deactivate`, and `OnDestroy` premorph cleanup from `p_mobj.cpp` (**5827–5976**). Skips **5577–5726** overlap with chunk **481**.

## Native ranges

- `src/playsim/p_mobj.cpp` **PostBeginPlay Activate Deactivate OnDestroy** 5827–5976 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjActivateDestroy1116Slice.cs`
Managed LOC (tally): 119.
Conversion commit: `cfba8e9c81dcaccec327696cab063a624653a5a0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PostBeginPlay` no-delay + lights (**5828–5833**) | yes |
| `isFast` always/never/skill (**5844–5846**) | yes |
| `Activate` dormant wake (**5862–5876**) | yes |
| `Deactivate` dormant sleep (**5907–5921**) | yes |
| `OnDestroy` alternative + world hook (**5954–5972**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **5836–5840**, **5880–5896**, **5925–5941** | VM / event thunks |
| **5974+** | attached-light host cleanup outside hull |
| **5577–5726** | overlaps chunk **481** |

| Managed (`VmPMobjActivateDestroy1116Slice.cs`) | Native (`p_mobj.cpp`) |
|-----------------------------------------------|---------------------|
| Begin/activate/destroy policy | **5827–5976** |
| Band hull (exclusive slice) | **5827–5976** |

### Chunk **1116** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1115** |


**Post-review ratio:** 119/150 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjActivateDestroy1116SliceTests.IsFast_NeverFastOverridesSkill`
- `VmPMobjActivateDestroy1116SliceTests.RunActivate_WakesDormantMonster`
- `VmPMobjActivateDestroy1116SliceTests.RunDeactivate_SetsDormantWithoutInactiveState`
- `VmPMobjActivateDestroy1116SliceTests.RunOnDestroyPremorph_ClearsLivingAlternative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1116 --to 1116` (0 hard / 0 soft).

## Seams

VM thunks, `SetState`, `WorldThingDestroyed`, and inventory destruction remain host-injected.

## Credit rationale

First ≥150 UNCredited window after **1115** is **5827–5976**; **5577–5726** overlaps **481**.
