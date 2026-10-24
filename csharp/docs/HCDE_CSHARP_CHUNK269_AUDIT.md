# Chunk 269 — PatchFrame property dispatch and MBF21 bits. Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

PatchFrame property dispatch and MBF21 bits. Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `87fdf4e90bf74ce2a5d0f0fc83ebb5770532185e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (269)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **154** |
| **Out-of-body removed** | **5** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 269 --to 269` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
