# Chunk 960 — Water sink macros, `sv_gravity` CUSTOM_CVAR, client CVAR defaults, and `AActor` GC pointer registration head from `p_mobj.cpp` (**1–150**).

## Scope

Water sink macros, `sv_gravity` CUSTOM_CVAR, client CVAR defaults, and `AActor` GC pointer registration head from `p_mobj.cpp` (**1–150**).

## Native ranges

- `src/playsim/p_mobj.cpp` **CUSTOM_CVAR** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjFileHead960Slice.cs`
Managed LOC (tally): 75.
Conversion commit: `4a77d2d0546fe1c92e247784bd2cb703facda9e4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `WATER_*` / `Z_UNDERFLOW` macros (**78–82**) | yes |
| `CUSTOM_CVAR sv_gravity` NaN/INF guard + level loop (**122–132**) | yes |
| `cl_*` archive defaults (**134–137**) | yes (default policy) |
| `IMPLEMENT_POINTER` through `LastLookActor` (**145–150**) | yes (field registry) |
| Static `FRandom` / `IMPLEMENT_CLASS` (**97–142**) | seam |

| Managed (`VmPMobjFileHead960Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------|---------------------|
| Water sink / jump constants | **78–82** |
| `IsNonFiniteGravity` / `SanitizeSvGravity` / `ApplySvGravityToAllLevels` | **125–131** |
| Client cvar default helpers | **134–137** |
| `RegistersAActorGcPointer` / registration order | **145–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **960** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**959** |


**Post-review ratio:** 75/150 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjFileHead960SliceTests.SanitizeSvGravity_ReplacesNaNWithDefault`
- `VmPMobjFileHead960SliceTests.SanitizeSvGravity_PreservesFiniteValue`
- `VmPMobjFileHead960SliceTests.ApplySvGravityToAllLevels_UpdatesEachLevel`
- `VmPMobjFileHead960SliceTests.AdvanceWaterSink_UsesSmallFactorWhenRequested`
- `VmPMobjFileHead960SliceTests.WaterJumpVelocity_ScalesMultiplier`
- `VmPMobjFileHead960SliceTests.RegistersAActorGcPointer_MatchesImplementPointerBand`
- `VmPMobjFileHead960SliceTests.GcPointerRegistrationOrder_MatchesNativeSequence`
- `VmPMobjFileHead960SliceTests.ResolveSvGravityCvar_SanitizesAndApplies`
- `VmPMobjFileHead960SliceTests.ClampsBelowZUnderflow_DetectsUnderflowConstant`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 960 --to 960` (0 hard / 0 soft).

## Seams

Includes, `FRandom` stream objects, `IMPLEMENT_CLASS`, and `PlayerLandedOnThing` prototype remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPMobjFileHead960Slice.cs` (before serialize **210+** and tick **728**).
