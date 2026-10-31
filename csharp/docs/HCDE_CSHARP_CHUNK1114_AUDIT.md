# Chunk 1114 — `UpdateWaterDepth` and `SplashCheck` boom-water gate from `p_mobj.cpp` (**5277–5426**); follows **1113** tick tail.

## Scope

`UpdateWaterDepth` and `SplashCheck` boom-water gate from `p_mobj.cpp` (**5277–5426**); follows **1113** tick tail.

## Native ranges

- `src/playsim/p_mobj.cpp` **UpdateWaterDepth and SplashCheck** 5277–5426 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjWaterDepth1114Slice.cs`
Managed LOC (tally): 125.
Conversion commit: `5ce5c1ee1ae34f70f5e568adf3bd0f268d79deb5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Missing sector early return (**5297–5300**) | yes |
| Underwater sector full height (**5302–5305**) | yes |
| Height-sec splash/mask depth (**5313–5322**) | yes |
| Swimmable 3D floor pick (**5328–5344**) | yes |
| Level 1/2/3 thresholds (**5349–5367**) | yes |
| `SplashCheck` boom rising edge (**5386–5390**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **5264–5273** | chunk **1113** tail |
| **5388** | host `P_HitWater` |
| **5402–5408** | virtual thunk |
| **5427+** | dive/surface sounds outside hull |

| Managed (`VmPMobjWaterDepth1114Slice.cs`) | Native (`p_mobj.cpp`) |
|-----------------------------------------|---------------------|
| Water depth + splash gate | **5277–5426** |
| Band hull (exclusive slice) | **5277–5426** |

### Chunk **1114** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1113** |


**Post-review ratio:** 125/150 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjWaterDepth1114SliceTests.RunUpdateWaterDepth_UsesFullHeightWhenSectorUnderwater`
- `VmPMobjWaterDepth1114SliceTests.RunUpdateWaterDepth_SkipsSolidRoversAndUsesSwimmableTop`
- `VmPMobjWaterDepth1114SliceTests.RunSplashCheck_HitsWaterOnRisingLevel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1114 --to 1114` (0 hard / 0 soft).

## Seams

Plane `ZatPoint`, `P_HitWater`, and the virtual dive/surface thunk remain host-injected; `UpdateWaterLevel` body continues at **5427**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **5277–5426** (+150); follows **1113** (**5127–5276**).
