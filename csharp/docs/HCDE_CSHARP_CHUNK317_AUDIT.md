# Chunk 317 — FinishDehPatch weapon ammo processing Retired (2026-10-23): out-of-body credit removed; honest **76** < **150**.

## Scope

FinishDehPatch weapon ammo processing Retired (2026-10-23): out-of-body credit removed; honest **76** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedFinishDehPatch.cs` / `VmDehackedFinishWeaponAmmo.cs`
Managed LOC (tally): 0.
Conversion commit: `8b7fc06b9c944e7123c5165fcb7f5bfd58b51e5f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (317)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **76** |
| **Out-of-body removed** | **0** |
| **Honest** | **76** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 317 --to 317` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
