# Rendering

HCDE ships Vulkan and desktop OpenGL hardware renderers plus a software
fallback. Presentation features (shadows, postprocess, k8vavoom-style
lighting) never change playsim state, RNG, saves, or net snapshots.

## Backends and fallback chain

At startup HCDE tries **Vulkan → desktop OpenGL → software** (with the
NanoBSP loader path when software rendering is active). The legacy OpenGL
ES backend was removed; stale `vid_preferbackend 2` configs migrate to
Vulkan automatically.

| Backend | Selector | Notes |
| --- | --- | --- |
| Vulkan | `vid_preferbackend 1` (default) | Primary path; supports hardware shadowmaps and optional `VK_KHR_ray_query` for ray-style dynamic light shadows. |
| Desktop OpenGL | `vid_preferbackend 0` | Used when Vulkan init fails or when explicitly requested. Shadowmaps when the driver exposes the required SSBO path. |
| Software | `vid_rendermode 1` after hardware fallback | Compatibility fallback; k8vavoom presets target hardware only. |

Use `vid_preferbackend` in `hcde.ini` or the video menu. After changing
backends, restart or reload video so shader pipelines recompile.

```text
r_hcde_renderer_status    # vid_preferbackend, vid_rendermode, active backend
```

## k8vavoom-style lighting (#17 / #38)

The k8vavoom profile composes existing HCDE renderer knobs (shadowmaps,
bloom, tonemap, SSAO) and, on capable Vulkan hardware, enables
`vk_raytrace` so `main.fp` uses ray-query shadow attenuation for dynamic
lights.

Implementation lives in
`src/common/rendering/hwrenderer/data/hw_k8vavoom_lighting.{cpp,h}`.
Design notes: `docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md`.

### CVARs

| CVAR | Default | Purpose |
| --- | --- | --- |
| `hcde_k8vavoom_auto_profile` | `true` | Apply the lighting profile automatically on first video init when shadowmaps are supported. Vulkan + ray-query also enables shadow boost and raylight. |
| `hcde_k8vavoom_lighting_profile` | `0` | Master preset (`0` = off, `1` = lighting-heavy). Setting to `0` disables `hcde_k8vavoom_auto_profile` so it does not re-enable on the next launch. |
| `hcde_k8vavoom_shadow_boost` | `false` | Raise shadowmap quality floor (`1024` vs `512`) and participate in raylight path selection. |
| `hcde_k8vavoom_raylight_probe` | `false` | Opt into Vulkan ray-query dynamic light shadows when `VK_KHR_ray_query` is available. |
| `vk_raytrace` | `false` | Vulkan ray-query acceleration structures; the profile may enable this before shader init on capable hardware. |

### Diagnostics

```text
r_k8vavoom_status    # composed preset, live CVARs, backend capability probe
r_k8vavoom_reset     # disable profile/sub-flags and restore HCDE renderer defaults
```

On Vulkan with ray-query support, expect `raylight-path-active=yes` when
shadow boost or raylight probe is on. Reset with `r_k8vavoom_reset` if you
want the legacy HCDE look.

### Quick try

```powershell
hcde -iwad C:\Games\doom2.wad +map MAP01
```

Then in the console:

```text
r_k8vavoom_status
```

If auto-profile is on and your GPU supports shadowmaps, the profile should
already be enabled. Force manually:

```text
hcde_k8vavoom_lighting_profile 1
hcde_k8vavoom_shadow_boost 1
```

## Related shadow CVARs

The profile composes these existing knobs (see [CVAR Reference](CVAR-Reference)):

- `gl_light_shadowmap`, `gl_shadowmap_quality`, `gl_shadowmap_maxlights`, `gl_shadowmap_prioritize`
- `hcde_shadow_autobudget`, `hcde_shadow_autofallback`
- `gl_bloom`, `gl_tonemap`, `gl_ssao`

Grouped presets also exist via `hcde_shadowprofile` in the options menu.

## Maintenance

- **Windows desktop GL black screen ([#31](https://github.com/bokoxthexchocobo/HCDE/issues/31))** — without the old GLES workaround, some desktop GL drivers may still fail at startup; try Vulkan (`vid_preferbackend 1`) or software fallback.
- **Determinism** — two clients may use different visual settings while receiving the same authoritative snapshots.
