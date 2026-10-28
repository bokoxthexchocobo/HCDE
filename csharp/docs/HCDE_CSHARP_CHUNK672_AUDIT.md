# Chunk 672 — Whole `EV_RotatePoly`, `EV_MovePoly`, `FPolyObj::MovePolyobj`, `DoMovePolyobj`, `RotatePolyobj` (`po_man.cpp` 322–368, 405–452, 858–982). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-door-chunk671 src/playsim/po_man.cpp 527-625).

## Scope

Whole `EV_RotatePoly`, `EV_MovePoly`, `FPolyObj::MovePolyobj`, `DoMovePolyobj`, `RotatePolyobj` (`po_man.cpp` 322–368, 405–452, 858–982). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-door-chunk671 src/playsim/po_man.cpp 527-625).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `902accb8a4f41aa4ce811724395c4d2218f94a40`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `EvRotatePoly` | `EV_RotatePoly` 322–368 |
| `MovePolyobj` / `DoMovePolyobj` | 858–910 |
| `RotatePolyobj` | 935–982 |
| `EvMovePoly` | `EV_MovePoly` 405–452 |

### Out-of-body uncredit (672)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **189** |
| **Out-of-body removed** | **7** |
| **Honest** | **182** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 672 --to 672` (0 hard / 0 soft).

## Seams

`UnLinkPolyobj`, `LinkPolyobj`, `ClearSubsectorLinks`, and thinker spawn remain native.

## Credit rationale

Polyobject kinematics and move line special.
