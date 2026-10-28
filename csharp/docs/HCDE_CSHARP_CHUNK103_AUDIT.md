# Chunk 103 — EOCD scan, multipart rejection, compression/encryption entry filters (not central-directory parse; SharpCompress decodes). Retired (2026-10-23): out-of-body credit removed; honest **80** < **150**.

## Scope

EOCD scan, multipart rejection, compression/encryption entry filters (not central-directory parse; SharpCompress decodes). Retired (2026-10-23): out-of-body credit removed; honest **80** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0a5dc48b2d84e5bb7600370f8db1e9cd1318e080`.

## Padding review (2026-10-21)



### Out-of-body uncredit (103)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **93** |
| **Out-of-body removed** | **13** |
| **Honest** | **80** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 103 --to 103` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
