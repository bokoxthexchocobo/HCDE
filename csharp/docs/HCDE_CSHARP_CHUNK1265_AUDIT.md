# Chunk 1265 — Shadow-map cvar clamps, the light-counter reset, and the buffer upload from hw_shadowmap.cpp (1-213).

## Scope

Shadow-map cvar clamps, the light-counter reset, and the buffer upload from hw_shadowmap.cpp (1-213).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_shadowmap.cpp` **IShadowMap** 1–213 (213 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmShadowMap1265Slice.cs`
Managed LOC (tally): 269.
Conversion commit: `86b40426bb0c851c2e621de7385e27fcdf48fb63`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Cvar clamps (**72–131**) | yes |
| ShadowTest (**133–139**) | yes |
| PerformUpdate (**141–165**) | yes |
| UploadLights (**167–179**) | yes |
| UploadAABBTree (**182–201**) | yes |
| Reset (**203–208**) | yes |
| Destructor (**210–213**) | yes |
| Band hull (**1–213**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| AABB ray test | host tree |
| GPU buffer object | host allocation |

The target-time cvar clamps below one quarter to one quarter and above ten to ten. The minimum and maximum light counts clamp into zero through 1024. The step clamps into one through 256. Quality keeps 128, 256, 512, 1024, 2048, 4096, and 8192, and any other value becomes 128. The stat line prints the upload time, the light counters, the runtime cap over the hard cap, and on or off for adaptive and priority. A shadow test with no tree, or with the shadow cvar off, is lit. With both present, the ray distance is lit at one or above. PerformUpdate zeros the light counters, sets the updated row count to one, and does not touch the budget caps. A null collector returns false before the clock. Otherwise it uploads the tree and then the lights. The light array grows to 4096 floats only when its length differs, and the existing prefix is kept. The list buffer is created once at binding six and then filled as a stream of four times that length. A new tree creates the node and line buffers at bindings four and five and uploads them as static. A quiet tree does not write. A tree that reports an update writes the dynamic sub-ranges and faults when those buffers are missing. Reset drops whichever of the three buffers exist. The destructor resets and does not drop a second time.

| Managed (`VmShadowMap1265Slice.cs`) | Native (`hw_shadowmap.cpp`) |
|------------------------------------|--------------------------|
| Clamps, counters, and buffer upload | **1–213** |
| Band hull (exclusive slice) | **1–213** |

### Chunk **1265** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **213** |
| **Net headline** | **+213** vs post-**1264** |


**Post-review ratio:** 269/213 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmShadowMap1265SliceTests.Clamps_SkipTheUploadAndTreatAMissAsLit`
- `VmShadowMap1265SliceTests.Upload_ReusesTheLightBufferAndFaultsAMissingTree`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1265 --to 1265` (0 hard / 0 soft).

## Seams

The AABB ray test, the cycle timer, and the GPU buffer objects are host steps. The slice keeps the cvar clamps, the counter reset, the 4096-float light list, and which buffer binding and usage this file requests.

## Credit rationale

UNCredited hw_shadowmap.cpp 1-213 (+213). A full tally line scan shows no overlap. The file ends at line 213.
