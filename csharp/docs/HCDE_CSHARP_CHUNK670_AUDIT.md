# Chunk 670 — Whole `FPolyObj::ThrustMobj` and `CheckMobjBlocking` (`po_man.cpp` 749–795, 1025–1146). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-move-rotate-chunk672 src/playsim/po_man.cpp 858-888).

## Scope

Whole `FPolyObj::ThrustMobj` and `CheckMobjBlocking` (`po_man.cpp` 749–795, 1025–1146). Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-move-rotate-chunk672 src/playsim/po_man.cpp 858-888).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `49eeb92a134c3dcf31943ba9a9e25c606856ee04`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `ThrustMobj` | `FPolyObj::ThrustMobj` 749–795 |
| `CheckMobjBlocking` | `FPolyObj::CheckMobjBlocking` 1025–1146 |

### Out-of-body uncredit (670)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **169** |
| **Out-of-body removed** | **0** |
| **Honest** | **169** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 670 --to 670` (0 hard / 0 soft).

## Seams

`P_CheckMove`, `P_DamageMobj`, blockmap iteration, and `P_IsBlockedByLine` remain native.

## Credit rationale

First `po_man.cpp` credit for polyobject–mobj interaction.
