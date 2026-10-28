# Chunk 231 — g_skill/teaminfo uncredited gap lines. Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Scope

g_skill/teaminfo uncredited gap lines. Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a81696a1a02528a65d31009f71271f1cd8b11817`.

## Padding review (2026-10-21)



### Out-of-body uncredit (231)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **195** |
| **Out-of-body removed** | **186** |
| **Honest** | **9** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 231 --to 231` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
