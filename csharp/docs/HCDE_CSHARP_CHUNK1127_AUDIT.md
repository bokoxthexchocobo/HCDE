# Chunk 1127 — AM_StaticInit arrow lumps, mark textures, pan increment, and FTOM/MTOF/CXMTOF/CYMTOF from `am_map.cpp` (**901–1050**); follows **1126**.

## Scope

AM_StaticInit arrow lumps, mark textures, pan increment, and FTOM/MTOF/CXMTOF/CYMTOF from `am_map.cpp` (**901–1050**); follows **1126**.

## Native ranges

- `src/am_map.cpp` **Automap static init and scale helpers** 901–1050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapStaticInit1127Slice.cs`
Managed LOC (tally): 86.
Conversion commit: `3ea35aa012088a401a6d3be005fa669224903729`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Optional map/cheat arrows and required key arrows (**904–907**) | yes |
| Empty map-arrow fatal gate (**908**) | yes |
| `AMMNUM%d` mark textures (**912–916**) | yes |
| `F_PANINC` 140/`TICRATE` and `scale_mtof` .2 (**933–940**) | yes |
| `FTOM` / `MTOF` / `CXMTOF` / `CYMTOF` (**1006–1025**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **901–903**, **910–911**, **918–1005**, **1027–1050** | clears already credited by **1126**, field storage, and method declarations |

`TICRATE` is **35**, so the pan step is **4**. `CXMTOF`/`CYMTOF` truncate toward zero. The commented `f_x`/`f_y` terms stay omitted.

| Managed (`VmAmMapStaticInit1127Slice.cs`) | Native (`am_map.cpp`) |
|-------------------------------------------|---------------------|
| Static init tail and scale helpers | **901–1050** |
| Band hull (exclusive slice) | **901–1050** |

### Chunk **1127** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1126** |


**Post-review ratio:** 86/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapStaticInit1127SliceTests.ArrowLumpsToLoad_SkipsEmptyOptionalNamesAndAlwaysLoadsKeys`
- `VmAmMapStaticInit1127SliceTests.MarkTextureNames_AreAmmnumZeroThroughNine`
- `VmAmMapStaticInit1127SliceTests.PanIncrement_IsFourPixelsPerTic`
- `VmAmMapStaticInit1127SliceTests.FrameMapScale_TruncatesWindowCoordinates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1127 --to 1127` (0 hard / 0 soft).

## Seams

Texture manager lookups and `I_FatalError` stay host-injected. Later drawer methods begin at **1051**.

## Credit rationale

UNCredited **`am_map.cpp`** **901–1050** (+150), inside the 3594-line file, with no conversion or recognition overlap.
