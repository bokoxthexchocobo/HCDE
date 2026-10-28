# Chunk 104 — VmGrp/VmHog/VmRff readers in b051701e; chunk 71ea4f56 publicized types and added VmLegacyArchiveReaderTests only. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

VmGrp/VmHog/VmRff readers in b051701e; chunk 71ea4f56 publicized types and added VmLegacyArchiveReaderTests only. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ecb9a86ca654bb3d5e4aa9be2ec87cf23b503b2c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (104)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **179** |
| **Out-of-body removed** | **179** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 104 --to 104` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
