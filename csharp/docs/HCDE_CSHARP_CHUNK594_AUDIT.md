# Chunk 594 — FBehavior::Init import resolution and destructor (2653–2790)

## Scope

FBehavior::Init import resolution and destructor (2653–2790)

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::Init** 2653–2745 (93 lines)
- `src/playsim/p_acs.cpp` **FBehavior::~FBehavior** 2752–2809 (58 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmAcsBehaviorInitImports.cs`
Managed LOC (tally): 77.
Conversion commit: `1af26d7e9fcbea8b266ce36389cee1cf2bbb6f7f`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `ShouldResolveImportFunction` / `ImportFunctionMatches` | FNAM import resolution 2663–2695 |
| `ResolveImportMapVarNative` | MIMP map var resolution 2697–2714 |
| `ShouldSkipNullImport` | import loop guard 2655–2661 |
| `ReleaseArrayStoreNative` | `~FBehavior` array teardown 2752–2790 |

**Post-review ratio:** 77/151 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsBehaviorInitImportsTests.ShouldResolveImportFunction_WhenAddressAndImportZero`
- `VmAcsBehaviorInitImportsTests.ResolveImportMapVarNative_OrLibraryIdWhenUnset`
- `VmAcsBehaviorInitImportsTests.ImportFunctionMatches_WhenLibDefinesBody`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 594 --to 594` (0 hard / 0 soft).

## Seams

`FindChunk`, `Printf`, and `delete[]` are not hosted.

## Credit rationale

Whole-function ports for import resolution and `~FBehavior` cleanup.
