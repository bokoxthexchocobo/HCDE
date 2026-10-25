# Chunk 680 — Whole `FBlockLinesIterator` block walk, `FPathTraverse::AddLineIntercepts`, and `FPathTraverse::Next` (`p_maputl.cpp` 563–683, 1154–1185, 1370–1388). Retired (2026-10-24): `--band` hull overlaps existing credit (p-maputl-add-thing-intercepts-chunk668 src/playsim/p_maputl.cpp 1194-1361).

## Scope

Whole `FBlockLinesIterator` block walk, `FPathTraverse::AddLineIntercepts`, and `FPathTraverse::Next` (`p_maputl.cpp` 563–683, 1154–1185, 1370–1388). Retired (2026-10-24): `--band` hull overlaps existing credit (p-maputl-add-thing-intercepts-chunk668 src/playsim/p_maputl.cpp 1194-1361).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c0c1cf7b3a308af117156c475edf95b7019af98f`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `Create` / `InitFromBox` | `FBlockLinesIterator` ctors 563–588 |
| `StartBlock` | `FBlockLinesIterator::StartBlock` 596–614 |
| `Next` (block lines) | `FBlockLinesIterator::Next` 622–683 |
| `AddLineIntercepts` | `FPathTraverse::AddLineIntercepts` 1154–1185 |
| `Next` (intercept list) | `FPathTraverse::Next` 1370–1388 |

### Out-of-body uncredit (680)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **158** |
| **Out-of-body removed** | **2** |
| **Honest** | **156** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 680 --to 680` (0 hard / 0 soft).

## Seams

`P_PointOnDivlineSide`, `P_MakeDivline`, `P_InterceptVector`, and poly block map storage remain native.

## Credit rationale

First `p_maputl.cpp` path-traverse line intercept band after thing intercepts in chunk 668.
