# Chunk 344 — Teleport TID rewrite and missing-texture warn cap (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Scope

Teleport TID rewrite and missing-texture warn cap (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `9600d31fc62bb61fab4152a589af9fe1dca8e68d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (344)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **43** |
| **Out-of-body removed** | **4** |
| **Honest** | **39** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 344 --to 344` (0 hard / 0 soft).

## Seams

Thinker iteration and `TexMan` IO remain outside managed code; TID assignment and missing-texture warn caps mirror loader policy.

## Credit rationale

Credits `TranslateTeleportThings` TID/`line.args` rewrite and `SetTexture` warn-limit plus flat-name truncation only.
