# Chunk 668 — Whole `FPathTraverse::AddThingIntercepts` (`p_maputl.cpp` 1194–1361).

## Scope

Whole `FPathTraverse::AddThingIntercepts` (`p_maputl.cpp` 1194–1361).

## Native ranges

- `src/playsim/p_maputl.cpp` **FPathTraverse::AddThingIntercepts** 1194–1361 (168 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmPMapUtlAddThingIntercepts.cs`
Managed LOC (tally): 121.
Conversion commit: `401ed9157dc0f8eb5c6367499b3ce83a337eea55`.

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


**Post-review ratio:** 121/168 managed/native ≈ 72% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlAddThingInterceptsTests.AddThingIntercepts_StartInsideBoxPushesZeroFrac`
- `VmPMapUtlAddThingInterceptsTests.AddThingIntercepts_CompatibleDiagonalRecordsFrac`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 668 --to 668` (0 hard / 0 soft).

## Seams

`FBlockThingsIterator`, `P_PointOnDivlineSide`, and `P_InterceptVector` dispatch remain native.

## Credit rationale

Thing intercept collection for path traverse.
