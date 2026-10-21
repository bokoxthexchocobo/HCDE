# Chunk 595 — MBF turn/face/scratch/play-sound emitters and SetDehParams body (815–1188)

## Scope

MBF turn/face/scratch/play-sound emitters and SetDehParams body (815–1188)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **CreateSpawnItemFunc** 798–811 (14 lines)
- `src/gamedata/d_dehacked.cpp` **CreateTurnFunc** 815–818 (4 lines)
- `src/gamedata/d_dehacked.cpp` **CreateFaceFunc** 821–826 (6 lines)
- `src/gamedata/d_dehacked.cpp` **CreateScratchFunc** 829–836 (8 lines)
- `src/gamedata/d_dehacked.cpp` **CreatePlaySoundFunc** 839–848 (10 lines)
- `src/gamedata/d_dehacked.cpp` **CreateRandomJumpFunc** 851–858 (8 lines)
- `src/gamedata/d_dehacked.cpp` **CreateLineEffectFunc** 861–870 (10 lines)
- `src/gamedata/d_dehacked.cpp` **SetDehParams** 1099–1188 (90 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDehackedSetDehParamsBody.cs`
Managed LOC (tally): 81.
Conversion commit: `0629f15f366845bf6e078959dd04e2353e79214e`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `TurnAngleDegrees` | `CreateTurnFunc` 815–818 |
| `FaceAngleArgs` | `CreateFaceFunc` 821–826 |
| `ScratchMeleeArgs` | `CreateScratchFunc` 829–836 |
| `PlaySoundArgs` | `CreatePlaySoundFunc` 839–848 |
| `SetDehParamsValidatePointerNative` / `SanitizePatchNameNative` | `SetDehParams` 1099–1172 |

**Post-review ratio:** 81/150 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedSetDehParamsBodyTests.SpawnItemArgs_DefaultsAmmoAndTranslation`
- `VmDehackedSetDehParamsBodyTests.ScratchMeleeArgs_MatchesNativeDefaults`
- `VmDehackedSetDehParamsBodyTests.RandomJumpArgs_MatchesNative`
- `VmDehackedSetDehParamsBodyTests.LineEffectArgs_PassThrough`
- `VmDehackedSetDehParamsBodyTests.SetDehParamsValidatePointerNative_RejectsNegative`
- `VmDehackedSetDehParamsBodyTests.SanitizePatchNameNative_ReplacesInvalidChars`
- `VmDehackedSetDehParamsBodyTests.BuildPrintableDehackedNameNative_FormatsPath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 595 --to 595` (0 hard / 0 soft).

## Seams

`FunctionCallEmitter`, `VMFunctionBuilder`, and `PClass::FindActor` are not hosted.

## Credit rationale

Whole-function ports for emitter argument shaping and SetDehParams validation/sanitization.
