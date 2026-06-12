# HCDE Wiki

How-to guides and reference documentation for HCDE.

## How-to

- [Getting Started](Getting-Started) — build, host, and join a dedicated server
- [Building](Building) — compile from source (Windows and Linux)
- [Windows Updater](Windows-Updater) — launcher update flow, backups, and recovery

## Reference

- [Launcher Protocol](Launcher-Protocol) — dedicated launch, master list (legacy + NMS1), per-server query
- [Network Protocol](Network-Protocol) — engine session lanes, net diagnostics, debug traces
- [Subsystems](Subsystems) — rewind, RCON, Invasion, Predator, AI Director, ID24/NanoBSP imports, killfeed/gyro/taunts
- [Rendering](Rendering) — Vulkan/OpenGL backends, k8vavoom lighting profile, shadow diagnostics
- [CVAR Reference](CVAR-Reference) — console variables and defaults

## Recent updates (June 2026)

- **Renderer fallback chain** — OpenGL ES removed; startup tries Vulkan, then
  desktop OpenGL, then software (NanoBSP path). Legacy `vid_preferbackend 2`
  migrates to Vulkan. Use `r_hcde_renderer_status` to inspect the active path.
- **k8vavoom lighting Phase 2 (#38)** — runtime backend capability probing,
  auto-profile on capable hardware (`hcde_k8vavoom_auto_profile`), and Vulkan
  ray-query dynamic light shadows when `VK_KHR_ray_query` is available. Use
  `r_k8vavoom_status` to inspect the active preset; `r_k8vavoom_reset` to
  return to HCDE defaults. See [Rendering](Rendering) and
  `docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md`.

## Earlier updates (May 2026)

- **Single-player Invasion** now starts cleanly on local solo launches.
  Doom Connector and other launchers can pass `+sv_gametype 4`,
  `+set sv_gametype 4`, or even combine that with `-coop`; HCDE preserves
  the explicit invasion gametype instead of resetting it to coop.
- **Crash on first invasion tick** (`Net_DestroyTrackedInvasionMonsters
  -> DObject::Destroy`) was caused by the participant counter going to
  zero during local startup. The console player is now treated as a
  participant in `!netgame` so the wave director cannot reset itself
  before a wave even begins.
- **Top-of-screen "perf mirror / HCDE lag" strip** is no longer forced
  on by `hcde_hud_debug` (a logging gate). Toggle it at runtime with
  `hcde_lag_hud 1` (overlay) or `stat hcde_lag` (panel).
- **Music in mods that use OGG (e.g. `D2Re.pk3`)** now resolves nested
  `music/` folders correctly, and Windows developer builds auto-stage
  `sndfile.dll` so OGG/FLAC/WAV decoding works out of the box. See the
  build instructions on [Building](Building).
- **OpenAL runtime** (`soft_oal.dll`) is now an explicit runtime
  dependency for SFX. It ships with HCDE releases; if you build from
  source on Windows, drop OpenAL Soft beside `hcde.exe` to get
  positional sound effects.
- **Single-player death/respawn** no longer freezes when the autosave
  path fails; pressing fire/use during the death animation is buffered
  so a respawn fires as soon as the death delay expires.
