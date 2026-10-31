# Chunk 1248 — Column-major matrix multiply, 3x3 and 4x4 determinants, and the 4x4 adjoint from matrix.cpp (516-723). inverseMatrix at 724 stays on the earlier matrix chunk.

## Scope

Column-major matrix multiply, 3x3 and 4x4 determinants, and the 4x4 adjoint from matrix.cpp (516-723). inverseMatrix at 724 stays on the earlier matrix chunk.

## Native ranges

- `src/common/utility/matrix.cpp` **VSMatrix::multMatrix** 516–723 (208 lines)

**Credited native LOC:** 208 (tally `nativeLoc` 208).

## Managed

`VmMatrix1248Slice.cs`
Managed LOC (tally): 140.
Conversion commit: `4522342a83d8c69ebbdc81c266d8c6b938ef774e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| multMatrix (**520–537**) | yes |
| mat3Determinant (**539–544**) | yes |
| mat4Determinant (**546–590**) | yes |
| mat4Adjoint (**592–722**) | yes |
| Band hull (**516–723**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Earlier transforms | matrix chunk 96 |
| Inverse divide | later in this file |

Multiply replaces the left matrix with left times right. The product is accumulated into a temporary so the left matrix can also be the right matrix. The 4×4 determinant expands four 3×3 minors of column 0. The adjoint stores each cofactor in column-major order. A 3×3 block is row-major inside that expansion.

| Managed (`VmMatrix1248Slice.cs`) | Native (`matrix.cpp`) |
|-----------------------------------|----------------------|
| Multiply, determinants, adjoint | **516–723** |
| Band hull (exclusive slice) | **516–723** |

### Chunk **1248** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **208** |
| **Net headline** | **+208** vs post-**1247** |


**Post-review ratio:** 140/208 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmMatrix1248SliceTests.Multiply_AppendsTheRightMatrixInColumnMajorOrder`
- `VmMatrix1248SliceTests.Determinant_ExpandsMinorsAndRejectsAShortMatrix`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1248 --to 1248` (0 hard / 0 soft).

## Seams

A null or short matrix returns false or a zero determinant. FLOATTYPE is float because USE_DOUBLE is off. The determinant is computed in float and returned as double, matching the native return.

## Credit rationale

UNCredited matrix.cpp 516-723 (+208). loadIdentity through computeNormalMatrix and inverseMatrix are already credited. A full tally line scan shows no overlap with 516-723.
