# Chunk 669 — Whole `SightCheck::init`, `P_SightOpening`, `LineBlocksSight`, `P_SightCheckLine`, `P_SightBlockLinesIterator`, `P_ResetSightCounters` (`p_sight.cpp` 126–145, 156–199, 388–523, 962–974). Retired (2026-10-24): `--band` hull overlaps existing credit (p-sight-ptr-traverse-chunk663 src/playsim/p_sight.cpp 210-384).

## Scope

Whole `SightCheck::init`, `P_SightOpening`, `LineBlocksSight`, `P_SightCheckLine`, `P_SightBlockLinesIterator`, `P_ResetSightCounters` (`p_sight.cpp` 126–145, 156–199, 388–523, 962–974). Retired (2026-10-24): `--band` hull overlaps existing credit (p-sight-ptr-traverse-chunk663 src/playsim/p_sight.cpp 210-384).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4af72aa053718e18061c31202ac634a31208223d`.

## Padding review (2026-10-21)

| Managed | Native (`p_sight.cpp`) |
|---------|----------------------|
| `InitSightCheck` | `SightCheck::init` 126–145 |
| `SightOpening` | `P_SightOpening` 156–199 |
| `LineBlocksSight` | `LineBlocksSight` 388–422 |
| `SightCheckLine` | `P_SightCheckLine` 432–466 |
| `SightBlockLinesIterator` | `P_SightBlockLinesIterator` 476–523 |
| `ResetSightCounters` | `P_ResetSightCounters` 962–974 |

### Out-of-body uncredit (669)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **195** |
| **Out-of-body removed** | **0** |
| **Honest** | **195** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 669 --to 669` (0 hard / 0 soft).

## Seams

Blockmap storage, polyobject lines, `P_MakeDivline`, and `P_PointOnDivlineSide` remain native.

## Credit rationale

Sight line gating after PTR/path chunks 663–665.
