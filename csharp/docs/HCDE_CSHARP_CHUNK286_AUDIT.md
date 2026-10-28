# Chunk 286 — ResolveSiblingLump and ApplyEMapInfoProperty head Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

ResolveSiblingLump and ApplyEMapInfoProperty head Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `e180325d586e79009e6756af77e61c6e6d451ac5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (286)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **150** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 286 --to 286` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
