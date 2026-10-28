# Chunk 223 — TranslateLineDef Boomish branch, sector special translation, xlat expressions. Retired (2026-10-23): `check_port_coverage` reports `managed_src_head=0` — chunk manifest has no `managedSourceFiles` and tally `conversionManagedCommit` was a tally-only SHA (`a2e37df1`); adding port SHA on manifest inflated `managedLoc` (576-style padding). Honest in-body **264** after OOB trim; VmXlat port stays off-tally.

## Scope

TranslateLineDef Boomish branch, sector special translation, xlat expressions. Retired (2026-10-23): `check_port_coverage` reports `managed_src_head=0` — chunk manifest has no `managedSourceFiles` and tally `conversionManagedCommit` was a tally-only SHA (`a2e37df1`); adding port SHA on manifest inflated `managedLoc` (576-style padding). Honest in-body **264** after OOB trim; VmXlat port stays off-tally.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `95fd5398cb5395c2686d6417805bd315f50b75f6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (223)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **276** |
| **Out-of-body removed** | **12** |
| **Honest** | **264** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 223 --to 223` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
