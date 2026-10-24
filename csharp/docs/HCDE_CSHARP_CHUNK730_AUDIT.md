# Chunk 730 — ACS clientside script flag, stack head, fixed conversions, and iterator policy (`p_acs.cpp` 669–944). Retired: replaced by p-acs-behavior-init-chunk730.

## Scope

ACS clientside script flag, stack head, fixed conversions, and iterator policy (`p_acs.cpp` 669–944). Retired: replaced by p-acs-behavior-init-chunk730.

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::Init** 2281–2792 (512 lines)

**Credited native LOC:** 512 (tally `nativeLoc` 0).

## Managed

`VmPAcsBehaviorInit.cs`
Managed LOC (tally): 0.
Conversion commit: `14c0c931b67991c0add872f4a0008d0495fe80e5`.

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


## Tests

- `VmPAcsBehaviorInitTests.Init_NullLevel_ReturnsFalse`
- `VmPAcsBehaviorInitTests.Init_ValidHeader_ReturnsTrue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 730 --to 730` (0 hard / 0 soft).

## Seams

`FileReader`, lump enumeration, `FBehavior` module tables, and `VM` script registration are injected.

## Credit rationale

Single-function oversize exception (~512 native LOC): ACS behavior module load and script index setup.
