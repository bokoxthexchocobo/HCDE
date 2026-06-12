<div align="center">

[<img src="branding/hcde-logo.svg" alt="HCDE logo" style="width: 100%; max-width: 1600px;" />](.)

</div>

# HCDE

HCDE is a Doom-engine project built on an UzDoom-derived core. It targets **server-authoritative multiplayer**, **mod compatibility** (MBF21, ID24, Eternity/EDGE surfaces), and **dedicated-server workflows** (`hcde`, `hcdeserv`, `hcdercon`).

- **Issue tracker:** [github.com/bokoxthexchocobo/HCDE/issues](https://github.com/bokoxthexchocobo/HCDE/issues)
- **Roadmap / Kanban:** [project board](https://github.com/users/bokoxthexchocobo/projects/2) · [`docs/HCDE_ROADMAP.md`](docs/HCDE_ROADMAP.md)

## Documentation (Wiki)

How-to guides and reference docs live on the [HCDE Wiki](https://github.com/bokoxthexchocobo/HCDE/wiki).

| | Page |
| --- | --- |
| **How-to** | |
| Getting started | [Getting Started](https://github.com/bokoxthexchocobo/HCDE/wiki/Getting-Started) |
| Build from source | [Building](https://github.com/bokoxthexchocobo/HCDE/wiki/Building) |
| Windows launcher updates | [Windows Updater](https://github.com/bokoxthexchocobo/HCDE/wiki/Windows-Updater) |
| **Reference** | |
| Dedicated launch & master list | [Launcher Protocol](https://github.com/bokoxthexchocobo/HCDE/wiki/Launcher-Protocol) |
| Engine netcode & diagnostics | [Network Protocol](https://github.com/bokoxthexchocobo/HCDE/wiki/Network-Protocol) |
| Console variables | [CVAR Reference](https://github.com/bokoxthexchocobo/HCDE/wiki/CVAR-Reference) |
| Rendering & k8vavoom lighting | [Rendering](https://github.com/bokoxthexchocobo/HCDE/wiki/Rendering) |

### In-repo contributor docs

These live beside the code and are the source of truth for architecture and audits:

| Doc | Topic |
| --- | --- |
| [`docs/HCDE_NETCODE.md`](docs/HCDE_NETCODE.md) | Netcode architecture, lead vs. ping, desync repair, `net_self_test` |
| [`docs/HCDE_INVASION.md`](docs/HCDE_INVASION.md) | Invasion operator guide (CVARs, state machine, tuning) |
| [`docs/HCDE_REWIND.md`](docs/HCDE_REWIND.md) | Rewind / lag-comp (`net_rewind_enable`, `sv_lagcomp`) |
| [`docs/HCDE_RCON.md`](docs/HCDE_RCON.md) | RCON transport and `hcdercon` usage |
| [`docs/HCDE_ROADMAP.md`](docs/HCDE_ROADMAP.md) | Kanban mirror + verified completion status |
| [`docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md`](docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md) | k8vavoom lighting profile design and Phase 2 ray-query path |
| [`tests/netcode_step12/README.md`](tests/netcode_step12/README.md) | Repeatable netcode stress harness |

## What ships in this repo

| Binary | Role |
| --- | --- |
| `hcde` | Client / game executable |
| `hcdeserv` | Dedicated server |
| `hcdercon` | Local RCON utility (`ping` / `status` today; admin commands planned) |

Master protocol constants live in `protocol/` so engine, launcher, and master stay separate (`protocol/hcde_master_protocol.json`, `protocol/hcde_master_protocol.h`).

## Quick build

**Windows (Visual Studio):**

```powershell
cmake -S C:\path\to\HCDE -B C:\path\to\HCDE\build -G "Visual Studio 17 2022" -A x64
cmake --build C:\path\to\HCDE\build --config Release
```

**Linux:**

```bash
cmake -S /path/to/HCDE -B /path/to/HCDE/build -DCMAKE_BUILD_TYPE=Release
cmake --build /path/to/HCDE/build -j
```

See [Building](https://github.com/bokoxthexchocobo/HCDE/wiki/Building) for requirements, output paths, and Windows runtime DLLs (`soft_oal.dll` for SFX, `sndfile.dll` for OGG/FLAC/WAV music — auto-staged via `cmake/StageSndFileRuntime.cmake`). [Getting Started](https://github.com/bokoxthexchocobo/HCDE/wiki/Getting-Started) covers hosting/joining a dedicated server and starting a single-player Invasion match.

**Netcode regression (optional):**

```bash
python tests/netcode_step12/netcode_step12_stress.py --dry-run
```

## Recent updates

- **k8vavoom lighting (Phase 2, #38):** runtime Vulkan/OpenGL capability probing, auto-profile on capable hardware (`hcde_k8vavoom_auto_profile`), and ray-query dynamic light shadows via `vk_raytrace` on Vulkan when `VK_KHR_ray_query` is present. Diagnostics: `r_k8vavoom_status` / `r_k8vavoom_reset`. See [Rendering wiki](https://github.com/bokoxthexchocobo/HCDE/wiki/Rendering) and [`docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md`](docs/HCDE_RENDERING_K8VAVOOM_AUDIT.md).
- **Netcode hardening:** late-join and rejoin handshake fixes, dedicated-server join setup no longer drops HCDE clients during pregame, co-op monster authority replication (#49), armor replication on dedicated clients (#51), and a crash fix when psprite desync logging fired on player death (`net_echo_debug`).
- **Renderer stack:** Vulkan is the default when supported, with automatic fallback to desktop OpenGL, then software rendering with the NanoBSP loader path (`hcde_nanobsp_loader`). The legacy OpenGL ES backend was removed.
- **Single-player startup:** a real "HCDE is loading..." window during ZDL command-line resolution, IWAD/mod scanning, compat patching, and archive mounting.
- **Invasion (`sv_gametype 4`):** starts cleanly from external launchers (`+set sv_gametype 4`); HCDE-styled wave announcements; operator guide in [`docs/HCDE_INVASION.md`](docs/HCDE_INVASION.md).
- **Diagnostics:** `hcde_lag_hud` and `hcde_hud_debug` are decoupled — the perf/lag overlay is opt-in (`hcde_lag_hud 1`).
- **Audio:** mod-nested `music/` folders prioritized so OGG-only mods play when `sndfile.dll` is staged.
- **Default-off experiments:** NanoBSP loader (`hcde_nanobsp_loader`) and Eternity spatial audio (`snd_backend eternity`, silent facade until the mixer is vendored) — board items [#4](https://github.com/bokoxthexchocobo/HCDE/issues/4) and [#3](https://github.com/bokoxthexchocobo/HCDE/issues/3).

## Project status

Feature and maintenance work is tracked on the [HCDE Kanban board](https://github.com/users/bokoxthexchocobo/projects/2). The full roadmap with a **verified, code-level completion status** for every item lives in [`docs/HCDE_ROADMAP.md`](docs/HCDE_ROADMAP.md). In short:

- **Complete and in use:** MBF21 compatibility, server-authoritative netcode foundation, core Invasion mode, k8vavoom-style lighting profile (shadowmaps + postprocess; Vulkan ray-query shadows when supported), smooth weapon bob + fullbright overrides, skin taunt sounds, actor-registry compaction hardening, and the Windows dedicated-server settings UI.
- **Opt-in / default-off and still in progress** (ship behind CVARs; not finished features yet): NanoBSP loader, Eternity spatial audio (silent facade), DSDA rewind / lag-comp, RCON (`ping`/`status` only), gyro input (Windows only), Nugget player-feel tweaks, Doom Retro compat tweaks, and Doomsday presentation features.
- **Backlog / scaffold:** Predator Economy mode and monster AI director (scaffolds, not playable), ID24 DEHEXTRA / extended-flag coverage.
- **Open maintenance bugs:** announcer playback ([#29](https://github.com/bokoxthexchocobo/HCDE/issues/29)), SP dmflags CVAR ([#30](https://github.com/bokoxthexchocobo/HCDE/issues/30)), Windows GL black screen ([#31](https://github.com/bokoxthexchocobo/HCDE/issues/31)), bot respawn ([#32](https://github.com/bokoxthexchocobo/HCDE/issues/32)).

See [`docs/HCDE_ROADMAP.md`](docs/HCDE_ROADMAP.md) for per-item detail and remaining work.

## Repository layout

| Path | Contents |
| --- | --- |
| `src/` | Engine, playsim, and networking (`d_net.*`, `i_net.cpp`, invasion, rewind, RCON) |
| `protocol/` | Master protocol schema |
| `tools/hcdemaster/` | Standalone master server source |
| `wadsrc*` | Game resources and compat packs |
| `wiki/` | Source for the GitHub Wiki |
| `docs/` | Architecture, operator guides, and audit notes |
| `tests/` | Validation harnesses (`netcode_step12`, invasion, ID24, etc.) |

## Licensing

HCDE is **GPL-3.0-or-later** ([`LICENSE`](LICENSE)). Branding and some asset trees have separate terms — see `branding/BRANDING-LICENSE.md` and license files under `wadsrc_bm`, `wadsrc_extra`, and `wadsrc_widepix`.

## Contributors

[`CONTRIBUTORS`](CONTRIBUTORS) — HCDE contributors / code sourced from

## Tech stack

CMake (≥ 3.16), C++20, Python 3; bundled libraries include ZMusic, ZVulkan, ZWidget, Abseil, WebP, LZMA, and others under `libraries/`. Per-component licenses: [`docs/licenses/`](docs/licenses/).
