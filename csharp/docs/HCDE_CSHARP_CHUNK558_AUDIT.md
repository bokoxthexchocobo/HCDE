# Chunk 558 — GetBoneEulerAngles, TransformByBone, GetBoneMatrixRaw (6152–6329)

## Scope

GetBoneEulerAngles, TransformByBone, GetBoneMatrixRaw (6152–6329)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneEulerAngles** 6152–6167 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneEulerAngles** 6169–6185 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, TransformByBone** 6189–6231 (43 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, TransformByNamedBone** 6233–6277 (45 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneMatrixRaw** 6285–6305 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneMatrixRaw** 6307–6329 (23 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmActorBoneEulerTransform.cs`
Managed LOC (tally): 153.
Conversion commit: `7f12cf05077f8ed75d4aba41834ae935aec5c6a7`.

## Padding review (2026-10-21)

Whole-function ports only.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `GetBoneEulerAnglesNative` | `GetBoneEulerAngles` 6152–6167 |
| `GetNamedBoneEulerAnglesNative` | `GetNamedBoneEulerAngles` 6169–6185 |
| `TransformByBoneNative` | `TransformByBone` 6189–6231 |
| `TransformByNamedBoneNative` | `TransformByNamedBone` 6233–6277 |
| `GetBoneMatrixRawNative` | `GetBoneMatrixRaw` 6285–6305 |
| `GetNamedBoneMatrixRawNative` | `GetNamedBoneMatrixRaw` 6307–6329 |

**Post-review ratio:** 153/165 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmActorBoneEulerTransformTests.GetBoneEulerAnglesNative_ReturnsEulerWhenModelOk`
- `VmActorBoneEulerTransformTests.GetBoneEulerAnglesNative_DefaultsWhenNoModel`
- `VmActorBoneEulerTransformTests.TransformByBoneNative_ReturnsNumret`
- `VmActorBoneEulerTransformTests.GetBoneMatrixRawNative_RequiresOutMatrix`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 558 --to 558` (0 hard / 0 soft).

## Seams

`GetBoneShared`, `AActor::GetBoneEulerAngles`, `GetBonePosition`, `GetBoneMatrix`, and `TArray<double>` matrix fill are not hosted.

## Credit rationale

One managed method per credited whole DEFINE body.
