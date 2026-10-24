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

## Managed

`VmP3dMidtex.cs`
Managed LOC (tally): 251.
Conversion commit: `63c0ca37362190523a1c92cbc97d8f2771dd588c`.

## Padding review (2026-10-21)

| Managed | Native (`p_3dmidtex.cpp`) |
|---------|---------------------------|
| `Scroll3dMidtex` | **43–64** |
| `Start3dMidtexInterpolations` | **75–86** |
| `Attach3dMidtexLinesToSector` | **100–203** |
| `GetMidTexturePosition` | **213–247** |
| `LineOpening3dMidtex` | **274–312** |

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

## Uncredited-range check (pre-port)

```
uncredited_range_check.py --chunk 811
  [UNCredited] P_Scroll3dMidtex … P_LineOpening_3dMidtex (all bands)
exit 0
```

## Portfolio delta

| | Native LOC |
|---|---:|
| **Gross** (`p-3dmidtex-scroll-attach-chunk811`) | **+212** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+212** (93,553 → **93,765**) |

## Land hygiene

**Not a single `land_chunk_fast.sh` run:** Port **`63c0ca37`**; first script pass stopped on reaudit (`thin_test_band` on `P_Attach3dMidtexLinesToSector`). Tally/audit landed in **`b0408143`** (second attach test + `loc_tally` matrix) without a separate `loc_tally and matrix for chunk 811` commit message. **`f20fd1d9..9bb5191c`** pushed **810 fixes + 811 + 812 + 813** in one `git push` — policy going forward: **one chunk → one `land_chunk_fast.sh` → one push**.

## Credit rationale

First tally credit for Eternity-style 3D midtex sector attachment and opening adjustment.
