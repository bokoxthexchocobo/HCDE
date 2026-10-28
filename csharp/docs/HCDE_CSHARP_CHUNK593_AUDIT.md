# Chunk 593 — FBehavior::Init ARRAY/AINI/AIMP/MSTR/ASTR/ATAG/LOAD chunks (2499–2651)

## Scope

FBehavior::Init ARRAY/AINI/AIMP/MSTR/ASTR/ATAG/LOAD chunks (2499–2651)

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::Init** 2499–2513 (15 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2515–2533 (19 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2535–2549 (15 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2551–2587 (37 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2589–2625 (37 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2627–2651 (25 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2746–2750 (5 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmAcsBehaviorInitArrays.cs`
Managed LOC (tally): 77.
Conversion commit: `e659c66558a9b66102aa4591880d698f7f074614`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `LoadArrayChunkNative` | ARRAY chunk setup 2499–2513 |
| `InitArrayFromChunkNative` | AINI initialization loop 2515–2533 |
| `TotalArrayCountNative` | AIMP array pointer setup 2535–2549 |
| `TagStringMapVarNative` / `ShouldTagLibraryStrings` | MSTR/ASTR tagging 2551–2587 |
| `ShouldApplyFunctionPtrTag` / `ShouldResolveStringTag` | ATAG loop 2589–2625 |
| `LoadLibraryTokenNative` | LOAD chunk parse 2627–2651 |

**Post-review ratio:** 77/153 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsBehaviorInitArraysTests.TotalArrayCountNative_AddsImportTotal`
- `VmAcsBehaviorInitArraysTests.InitArrayFromChunkNative_CapsByArraySize`
- `VmAcsBehaviorInitArraysTests.LoadLibraryTokenNative_ParsesNullTerminatedToken`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 593 --to 593` (0 hard / 0 soft).

## Seams

`FindChunk`, `LookupString`, `GlobalACSStrings`, and `LoadModule` are not hosted.

## Credit rationale

Whole-function slices of enhanced-format `FBehavior::Init` array and library bootstrap.
