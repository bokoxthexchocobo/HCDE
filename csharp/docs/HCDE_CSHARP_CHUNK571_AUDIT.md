# Chunk 571 — MBF monster/weapon emitter factories 872–1044 (skip 936–941 FindTracer — chunk 339) plus SetDehParams prologue

## Scope

MBF monster/weapon emitter factories 872–1044 (skip 936–941 FindTracer — chunk 339) plus SetDehParams prologue

## Native ranges

- `src/gamedata/d_dehacked.cpp` **MBF emitter factories (monster)** 872–935 (64 lines)
- `src/gamedata/d_dehacked.cpp` **MBF emitter factories (jump/weapon)** 961–1044 (84 lines)
- `src/gamedata/d_dehacked.cpp` **SetDehParams** 1091–1098 (8 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmDehackedMbfWeaponPointerEmitters.cs`
Managed LOC (tally): 79.
Conversion commit: `6530fd1795cd2d203e04f3387c4a6ecbafa4e3d8`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `VmDehackedMbfWeaponPointerEmitters` | Monster/weapon `Create*Func` thunks 872–935 and 961–1044; `SetDehParamsNative` ↔ prologue 1091–1098 |

**Post-review ratio:** 79/156 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedMbfWeaponPointerEmittersTests.WeaponMeleeAttackArgs_AppliesNativeDefaults`
- `VmDehackedMbfWeaponPointerEmittersTests.WeaponBulletAttackArgs_AppliesNativeDefaults`
- `VmDehackedMbfWeaponPointerEmittersTests.SetDehParams_RejectsInvalidPointer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 571 --to 571` (0 hard / 0 soft).

## Seams

`FunctionCallEmitter`, `MBFParamState`, and `soundEngine` mutation are not hosted.

## Credit rationale

Ports MBF21 emitter argument shaping; `CreateFindTracerFunc` remains credited on chunk 339.
