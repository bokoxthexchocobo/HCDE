# Chunk 276 — PatchSound consumer and PatchPointer Boom hack Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Scope

PatchSound consumer and PatchPointer Boom hack Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4262627847645ad4f83c2ce6b98dd19746c24ab2`.

## Padding review (2026-10-21)



### Out-of-body uncredit (276)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **4** |
| **Honest** | **146** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 276 --to 276` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
