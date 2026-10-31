# Chunk 1123 — Overlay color CVARs, AMColor packing, color-set names, and initFromCVars almost-background from `am_map.cpp` (**301–450**); follows **1122**.

## Scope

Overlay color CVARs, AMColor packing, color-set names, and initFromCVars almost-background from `am_map.cpp` (**301–450**); follows **1122**.

## Native ranges

- `src/am_map.cpp` **Automap overlay colors and initFromCVars** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapOverlayColor1123Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `b4e392a8878ffbe17da5a0adca1febd4d8603fd1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Overlay color CVARs and fill alpha (**301–318**) | yes |
| `AMColor` opaque pack / invalid RGB (**326–349**) | yes |
| `ColorNames` through almost-background (**357–385**) | yes |
| `AMColorset` indexes (**387–418**) | yes |
| `initFromCVars` almost-background and lock flags (**426–450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **320–325**, **351–356**, **419–425** | section comments |
| **453+** | `initFromColors` and later tables outside hull |

| Managed (`VmAmMapOverlayColor1123Slice.cs`) | Native (`am_map.cpp`) |
|---------------------------------------------|---------------------|
| Overlay defaults, pack, names, initFromCVars | **301–450** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1123** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1122** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapOverlayColor1123SliceTests.DefaultOverlayColors_MatchLockedThingAndPortal`
- `VmAmMapOverlayColor1123SliceTests.ForceOpaque_SetsAlphaAndInvalidRgbStaysRejected`
- `VmAmMapOverlayColor1123SliceTests.DeriveAlmostBackground_WrapsChannelsBelowZero`
- `VmAmMapOverlayColor1123SliceTests.InitFromCvars_PaintsAlmostBackgroundAndEnablesLocks`
- `VmAmMapOverlayColor1123SliceTests.ColorNames_EndAtAlmostBackground`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1123 --to 1123` (0 hard / 0 soft).

## Seams

Host `FColorCVar` storage stays outside this slice; `initFromColors` begins at **453**.

## Credit rationale

UNCredited **`am_map.cpp`** **301–450** (+150), inside the 3594-line file, with no conversion or recognition overlap.
