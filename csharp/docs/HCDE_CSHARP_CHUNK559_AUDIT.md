# Chunk 559 — GetObjectToWorldMatrixRaw, SetAnimation*/FrameRate natives and DEFINE wrappers (6331–6349, 6678–7041, AnimLayerToAnimInfo) Retired (2026-10-24): `--band` hull overlaps existing credit (p-action-set-animation-internal-chunk517 src/playsim/p_actionfunctions.cpp 6507-6676).

## Scope

GetObjectToWorldMatrixRaw, SetAnimation*/FrameRate natives and DEFINE wrappers (6331–6349, 6678–7041, AnimLayerToAnimInfo) Retired (2026-10-24): `--band` hull overlaps existing credit (p-action-set-animation-internal-chunk517 src/playsim/p_actionfunctions.cpp 6507-6676).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `fd57a3b30c26288de09638bb6c0a34f657a2ccfb`.

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

### Out-of-body uncredit (559)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 559 --to 559` (0 hard / 0 soft).

## Seams

`SetAnimationInternal`, `I_GetTimeFrac`, `GetObjectToWorldMatrix`, and `AnimInfo` storage are not hosted.

## Credit rationale

Whole-function ports for thin natives and DEFINE ticFrac forwarding; `SetAnimationInternal` credited in chunk 517.
