# HCDE k8vavoom-Style Rendering Audit

Roadmap board item: **#17** ("Rocket wants more k8vavooom style rendering
as default with shadows and raytracing like lights").

This is the scoping document for turning that broad request into reviewable
renderer work. The target is a modernized visual profile inspired by
k8vavoom's lighting-heavy presentation, but imported as HCDE renderer features
rather than as a parallel engine.

## Current Renderer Surface

| Area | Existing HCDE surface | Notes |
| --- | --- | --- |
| Hardware scene renderer | `src/rendering/hwrenderer/scene/*` | Walls, flats, sprites, BSP traversal, portals, decals, sky, fake flats. |
| Dynamic lights | `src/common/rendering/hwrenderer/data/hw_dynlightdata.h`, `src/r_data/gldefs.cpp` | GLDEFS-driven light definitions already exist. |
| Shadow maps | `src/common/rendering/hwrenderer/data/hw_shadowmap.cpp`, `gl_light_shadowmap`, `gl_shadowmap_quality`, `gl_shadowmap_maxlights`, `hcde_shadow_autofallback`, `hcde_shadow_autobudget` | Already has a shadowmap pipeline and HCDE-specific budget/fallback knobs. |
| Postprocess | `hw_postprocess.cpp`, `gl_bloom`, `gl_tonemap`, `gl_exposure`, `gl_ssao` | Bloom, tonemap, SSAO, lens, exposure are already in tree. |
| Vulkan / GL shader stack | `common/rendering/vulkan/*`, `common/rendering/gl/*` | Shader infrastructure exists; new visual experiments should enter here, not playsim. |
| Software renderer | `src/rendering/swrenderer/*` | Keep compatibility fallback. k8vavoom-style work targets hardware renderer only. |
| Brightmaps / fullbright | `autoloadbrightmaps`, `r_fullbright_overrides` scaffold | Presentation-only, no network relevance. |

## Import Boundaries

- **Presentation only.** Shadows, ray-style lighting approximations, bloom,
  SSAO, and fullbright overrides must not change actor state, collision,
  visibility used by AI, sound propagation, RNG, or savegame contents.
- **Renderer-facing facade.** New k8vavoom-inspired toggles live behind HCDE
  CVARs and feed existing renderer code paths. Do not fork map loading or
  playsim light definitions.
- **Hardware renderer first.** The software renderer stays stable and can
  ignore the new CVARs unless a simple no-op is needed.
- **Auto-profile default-on.** `hcde_k8vavoom_auto_profile` applies the preset on
  capable hardware; operators can disable with `r_k8vavoom_reset` or
  `hcde_k8vavoom_lighting_profile 0`.
- **Demo/multiplayer deterministic.** Two clients may choose different visual
  settings while receiving the same authoritative snapshots and producing the
  same demos.

## Existing CVARs To Reuse

| CVAR | Purpose |
| --- | --- |
| `gl_light_shadowmap` | Master shadowmap switch. |
| `gl_shadowmap_quality` | Shadowmap resolution. |
| `gl_shadowmap_maxlights` | Light budget cap. |
| `gl_shadowmap_prioritize` | Prioritize lights for shadowmap budget. |
| `hcde_shadow_autofallback` | HCDE automatic fallback when shadowmap support is unsuitable. |
| `hcde_shadow_autobudget` | HCDE adaptive shadow budget. |
| `hcde_shadow_autobudget_targetms` | Target upload/pass cost for adaptive budget. |
| `gl_bloom`, `gl_bloom_amount` | Bloom. |
| `gl_tonemap`, `gl_exposure` | Tonemapping / exposure. |
| `gl_ssao`, `gl_ssao_strength`, `gl_ssao_radius` | Ambient occlusion. |

## Profile CVARs and diagnostics

`src/common/rendering/hwrenderer/data/hw_k8vavoom_lighting.cpp` owns the
k8vavoom profile CVARs, capability probe, and diagnostic CCMDs:

| CVAR / CCMD | Meaning |
| --- | --- |
| `hcde_k8vavoom_auto_profile` | Default `true`. Applies the lighting profile on capable hardware at video init; Vulkan + ray-query also enables shadow boost and raylight. |
| `hcde_k8vavoom_lighting_profile` | Master profile selector. `0` = disabled, `1` = lighting-heavy preset composing shadowmap and postprocess CVARs. Setting to `0` disables auto-profile. |
| `hcde_k8vavoom_shadow_boost` | Raises shadowmap quality floor (`1024` vs `512`) and participates in raylight path selection. |
| `hcde_k8vavoom_raylight_probe` | Opt into Vulkan ray-query dynamic light shadows when `VK_KHR_ray_query` is available. |
| `vk_raytrace` | Vulkan ray-query acceleration structures; enabled by the profile before shader init when the raylight path is active. |
| `r_k8vavoom_status` | Prints composed preset snapshot, live renderer CVARs, and backend capability probe. |
| `r_k8vavoom_reset` | Disables profile/sub-flags and resets touched renderer CVARs to HCDE defaults. |

