# Chunk 536 — FxDotCross, FxTypeCheck, FxDynamicCast, FxConditional::Resolve; DPlaneWatcher Construct/Tick Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Scope

FxDotCross, FxTypeCheck, FxDynamicCast, FxConditional::Resolve; DPlaneWatcher Construct/Tick Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f34d14b5c1e9a642911c54091f86276ea00f1641`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `FxDotCross`/`FxTypeCheck`/`FxDynamicCast`/`FxConditional::Resolve` **5142–5442** and `DPlaneWatcher` **1899–1971** (`codegen.cpp`, `p_acs.cpp`).

| Managed | Native |
|---------|--------|
| `DotCrossOperandsCompatible` / `DotCrossResultType` | `FxDotCross::Resolve` **5142–5157** (`codegen.cpp`) |
| `DynamicCastNeedsObjectPointer` | `FxDynamicCast::Resolve` **5301–5314** |
| `NeedsConditionBoolCast` / `ShouldFloatCastBranch` / `TranslationIdBranchValueType` / `ConditionalIncompatible` | `FxConditional::Resolve` **5365–5442** |
| `Construct` | `DPlaneWatcher::Construct` **1899–1928** (`p_acs.cpp`) |
| `PlaneCrossedWatch` | `DPlaneWatcher::Tick` **1945–1971** |

### Out-of-body uncredit (536)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **191** |
| **Out-of-body removed** | **30** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 536 --to 536` (0 hard / 0 soft).

## Seams

`Promote`, `FxBoolCast`, `FxFloatCast`, `FxTranslationCast`, `P_ExecuteSpecial`, and `Emit` are not hosted.

## Credit rationale

Credits vector dot/cross typing, `is` type-check casts, dynamic cast pointer rewrite, ternary resolve, and ACS plane watcher setup/crossing from `codegen.cpp` (5142–5442) and `p_acs.cpp` (1899–1971).
