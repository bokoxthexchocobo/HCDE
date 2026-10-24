# Chunk 216 — HU font constants and InitDoomFonts small-font path (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **57** < **150**.

## Scope

HU font constants and InitDoomFonts small-font path (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **57** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c036923138050884668fe9ca0d5c09f99b62d7b4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (216)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **62** |
| **Out-of-body removed** | **5** |
| **Honest** | **57** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 216 --to 216` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits `HU` font size constants and small-font lump preference — not the full fallback chain.
