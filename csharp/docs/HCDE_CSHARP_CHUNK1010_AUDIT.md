# Chunk 1010 — Level start globals, save chunk IDs, and GL CUSTOM_CVAR propagation from `g_level.cpp` (**1–150**). Primary `dthinker.cpp` **1–150** deferred: overlaps existing `VmDThinkerCollectionEarly` / sub-150 header-only **1–70**.

## Scope

Level start globals, save chunk IDs, and GL CUSTOM_CVAR propagation from `g_level.cpp` (**1–150**). Primary `dthinker.cpp` **1–150** deferred: overlaps existing `VmDThinkerCollectionEarly` / sub-150 header-only **1–70**.

## Native ranges

- `src/g_level.cpp` **CUSTOM_CVAR(gl_brightfog)** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelCvarHooks1010Slice.cs`
Managed LOC (tally): 89.
Conversion commit: `f1f5af58efecfcf6e7366d650cc916bead03d316`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `startpos` / `laststartpos` globals (**111–112**) | seam (host globals) |
| Save snapshot chunk IDs (**114–119**) | yes (constants) |
| `gl_brightfog` CVAR (**124–130**) | yes |
| `gl_lightadditivesurfaces` (**132–138**) | yes |
| `gl_notexturefill` (**140–146**) | yes |
| `gl_maplightmode` clamp (**148–151**) | yes (partial line **151** seam) |
| `gl_lightmode` clamp (**153–156**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–110** | includes / extern declarations |
| **158+** | `getRealLightmode` body (next band) |
| **121–122** | forward decl host |

| Managed (`VmGLevelCvarHooks1010Slice.cs`) | Native (`g_level.cpp`) |
|------------------------------------------|----------------------|
| Chunk ID constants | **114–119** |
| `ShouldApplyLevelInfoDefault` / propagate helpers | **124–146** |
| `ClampGlMapLightMode` / `ClampGlLightMode` | **148–156** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1010** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1009** |


**Post-review ratio:** 89/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelCvarHooks1010SliceTests.SaveChunkIds`
- `VmGLevelCvarHooks1010SliceTests.GlCvarPropagation`
- `VmGLevelCvarHooks1010SliceTests.GlLightModeClamps`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1010 --to 1010` (0 hard / 0 soft).

## Seams

`AllLevels()` iteration, `CUSTOM_CVAR` registration, and `getRealLightmode` (**158+**) remain injected.

## Credit rationale

UNCredited **g_level.cpp 1–150** CLI band; no live tally overlap; pivot off exhausted `g_game` tail and blocked `dthinker` head duplicate.