The profile is presentation-only. Enabling it is a one-shot preset apply: it
raises selected CVARs to conservative floors but does not remember or restore
previous user values when the profile is disabled. Use `r_k8vavoom_status` to
inspect active values and `r_k8vavoom_reset` to undo the preset.

## Phase 1 Implemented

1. **Shadowmap preset wrapper.** When `hcde_k8vavoom_lighting_profile=1`,
   the renderer enables `gl_light_shadowmap`, enables
   `gl_shadowmap_prioritize`, and raises `gl_shadowmap_quality` to a floor of
   `512` (`1024` with `hcde_k8vavoom_shadow_boost=1`). This composes existing
   shadow code instead of adding a new renderer path.

2. **Postprocess lighting preset.** When the profile is enabled, it enables
   `gl_bloom`, raises `gl_tonemap` from `0` to `1`, and raises `gl_ssao` from
   `0` to `1`. Existing user-selected nonzero tonemap/SSAO values are not
   lowered.

3. **Diagnostics.** `r_k8vavoom_status` prints both the last composed snapshot
   and the live CVAR values so operators can see whether later manual edits
   changed the preset output.

## Phase 2 Implemented (#38)

1. **Runtime backend probing.** `HCDE_ProbeK8vavoomBackendCapabilities()` queries
   the active `screen` framebuffer for hardware shadowmap SSBO support and Vulkan
   `VK_KHR_ray_query` extension availability instead of guessing from
   `vid_rendermode`.

2. **Ray-style light path.** When `hcde_k8vavoom_raylight_probe` or
   `hcde_k8vavoom_shadow_boost` is enabled on capable Vulkan hardware, the
   profile enables `vk_raytrace` before shader compilation so `main.fp` uses
   the existing `SUPPORTS_RAYTRACING` / `traceHit()` path for dynamic light
   shadow attenuation.

3. **Default-on preset.** `hcde_k8vavoom_auto_profile` (default `true`) applies
   the lighting profile automatically on first video init when shadowmaps are
   supported; Vulkan + ray-query hardware also enables shadow boost and raylight.
   Disabling the profile (`hcde_k8vavoom_lighting_profile 0`) turns off
   `hcde_k8vavoom_auto_profile` so it does not re-enable on the next launch.

4. **Init ordering.** `HCDE_K8vavoomPrepareBeforeInitializeState()` runs after the
   real framebuffer is created but before `InitializeState()` so `vk_raytrace`,
   Vulkan descriptor layouts, and shader defines all agree on ray-query support.

## Deferred / High-Risk Work

- Full hardware raytracing pipeline beyond ray-query shadow attenuation.
- Renderer-visible light changes driven by actor AI or gameplay logic. Rejected
  unless represented as existing dynamic-light definitions.
- Any map geometry acceleration structure that touches collision or movement.
  That belongs to #4 NanoBSP, not #17 rendering.

## Raylight / Raytrace Boundary (Phase 2)

Phase 2 wires the existing Vulkan ray-query path (`vk_raytrace` +
`main.fp` `traceHit()` / `SUPPORTS_RAYTRACING`) for dynamic light shadow
attenuation when `hcde_k8vavoom_raylight_probe` or
`hcde_k8vavoom_shadow_boost` is active on hardware that exposes
`VK_KHR_ray_query`. The probe runs in the hardware renderer only, reads
draw-side buffers, and never writes playsim state. `r_k8vavoom_status`
reports `raylight-path-active` and backend capability fields.

Still out of scope: gameplay-visible ray queries, AI sight/sound changes,
replacing BSP/node traversal, or requiring Vulkan raytracing for all backends.
Full hardware raytracing beyond ray-query attenuation remains deferred.

## Smoke / Tuning Log

### 2026-05-28

- Attempted local render launch:
  `build/RelWithDebInfo/hcde.exe -iwad doom2.wad +map map01 +hcde_k8vavoom_lighting_profile 1 +wait 5 +quit`.
- The process printed startup/version information and exited before a useful
  frame sample in this environment.
- Preset tuning was therefore made conservatively: the base profile now keeps
  `gl_shadowmap_quality` at the HCDE reset/default floor (`512`), while
  `hcde_k8vavoom_shadow_boost=1` raises the floor to `1024`. Bloom, tonemap,
  and SSAO remain opt-in through the profile and still do not change defaults.

## Done Criteria For #17 Phase 1

- CVAR scaffold lands and builds.
- Shadowmap and postprocess preset composition is implemented.
- `r_k8vavoom_status` reports composed and live renderer CVAR state.
- Roadmap explicitly lists the implemented Phase 1 behavior.
- Documentation states the determinism/multiplayer rules and reset semantics.
- No default visual change until a follow-up patch intentionally enables the
  preset.
