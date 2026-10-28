# Chunk 595 — MBF spawn/face/scratch/play-sound emitters and SetDehParams body (798–1188; skip 817 and 831–832 — chunks 292–293) Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Scope

MBF spawn/face/scratch/play-sound emitters and SetDehParams body (798–1188; skip 817 and 831–832 — chunks 292–293) Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4eb2d9c6e33f0bab2529dc8d782f9e2b0eeedbe3`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target). Legacy chunks 292–293 retain 791–795 and 817, 831–832.

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `SpawnItemArgs` | `CreateSpawnFunc` 798–812 |
| `TurnAngleDegrees` | `CreateTurnFunc` 815–816, 818 |
| `FaceAngleArgs` | `CreateFaceFunc` 819–826 |
| `ScratchMeleeArgs` | `CreateScratchFunc` 829–830, 833–836 |
| `PlaySoundArgs` | `CreatePlaySoundFunc` 839–848 |
| `MbfFactoryTableEntryCountNative` | `MBFCodePointerFactories` 1046–1087 |
| `EmitStandardActionParametersNative` | `SetDehParams` parameter loop 1136–1139 |
| `SetDehParamsValidatePointerNative` / `TryValidateSetDehParamsNative` | `SetDehParams` 1110–1161, 1173–1188 (skip 1091–1109, 1162–1172 — chunks 263/571) |

### Out-of-body uncredit (595)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **152** |
| **Out-of-body removed** | **44** |
| **Honest** | **108** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 595 --to 595` (0 hard / 0 soft).

## Seams

`FunctionCallEmitter`, `VMFunctionBuilder`, and `PClass::FindActor` are not hosted.

## Credit rationale

Whole-function ports for emitter argument shaping and SetDehParams validation/sanitization.
