# Chunk 313 — DehSupp SoundMap InfoNames ThingBits and load errors (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **59** < **150**.

## Scope

DehSupp SoundMap InfoNames ThingBits and load errors (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **59** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppSoundInfo.cs`, `VmDehackedDehSuppThingBits.cs` / `VmDehackedDehSuppRenderStyles.cs`
Managed LOC (tally): 0.
Conversion commit: `29b51210a1e4c675c789cf71e4d9e0f0cd4b2c7f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (313)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **69** |
| **Out-of-body removed** | **10** |
| **Honest** | **59** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 313 --to 313` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits secondary DehSupp sections and lump error gates — not the full `StateMap` actor/state expansion loop.
