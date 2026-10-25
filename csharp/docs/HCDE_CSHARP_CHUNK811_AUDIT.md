# Chunk 811 — `P_Scroll3dMidtex`, `P_Start3dMidtexInterpolations`, `P_Attach3dMidtexLinesToSector`, `P_GetMidTexturePosition`, and `P_LineOpening_3dMidtex` from `p_3dmidtex.cpp` (43–324).

## Scope

`P_Scroll3dMidtex`, `P_Start3dMidtexInterpolations`, `P_Attach3dMidtexLinesToSector`, `P_GetMidTexturePosition`, and `P_LineOpening_3dMidtex` from `p_3dmidtex.cpp` (43–324).

## Native ranges

- `src/playsim/p_3dmidtex.cpp` **P_Scroll3dMidtex** 43–64 (22 lines)
- `src/playsim/p_3dmidtex.cpp` **P_Start3dMidtexInterpolations** 75–86 (12 lines)
- `src/playsim/p_3dmidtex.cpp` **P_Attach3dMidtexLinesToSector** 100–203 (104 lines)
- `src/playsim/p_3dmidtex.cpp` **P_GetMidTexturePosition** 213–247 (35 lines)
- `src/playsim/p_3dmidtex.cpp` **P_LineOpening_3dMidtex** 274–312 (39 lines)

**Credited native LOC:** 212 (tally `nativeLoc` 212).

## Portfolio delta

**Gross CLI band:** **282** lines in the UNCredited-range probe; tally credits **212** whole-function native LOC only.
**70** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **282** |
| **Credited** (whole functions, this row) | **212** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+212** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmP3dMidtex.cs`
Managed LOC (tally): 251.
Conversion commit: `9b013448295a327b99dfb44034e62a043941f948`.

## Padding review (2026-10-21)

| Managed | Native (`p_3dmidtex.cpp`) |
|---------|---------------------------|
| `Scroll3dMidtex` | **43–64** |
| `Start3dMidtexInterpolations` | **75–86** |
| `Attach3dMidtexLinesToSector` | **100–203** |
| `GetMidTexturePosition` | **213–247** |
| `LineOpening3dMidtex` | **274–312** |

### Out-of-body uncredit (811)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **212** |
| **Out-of-body removed** | **0** |
| **Honest** | **212** |


**Post-review ratio:** 251/212 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmP3dMidtexTests.Scroll3dMidtex_OffsetsSidesAndChangesSectors`
- `VmP3dMidtexTests.Attach3dMidtexLinesToSector_ByLineId`
- `VmP3dMidtexTests.Attach3dMidtexLinesToSector_BySectorTag`
- `VmP3dMidtexTests.GetMidTexturePosition_DontPegBottom`
- `VmP3dMidtexTests.LineOpening3dMidtex_AdjustsCeilingWhenBelowMidpoint`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 811 --to 811` (0 hard / 0 soft).

## Seams

`P_ChangeSector`, line/tag iterators, `TexMan`, and `FLineOpening` plane writes are injected.

## Credit rationale

First tally credit for Eternity-style 3D midtex sector attachment and opening adjustment.
