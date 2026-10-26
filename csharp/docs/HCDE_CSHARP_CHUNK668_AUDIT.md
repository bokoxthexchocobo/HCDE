# Chunk 668 — Whole `FPathTraverse::AddThingIntercepts` (`p_maputl.cpp` 1194–1361).

## Scope

Whole `FPathTraverse::AddThingIntercepts` (`p_maputl.cpp` 1194–1361).

## Native ranges

- `src/playsim/p_maputl.cpp` **FPathTraverse::AddThingIntercepts** 1194–1361 (168 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmPMapUtlAddThingIntercepts.cs`
Managed LOC (tally): 145.
Conversion commit: `a9140a580ecc36950f3267ae845699680146959e`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `AddThingIntercepts` | `FPathTraverse::AddThingIntercepts` 1194–1361 |

### Out-of-body uncredit (668)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **168** |
| **Out-of-body removed** | **0** |
| **Honest** | **168** |


**Post-review ratio:** 145/168 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlAddThingInterceptsTests.AddThingIntercepts_StartInsideBoxPushesZeroFrac`
- `VmPMapUtlAddThingInterceptsTests.AddThingIntercepts_CompatibleDiagonalRecordsFrac`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 668 --to 668` (0 hard / 0 soft).

## Seams

`FBlockThingsIterator`, `P_PointOnDivlineSide`, and `P_InterceptVector` dispatch remain native.

## Credit rationale

Thing intercept collection for path traverse.
