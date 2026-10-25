# Chunk 564 — FxCompareRel string emit, emit wrappers, FxBitOp/Shift/Concat emit (4055–4156, 4546–4926) Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-compare-rel-chunk55 src/common/scripting/backend/codegen.cpp 4096-4143).

## Scope

FxCompareRel string emit, emit wrappers, FxBitOp/Shift/Concat emit (4055–4156, 4546–4926) Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-compare-rel-chunk55 src/common/scripting/backend/codegen.cpp 4096-4143).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5ce0c2976ee739ada1f9f0eb92231b6494e161e2`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `ZccCompareRelStringEmit.StringCompareOpcode` / `ApplyKonstFlags` | `FxCompareRel::EmitCommon` string branch 4055–4095 |
| `ZccCompareRelStringEmit.ShouldMaterializeBoolResult` | `Emit` / `EmitCompare` 4146–4156 |
| `ZccBitOpEmit` | `FxBitOp::Emit` 4546–4572 (swap konst left, `instr + op2.Konst`) |
| `ZccShiftEmit` | `FxShift::Emit` 4637–4684 (`InstrMap`, immediate column) |
| `ZccConcatEmit.UseStringKonstReload` / `ReuseStringRegister` | `FxConcat::Emit` string/konst branches 4853–4861, 4887–4895 |
| `ZccConcatEmit.SelectCastOpcode` | `FxConcat::Emit` `OP_CAST` cast selection 4872–4881, 4906–4915 |

### Out-of-body uncredit (564)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **164** |
| **Out-of-body removed** | **0** |
| **Honest** | **164** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 564 --to 564` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, `FxConstant`, and per-player command skip loop are not hosted.

## Credit rationale

Whole-function emit ports for previously uncredited codegen slices; numeric compare-rel emit remains chunk 55.
