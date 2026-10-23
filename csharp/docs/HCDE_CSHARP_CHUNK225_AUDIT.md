# Chunk 225 — Uncredited InitDoomFonts FFont constructors (chunk216 credited lump branches 36-92 without splitting FFont lines). Retired (2026-10-23): out-of-body credit removed; honest **6** < **150**.

## Scope

Uncredited InitDoomFonts FFont constructors (chunk216 credited lump branches 36-92 without splitting FFont lines). Retired (2026-10-23): out-of-body credit removed; honest **6** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `2d4a8fd29f13650331771236aa18b8122cd9f393`.

## Padding review (2026-10-21)



### Out-of-body uncredit (225)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **6** |
| **Out-of-body removed** | **0** |
| **Honest** | **6** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 225 --to 225` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
