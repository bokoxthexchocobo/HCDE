# Chunk 1125 — Doom/Strife/Raven automap palettes, AM_ClearColorsets, and AM_initColors through the Strife case from `am_map.cpp` (**601–750**); follows **1124**.

## Scope

Doom/Strife/Raven automap palettes, AM_ClearColorsets, and AM_initColors through the Strife case from `am_map.cpp` (**601–750**); follows **1124**.

## Native ranges

- `src/am_map.cpp` **Automap palettes and initColors** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapPaletteInit1125Slice.cs`
Managed LOC (tally): 186.
Conversion commit: `e41b7b884414f365fe23d8e9a030e829e0d14349`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Doom palette rows after the background triplet (**601–626**) | yes |
| Strife palette (**628–655**) | yes |
| Raven palette bytes (**657–684**) | yes |
| `AM_ClearColorsets` (**693–697**) | yes |
| `AM_initColors` overlay/mod/Doom/Strife (**705–750**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **600** | Doom background triplet already credited by **1124** |
| **686–692**, **699–704** | storage and section comments |
| **752–755** | Raven `initFromColors(..., true)` switch arm |

`GAME_DoomChex` is **17**, `GAME_Strife` is **8**, `GAME_Raven` is **6**. Doom and Strife palettes pass `showlocks` false.

| Managed (`VmAmMapPaletteInit1125Slice.cs`) | Native (`am_map.cpp`) |
|--------------------------------------------|---------------------|
| Palettes, clear, initColors head | **601–750** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **1125** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1124** |


**Post-review ratio:** 186/150 managed/native ≈ 124% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapPaletteInit1125SliceTests.DoomColors_WallIsRedAndSpecialIsUnused`
- `VmAmMapPaletteInit1125SliceTests.StrifeAndRaven_SpotCheckThingAndPortal`
- `VmAmMapPaletteInit1125SliceTests.ClearColorsets_ClearsBothDefinedFlags`
- `VmAmMapPaletteInit1125SliceTests.ChooseInit_PrefersOverlayModThenGamePalette`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1125 --to 1125` (0 hard / 0 soft).

## Seams

Host color CVARs stay injected. The Raven switch arm is **752–755** and is not applied here.

## Credit rationale

UNCredited **`am_map.cpp`** **601–750** (+150), inside the 3594-line file, with no conversion or recognition overlap.
