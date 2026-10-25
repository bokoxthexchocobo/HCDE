# Chunk 533 — FxAddSub::Resolve, FxMulDiv::Resolve, FxPow::Resolve Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-constant-fold-chunk46 src/common/scripting/backend/codegen.cpp 3361-3388).

## Scope

FxAddSub::Resolve, FxMulDiv::Resolve, FxPow::Resolve Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-constant-fold-chunk46 src/common/scripting/backend/codegen.cpp 3361-3388).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `194b7a4c465b54da4a55453a92104b3ec87eac98`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `FxAddSub::Resolve` **3309–3396**, `FxMulDiv::Resolve` **3533–3614**, `FxPow::Resolve` **3801–3835** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `CustomAdditionHook` | **3321–3328** |
| `TextureIdPlusInteger` | **3331–3333** |
| `QuaternionBinary` / `VectorBinaryCompatible` / `ClassifyAddSub` | **3335–3358** |
| `FoldFloatBinary` / `FoldIntBinary` / `TryFoldBinaryConstants` | **3361–3387** |
| `VectorModulusForbidden` / `VectorDivisionRequiresVectorLeft` / `QuatTimesVecOrQuat` / `VectorScaleByNumeric` / `NumericScaleVector` / `ClassifyMulDivVector` | **3551–3603** |
| `PowRequiresNumericOperands` | **3812–3816** |
| `PowFoldConstants` (via tests) | **3829–3833** |

### Out-of-body uncredit (533)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 533 --to 533` (0 hard / 0 soft).

## Seams

`Promote`, `FxFloatCast`, `FxConstant`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript add/sub and mul/div vector resolve typing and FxPow numeric resolve from `codegen.cpp` (3309–3360, 3533–3605, 3801–3836); mul/div constant fold remains chunk 51.
