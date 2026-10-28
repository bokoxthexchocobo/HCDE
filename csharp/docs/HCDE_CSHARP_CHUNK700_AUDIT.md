# Chunk 700 — Whole `DPSprite::SetState` (`p_pspr.cpp` 485–639).

## Scope

Whole `DPSprite::SetState` (`p_pspr.cpp` 485–639).

## Native ranges

- `src/playsim/p_pspr.cpp` **DPSprite::SetState** 485–639 (155 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPPSpriteSetState.cs`
Managed LOC (tally): 189.
Conversion commit: `2b2df607074a1aab9e52b90fee914de6a238e3bb`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `VmPPSpriteSetState` | `DPSprite::SetState` 485–639 |

### Out-of-body uncredit (700)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **0** |
| **Honest** | **155** |


**Post-review ratio:** 189/155 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSpriteSetStateTests.SetState_ClearsWeaponReadyOnWeaponLayer`
- `VmPPSpriteSetStateTests.SetState_FastWeaponsCvarForcesOneTic`
- `VmPPSpriteSetStateTests.SetState_ActionJumpContinuesWithZeroTics`
- `VmPPSpriteSetStateTests.SetState_RejectsOverlayOnlyStateOnWeaponCaller`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 700 --to 700` (0 hard / 0 soft).

## Seams

`Printf`, live `FState::CallAction`, and `Destroy` teardown are injected; DEFINE wrapper at 641–648 is excluded.

## Credit rationale

Weapon psprite state machine after combat trace chunks; documents HCDE local-echo split in native comment without suppressing SetState.
