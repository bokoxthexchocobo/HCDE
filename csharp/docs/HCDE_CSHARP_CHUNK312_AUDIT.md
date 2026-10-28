# Chunk 312 — LoadDehSupp OrgSprNames section Retired (2026-10-23): out-of-body credit removed; honest **26** < **150**.

## Scope

LoadDehSupp OrgSprNames section Retired (2026-10-23): out-of-body credit removed; honest **26** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppOrgSprNames.cs`
Managed LOC (tally): 0.
Conversion commit: `ae8b13d650a423a91cef227abccc26bd72f16302`.

## Padding review (2026-10-21)



### Out-of-body uncredit (312)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **28** |
| **Out-of-body removed** | **2** |
| **Honest** | **26** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 312 --to 312` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
