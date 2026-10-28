# Chunk 673 — Whole link/update/spatial polyobj helpers (`po_man.cpp` 803–856, 990–1017, 1154–1248, 1259–1384). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-thinker-init-chunk678 src/playsim/po_man.cpp 918-927).

## Scope

Whole link/update/spatial polyobj helpers (`po_man.cpp` 803–856, 990–1017, 1154–1248, 1259–1384). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-thinker-init-chunk678 src/playsim/po_man.cpp 918-927).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e9147c4cc57160314ce0ce1aad042df99452168d`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `UpdateLinks` | `FPolyObj::UpdateLinks` 803–831 |
| `UpdateBBox` | `FPolyObj::UpdateBBox` 833–840 |
| `CalcCenter` | `FPolyObj::CalcCenter` 842–850 |
| `UnlinkPolyobj` | `FPolyObj::UnLinkPolyobj` 990–1017 |
| `LinkPolyobj` | `FPolyObj::LinkPolyobj` 1154–1217 |
| `RecalcActorFloorCeil` | `FPolyObj::RecalcActorFloorCeil` 1228–1248 |
| `ClosestPoint` / `PoBusy` / `ClearSubsectorLinks` / `ClearAllSubsectorLinks` | 1259–1384 |

### Out-of-body uncredit (673)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **268** |
| **Out-of-body removed** | **0** |
| **Honest** | **268** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 673 --to 673` (0 hard / 0 soft).

## Seams

Blockmap storage, `P_FindFloorCeiling`, line portal host data, and polyblock allocation remain native.

## Credit rationale

Polyobject bbox/link maintenance after move/rotate chunks 670–672.
