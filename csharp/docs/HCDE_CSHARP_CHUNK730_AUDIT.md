# Chunk 730 — Retired prior `p-acs-clientside-script-chunk730` (669–944 was not one function). Whole `FBehavior::Init` (`p_acs.cpp` 2281–2792).

## Scope

Retired prior `p-acs-clientside-script-chunk730` (669–944 was not one function). Whole `FBehavior::Init` (`p_acs.cpp` 2281–2792).

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::Init** 2281–2750 (470 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2752–2792 (41 lines)

**Credited native LOC:** 511 (tally `nativeLoc` 511).

## Managed

`VmPAcsBehaviorInit.cs`
Managed LOC (tally): 525.
Conversion commit: `3a28e6289592348402ba44a7b4db5da48d627554`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `Init` | `FBehavior::Init` 2281–2749 |
| `RejectNullLevel` / `PickLumpLength` | null/length guards 2281–2310 |
| `ApplyOldFormatHeader` | old ACSE upgrade 2358–2373 |
| `LoadOldStringTable` / `LoadEnhancedStringTable` | string table 2382–2413 |
| `LoadEnhancedChunks` | enhanced map vars/functions 2416–2745 |
| `LoadLibraries` / `ResolveImports` | LOAD/import resolution 2627–2745 |
| `FinishInit` | epilogue 2748–2749 |
| `ReleaseBehavior` | `~FBehavior` 2752–2792 |

### Out-of-body uncredit (730)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **512** |
| **Out-of-body removed** | **1** |
| **Honest** | **511** |


**Post-review ratio:** 525/511 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsBehaviorInitTests.Init_NullLevel_ReturnsFalse`
- `VmPAcsBehaviorInitTests.Init_ValidHeader_ReturnsTrue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 730 --to 730` (0 hard / 0 soft).

## Seams

`FileReader`, lump enumeration, `FBehavior` module tables, and `VM` script registration are injected.

## Credit rationale

Single-function oversize exception (~512 native LOC): ACS behavior module load and script index setup.
