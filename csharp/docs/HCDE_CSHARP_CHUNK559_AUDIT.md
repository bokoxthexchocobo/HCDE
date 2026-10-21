# Chunk 559 — GetObjectToWorldMatrixRaw, SetAnimation*/FrameRate natives and DEFINE wrappers (6331–6349, 6678–7041, AnimLayerToAnimInfo)

## Scope

GetObjectToWorldMatrixRaw, SetAnimation*/FrameRate natives and DEFINE wrappers (6331–6349, 6678–7041, AnimLayerToAnimInfo)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetObjectToWorldMatrixRaw** 6331–6349 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **SetAnimationNative** 6678–6681 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetAnimationUINative** 6683–6686 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetAnimationFrameRateInternal** 6688–6728 (41 lines)
- `src/playsim/p_actionfunctions.cpp` **SetAnimationFrameRateNative** 6730–6733 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetAnimationFrameRateUINative** 6735–6738 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetAnimation** 6991–7005 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetAnimationUI** 7007–7021 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetAnimationFrameRate** 7023–7031 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetAnimationFrameRateUI** 7033–7041 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimLayerToAnimInfo** 7043–7078 (36 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmActionSetAnimationNatives.cs`
Managed LOC (tally): 161.
Conversion commit: `8c5a16b807e3ed2d5012504025fa657c88954017`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `GetObjectToWorldMatrixRawNative` | `GetObjectToWorldMatrixRaw` 6331–6349 |
| `SetAnimationNative` | `SetAnimationNative` 6678–6681; DEFINE 6991–7005 (`ActionWrapperTicFrac`) |
| `SetAnimationUiNative` | `SetAnimationUINative` 6683–6686; DEFINE 7007–7021 |
| `SetAnimationFrameRateInternalNative` | `SetAnimationFrameRateInternal` 6688–6728 |
| `SetAnimationFrameRateNative` | `SetAnimationFrameRateNative` 6730–6733; DEFINE 7023–7031 |
| `SetAnimationFrameRateUiNative` | `SetAnimationFrameRateUINative` 6735–6738; DEFINE 7033–7041 |
| `AnimLayerToAnimInfoNative` | `AnimLayerToAnimInfo` 7043–7078 |

**Post-review ratio:** 161/160 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetAnimationNativesTests.GetObjectToWorldMatrixRawNative_FillsWhenOk`
- `VmActionSetAnimationNativesTests.SetAnimationNative_UsesTicFracOne`
- `VmActionSetAnimationNativesTests.SetAnimationFrameRateInternalNative_RejectsNegativeRate`
- `VmActionSetAnimationNativesTests.AnimLayerToAnimInfoNative_NoneWhenNoLayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 559 --to 559` (0 hard / 0 soft).

## Seams

`SetAnimationInternal`, `I_GetTimeFrac`, `GetObjectToWorldMatrix`, and `AnimInfo` storage are not hosted.

## Credit rationale

Whole-function ports for thin natives and DEFINE ticFrac forwarding; `SetAnimationInternal` credited in chunk 517.
