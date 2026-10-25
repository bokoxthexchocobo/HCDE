# Chunk 737 — Whole `AActor::SetState`, inventory destroy/use, and `FindInventory` (`p_mobj.cpp` 856–961, 978–1005, 1024–1035, 1064–1095). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-inventory-give-chunk738 src/playsim/p_mobj.cpp 1045-1056).

## Scope

Whole `AActor::SetState`, inventory destroy/use, and `FindInventory` (`p_mobj.cpp` 856–961, 978–1005, 1024–1035, 1064–1095). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-inventory-give-chunk738 src/playsim/p_mobj.cpp 1045-1056).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d29df129b7a6863a6f5f77c61d40811b36871adf`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SetState` | `AActor::SetState` 856–961 |
| `DestroyAllInventory` | static `DestroyAllInventory` 978–1005 |
| `UseInventory` | `AActor::UseInventory` 1024–1035 |
| `FindInventoryByClass` / `FindInventoryByName` | `AActor::FindInventory` 1064–1095 |

### Out-of-body uncredit (737)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **177** |
| **Out-of-body removed** | **0** |
| **Honest** | **177** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 737 --to 737` (0 hard / 0 soft).

## Seams

`CallAction`, skin tables, `VMCall`, inventory `Destroy`, and `PClass::FindActor` are injected.

## Credit rationale

State machine plus inventory helpers; VM DEFINE wrappers excluded (no managed glue).
