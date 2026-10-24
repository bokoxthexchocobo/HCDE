# Chunk 893 — Retired (2026-10-23): honest whole-function credit for `AlignPlane` / `SetSlopes` / `CopySlopes` (`slopes.cpp` **439–588**) is **136** native lines (< **150** floor). Separator spans **502–508** and **540–546** were invalid padding to reach **150** — rejected. Matrix credit **0**; band uncredited.

## Scope

Retired (2026-10-23): honest whole-function credit for `AlignPlane` / `SetSlopes` / `CopySlopes` (`slopes.cpp` **439–588**) is **136** native lines (< **150** floor). Separator spans **502–508** and **540–546** were invalid padding to reach **150** — rejected. Matrix credit **0**; band uncredited.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapLoaderSlopes.cs`, `VmMapLoaderSlopesTests.cs`
Managed LOC (tally): 0.
Conversion commit: `fdb39b3b7bdf17d92f1a8dfac676a2b36bef3898`.

## Padding review (2026-10-21)

### Incident — separator padding to **150** (rejected)

| Issue | Detail |
|-------|--------|
| Honest function `nativeLoc` | **136** (`AlignPlane` **63** + `SetSlopes` **31** + `CopySlopes` **42**) |
| Invalid top-up | Credited banner/blank spans **502–508**, **540–546** (**14** lines) to reach **150** |
| Verdict | **Padding** — retire to **0** matrix credit; do not stretch band |

### Incident — manual `fix_chunk_tally.py` before `--resume` (banned)

| Command (manual) | Why run |
|------------------|--------|
| `python3 csharp/tools/fix_chunk_tally.py 892 $(git rev-parse HEAD)` | **`managedLoc_mismatch`** after partial-class edit on **892** file during **893** WIP |
| `python3 csharp/tools/fix_chunk_tally.py 893 8201958d…` | Pre-sync tally before **`land_chunk_fast.sh --resume 893`** |
| `sync_loc_tally.py` / `generate_native_managed_matrix.py` / `reaudit_chunks.py` | Same manual land bypass — **invalid**; superseded by this retire land |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** |

### Gross vs net (**893** retire)

| | Value |
|--|------:|
| Gross attempted (invalid land, incl. separator padding) | **150** |
| Honest whole-function only | **136** |
| Net behavior-ported (matrix) | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 893 --to 893` (0 hard / 0 soft).

## Seams

Invalid land used `partial` / `SlopeLine.Special` on chunk **892** file; reverted with retire. Off-tally slope align/copy not claimed.

## Credit rationale

Standing policy: no separator/blank/banner credit to hit line counts; sub-150 honest bands retire instead of top-up.
