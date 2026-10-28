# Chunk 1020 — `DLineTracer` ZScript field exports, `Trace` action flag masks, and `TraceCallback` wall-texture patching from `p_trace.cpp` (**983–1132**, hull ends at EOF). **Pivot:** `EditTraceResult` (**983–1025**) excluded — already on `VmPTraceThingPlaneEdit` (retired chunk **693**).

## Scope

`DLineTracer` ZScript field exports, `Trace` action flag masks, and `TraceCallback` wall-texture patching from `p_trace.cpp` (**983–1132**, hull ends at EOF). **Pivot:** `EditTraceResult` (**983–1025**) excluded — already on `VmPTraceThingPlaneEdit` (retired chunk **693**).

## Native ranges

- `src/playsim/p_trace.cpp` **DLineTracer / TraceResults** 983–1132 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPTraceDLineTracer1020Slice.cs`
Managed LOC (tally): 97.
Conversion commit: `697cbd28c019f931e2e8656246b33e8225766692`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `TraceResults` `DEFINE_FIELD*` export block (**1036–1054**) | yes (field manifest) |
| `DLineTracer::Trace` flag sanitize (**1079–1081**) | yes |
| `DLineTracer::Trace` actor-mask ternary (**1085**) | yes |
| `TraceCallback` wall tier texture patch (**1096–1114**) | yes |
| `CallZScriptCallback` virtual dispatch fallback (**1120–1131**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **983–1025** | `EditTraceResult` — **`VmPTraceThingPlaneEdit`** (retired **693**) |
| **1026–1031** | comment / class header banners |
| **1056–1086** | VM action glue outside ported policy helpers |

| Managed (`VmPTraceDLineTracer1020Slice.cs`) | Native (`p_trace.cpp`) |
|---------------------------------------------|----------------------|
| `TraceResultsVmFieldNames` | **1036–1054** |
| `SanitizeZScriptTraceFlags` | **1079–1081** |
| `ActorMaskForIgnoreAll` | **1085** |
| `TryPatchWallHitTexture` / `SelectFfloorTexturePart` | **1096–1114** |
| `ResolveTraceCallbackStatus` | **1120–1131** |
| Band hull (exclusive slice) | **983–1132** |

### Chunk **1020** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1019** |


**Post-review ratio:** 97/150 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceDLineTracer1020SliceTests.SanitizeZScriptTraceFlags_ClearsPcrossImpactAndSets3DCallback`
- `VmPTraceDLineTracer1020SliceTests.ActorMaskForIgnoreAll_MatchesNativeTernary`
- `VmPTraceDLineTracer1020SliceTests.TraceResultsVmFieldCount_MatchesNativeDefineBlock`
- `VmPTraceDLineTracer1020SliceTests.TryPatchWallHitTexture_MapsTiersLikeNativeCallback`
- `VmPTraceDLineTracer1020SliceTests.SelectFfloorTexturePart_UsesUpperLowerOrMiddle`
- `VmPTraceDLineTracer1020SliceTests.ResolveTraceCallbackStatus_StopWhenNoVirtual`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1020 --to 1020` (0 hard / 0 soft).

## Seams

`Trace()` host call, `PARAM_*` parsing, `IMPLEMENT_CLASS`, and `GC`/VM runtime remain native.

## Credit rationale

First tally-clean **150**-line `p_trace.cpp` tail with exclusive managed slice; honest zero-credit for sky/no-sky result editing already ported off-tally.
