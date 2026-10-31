# Chunk 1207 — ZScript scope sides, field access, and class construction checks from scopebarrier.cpp (1–223).

## Scope

ZScript scope sides, field access, and class construction checks from scopebarrier.cpp (1–223).

## Native ranges

- `src/common/scripting/core/scopebarrier.cpp` **FScopeBarrier::AddFlags** 1–223 (223 lines)

**Credited native LOC:** 223 (tally `nativeLoc` 223).

## Managed

`VmScopeBarrier1207Slice.cs`
Managed LOC (tally): 204.
Conversion commit: `59101555244fb314e642a518d7e7aa223785068f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SideFromFlags through StringFromSide (**31–103**) | yes |
| ChangeSideInFlags and ChangeSideInObjectFlags (**106–120**) | yes |
| Constructors (**122–140**) | yes |
| AddFlags (**145–199**) | yes |
| CheckSidesForFunctionPointer (**201–207**) | yes |
| ValidateNew and ValidateCall (**210–223**) | yes |
| Band hull (**1–223**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

UI wins over play when both bits are set. Virtual and clear scope are their own sides, and object flags only distinguish UI, play, and plain data. Changing a side clears the old scope bits, including no-rollback on fields. The first access records the calling side. A plain field inherits the last real side. Only UI may read a UI field. A failed read also bans writes and calls, and later fields in the chain do not replace that error. Different sides cannot be written. A read-only method may still be called across sides when the field stayed readable. Clearscope is plain data for a function pointer, and plain data may point at any side. A class with no VM type cannot be instantiated. Constructing or calling across sides is rejected unless the target is plain data.

| Managed (`VmScopeBarrier1207Slice.cs`) | Native (`scopebarrier.cpp`) |
|----------------------------------------|------------------------------|
| Scope barrier | **1–223** |
| Band hull (exclusive slice) | **1–223** |

### Chunk **1207** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **223** |
| **Net headline** | **+223** vs post-**1206** |


**Post-review ratio:** 204/223 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmScopeBarrier1207SliceTests.Sides_MapFlagsAndBlockUiReads`
- `VmScopeBarrier1207SliceTests.Chain_FreezesAfterTheFirstBanAndConstructionChecksTheClassSide`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1207 --to 1207` (0 hard / 0 soft).

## Seams

Class and function names stand in for PClass and VMFunction. ValidateCall with a missing type is the native null pointer. X_OTHER abort text has no extra sentence before the detail.

## Credit rationale

UNCredited scopebarrier.cpp 1–223 (+223). The file ends at 223. A full tally line scan shows no overlap.
