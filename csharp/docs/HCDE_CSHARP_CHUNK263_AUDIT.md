# Chunk 263 — FindStyle, MBF21 flags, SetDehParams guards, PatchThing group clamps, DEHSUPP RenderStyles. Retired (2026-10-23): out-of-body credit removed; honest **119** < **150**.

## Scope

FindStyle, MBF21 flags, SetDehParams guards, PatchThing group clamps, DEHSUPP RenderStyles. Retired (2026-10-23): out-of-body credit removed; honest **119** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppThingBits.cs` / `VmDehackedDehSuppRenderStyles.cs`
Managed LOC (tally): 0.
Conversion commit: `3d92bfe2bcaa627eb79c1f810328092abd2f3c6a`.

## Padding review (2026-10-21)



### Out-of-body uncredit (263)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **36** |
| **Honest** | **119** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 263 --to 263` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
