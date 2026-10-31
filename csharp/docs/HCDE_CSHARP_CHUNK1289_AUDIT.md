# Chunk 1289 — Flag stripes, the mix, and the banner frame from launcherbanner.cpp (1-153).

## Scope

Flag stripes, the mix, and the banner frame from launcherbanner.cpp (1-153).

## Native ranges

- `src/widgets/launcherbanner.cpp` **getColors** 1–153 (153 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmLauncherBanner1289Slice.cs`
Managed LOC (tally): 182.
Conversion commit: `e2fc0a0739c7d7b374e6cd09e99b86fef8da5a27`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Flag table and expansion (**33–105**) | yes |
| Constructor (**107–135**) | yes |
| Preferred height (**137–140**) | yes |
| Layout (**142–153**) | yes |
| Band hull (**1–153**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Header color | host background |
| Theme mode | host light or dark |
| Clock | host millisecond count |
| Image load | path only |

A stop of N stores that color N+1 times, because the inner walk includes zero. Names compare in ASCII case fold. `random` and `list` take the clock modulo the table length. `list` prints every table name in lowercase and the selected name. A non-positive mix still builds stripes, swaps the base to the header color, and negates the mix before clamping it into 0..1, so -2 paints the flag color and selects the dark logo. An empty name skips stripes. An unknown name keeps the light logo when the mix is positive. The stripe top is the floor of the band, and the logo can sit at a negative x.

| Managed (`VmLauncherBanner1289Slice.cs`) | Native (`launcherbanner.cpp`) |
|------------------------------------------|-------------------------------|
| Stripes, mix, and the frame | **1–153** |
| Band hull (exclusive slice) | **1–153** |

### Chunk **1289** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **153** |
| **Net headline** | **+153** vs post-**1288** |


**Post-review ratio:** 182/153 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmLauncherBanner1289SliceTests.Setup_RepeatsTheStripeAndPrintsTheClockFlag`
- `VmLauncherBanner1289SliceTests.Layout_FloorsTheStripeAndCentersTheLogo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1289 --to 1289` (0 hard / 0 soft).

## Seams

The header color, the theme mode, and the clock are host inputs. The image load is the path string.

## Credit rationale

UNCredited launcherbanner.cpp 1-153 (+153). A full tally line scan shows no overlap. The file ends at line 153.
