# HCDE Roadmap

This is the single current roadmap for HCDE. It mirrors the public HCDE Kanban
project board and treats its items as the engine integration roadmap. The board
mixes feature work with bug-fix/maintenance work; both are tracked here.

- **Project board:** <https://github.com/users/bokoxthexchocobo/projects/2>
- **Issue tracker:** <https://github.com/bokoxthexchocobo/HCDE/issues>
- **Last reconciled with the board:** 2026-06-05

> This file replaces the former `HCDE_ROADMAP_INTEGRATION_PLAN.md`, whose long
> per-batch changelog had grown stale. Historical batch notes live in the git
> history; this document only carries the *current* state of each item. Detailed
> per-feature design and status notes live in the linked `docs/HCDE_*_AUDIT.md`
> files.

## Architecture target

HCDE is a compatibility-layer engine, not a single-source-port fork. The
intended shape is:

```text
UzDoom-derived middle core
  - playsim, renderer, scripting, mod-facing runtime, engine services

Compatibility importers
  - ZDoom/UzDoom formats and behavior
  - Eternity-style EDF/EMAPINFO
  - EDGE Classic-style DDF, Lua, and COAL
  - ID24 and MBF21 feature surfaces

Multiplayer and diagnostics
  - Odamex-style server authority and session behavior
  - HCDE-native command, snapshot, authority-event, and repair lanes
  - DSDA-inspired rewind, state hashing, and determinism tools

Feature/feel layers
  - selected rendering, physics, input, audio, BSP, AI, and gameplay systems
    imported as facades into the canonical HCDE runtime
```

The UzDoom-derived core remains the canonical runtime. Imported systems should
translate into that core rather than becoming parallel simulation engines.

## Current board snapshot

Statuses below match the live Kanban board. Each row links to its GitHub issue
and, where one exists, its detailed audit/design doc.

### Done

These items are implemented and their issues are closed. Most ship default-off
or presentation-only; "Done" means the agreed scope landed and is build-verified,
not that every future phase is finished (remaining phases, where any, are noted).

| Issue | Item | Notes |
| --- | --- | --- |
| [#1](https://github.com/bokoxthexchocobo/HCDE/issues/1) | Full MBF21 compliance | Supported compatibility surface. Future gameplay/importer changes must preserve it. |
| [#14](https://github.com/bokoxthexchocobo/HCDE/issues/14) | Odamex netcode baked in | Foundation for server-authoritative multiplayer. Harden/decouple rather than reintroduce lockstep. |
| [#2](https://github.com/bokoxthexchocobo/HCDE/issues/2) | ID24 | Map numbering, `id24.wad` autoload, intermission anim layers, Nightmare respawn, UMAPINFO numbering in tree. Generated smoke harness under `tests/id24_validation/`. See `HCDE_ID24_AUDIT.md`, `HCDE_ID24_COMPAT.md`. |
| [#3](https://github.com/bokoxthexchocobo/HCDE/issues/3) | Eternity spatial audio | `snd_backend=eternity` builds a real silent `SoundRenderer` facade with diagnostics (`snd_eternity_status` / `snd_eternity_probe`). Silent until the Eternity mixer is vendored. See `HCDE_FEATURE_IMPORTS.md`. |
| [#4](https://github.com/bokoxthexchocobo/HCDE/issues/4) | NanoBSP (from Woof) | `hcde_nanobsp_loader` + `r_nanobsp_status`; NanoBSP partitioning ported onto HCDE arrays. Polyobject/invalid inputs fall back. See `HCDE_NANOBSP_AUDIT.md`. |
| [#5](https://github.com/bokoxthexchocobo/HCDE/issues/5) | DSDA-Doom rewind system | Phases 1-5 in `src/d_net_rewind.{cpp,h}`: ring-buffer keyframes, restore primitives, lag-comp bracket, server-side hit replay. Default off (`net_rewind_enable 0`, `sv_lagcomp 0`). See `HCDE_REWIND.md`. |
| [#15](https://github.com/bokoxthexchocobo/HCDE/issues/15) | Finish Invasion mode | Boss-wave diagnostics, spawn fallback, RNG/replay/capability decisions, late-join replay probe, sim-LOD observability, announcement coverage. Remaining work is soak/content tuning, not core. See `HCDE_INVASION.md`, `HCDE_INVASION_AUDIT.md`. |
| [#17](https://github.com/bokoxthexchocobo/HCDE/issues/17) | k8vavoom-style rendering | Phase 1 preset: `hcde_k8vavoom_lighting_profile` composes shadowmap + bloom/tonemap/SSAO; `r_k8vavoom_status` / `r_k8vavoom_reset`. Default off, presentation-only. See `HCDE_RENDERING_K8VAVOOM_AUDIT.md`. |
| [#7](https://github.com/bokoxthexchocobo/HCDE/issues/7) | Nugget Doom — gyroscope input | `HCDEGyro_GetTiccmdContribution()` wired into `G_BuildTiccmd`; Windows SDL2 sensor probe. Default off. Held/toggle binding still pending. |
| [#9](https://github.com/bokoxthexchocobo/HCDE/issues/9) | Nugget Doom — player feel & input | `m_smooth_curve`, `r_crosshair_recoil`, `r_killfeed`, `snd_footsteps_surface`. Presentation/input only — no physics, hitscan, or weapon timing. See `HCDE_NUGGET_FEEL_AUDIT.md`. |
| [#8](https://github.com/bokoxthexchocobo/HCDE/issues/8) | Doom Retro — physics & feel | Highest-risk port; every tweak behind a compat flag with demo-version awareness, one tweak per PR. First candidates landed (pain view-kick smoothing, `compat_dr_crusher`). See `HCDE_DOOM_RETRO_AUDIT.md`, `HCDE_COMPATF2_DR_RESERVATIONS.md`. |
| [#10](https://github.com/bokoxthexchocobo/HCDE/issues/10) | Crispy Doom — variable framerate | Audited as presentation-only; existing interpolation already covers it. No engine import needed. See `HCDE_CRISPY_VFR_AUDIT.md`. |
| [#11](https://github.com/bokoxthexchocobo/HCDE/issues/11) | International Doom — two features | `r_weapon_bob_smooth` (software + hardware paths) and `r_fullbright_overrides` (GLDEFS brightmap-aware fullbright). See `HCDE_FEATURE_IMPORTS.md`. |
| [#13](https://github.com/bokoxthexchocobo/HCDE/issues/13) | Monster/enemy AI system | Director scaffold landed in `src/d_net_aidirector.{cpp,h}` (`sv_aidirector_enable` default off, `ai_status`). Real tactical behaviour is ZScript work on top of this; no actor mutation or replication today. See `HCDE_AIDIRECTOR_AUDIT.md`. |
| [#21](https://github.com/bokoxthexchocobo/HCDE/issues/21) | Doom skins taunt sound | `taunt [variant]` emits `DEM_TAUNT`; plays `*taunt[-name]` on the originating pawn at `CHAN_VOICE`. Cosmetic-only opcode on the existing allowlist. |
| [#22](https://github.com/bokoxthexchocobo/HCDE/issues/22) | Net_CompactHCDEReplicatedActors guard pass | Distinguishes Id==0 defects, retired-expired drops, and live-remote baselines; rebuild detects duplicate ids. See `src/d_net_snapshot_part1.cpp`. |
| [#23](https://github.com/bokoxthexchocobo/HCDE/issues/23) | hcdeserv UI: auto-apply | Launcher UI cleanup tracked as resolved on the board. Launcher source lives outside `src/`. |
| [#24](https://github.com/bokoxthexchocobo/HCDE/issues/24) | RCON utility | Transport state machine + ingress-drain in `src/d_net_rcon.{cpp,h}`; `hcdercon` client ships. See `HCDE_RCON.md`. |
| [#18](https://github.com/bokoxthexchocobo/HCDE/issues/18) | Check over the updater | Audited as a changelog viewer (no auto-download/install). Native-fetch abstraction + libcurl skeleton landed. See `HCDE_UPDATER_AUDIT.md`. |
| [#19](https://github.com/bokoxthexchocobo/HCDE/issues/19) | Console bug — getting stuck | Bug-fix item closed; keep covered by startup/console smoke checks. |

### In progress

| Issue | Item | Notes |
| --- | --- | --- |
| [#6](https://github.com/bokoxthexchocobo/HCDE/issues/6) | Doomsday Engine — three things | FakeRadio lighting, geometry-based AO, and per-sector reverb as presentation-side opt-ins. Phases 0-3 wired (`r_fakeradio`, `r_geom_ao`, `snd_env_reverb`, `r_doomsday_status`); reverb on OpenAL EFX. Phase 4 soak remaining. See `HCDE_DOOMSDAY_AUDIT.md`. |

### Backlog — features

| Issue | Item | Notes |
| --- | --- | --- |
| [#12](https://github.com/bokoxthexchocobo/HCDE/issues/12) | Predator Economy game mode | Phase 1 scaffold + snapshot V1 contract in `src/d_net_predator.{cpp,h}` (round director, CVARs, `predator_status`). Buy opcode, per-player currency, and role gameplay pending. See `HCDE_PREDATOR_AUDIT.md`. |

### Backlog — bug fixes and maintenance

| Issue | Item | Notes |
| --- | --- | --- |
| [#29](https://github.com/bokoxthexchocobo/HCDE/issues/29) | Fix bokoannouncer | Announcer playback bug. |
| [#30](https://github.com/bokoxthexchocobo/HCDE/issues/30) | CVAR for SP dmflags usage in campaign | Add a CVAR to control how single-player campaign honors dmflags. |
| [#31](https://github.com/bokoxthexchocobo/HCDE/issues/31) | Windows desktop OpenGL startup black screen | Desktop GL auto-routes to GL ES at startup; tracking residual cases. |
| [#32](https://github.com/bokoxthexchocobo/HCDE/issues/32) | Bots don't respawn | Bots should respawn and currently do not. |

## Roadmap grouping by subsystem

### Networking and authority

- Preserve the Odamex-style authority foundation from #14.
- Keep Invasion (#15) as the multiplayer stress test for monsters, projectiles,
  actor deltas, late join, and server load.
- Keep actor registry compaction (#22) hardened before broadening replicated
  actor categories.
- Treat RCON (#24) as admin tooling, not a gameplay packet extension.

### Rewind, observability, and determinism

- Treat DSDA-inspired rewind (#5) as history/state tooling.
- Use rewind/state comparison to debug divergence in Invasion, AI, physics, and
  imported compatibility behavior.

### Compatibility importers

- Preserve MBF21 compliance (#1).
- Keep ID24 (#2) as a compatibility surface.
- Treat Eternity EDF/EMAPINFO, EDGE Classic DDF/Lua/COAL, and other imported
  definitions as facades into the UzDoom-derived core. Avoid parallel runtime
  semantics unless there is a deliberate adapter mapping them into HCDE's
  command/event/snapshot model.

### Rendering, timing, and presentation

- Keep k8vavoom-style rendering (#17) and the Doomsday presentation features (#6)
  renderer-/audio-facing and default-off.
- Evaluate NanoBSP (#4) as infrastructure with strict boundaries around map
  collision and gameplay state.
- Keep Crispy-style variable framerate (#10) presentation-side; the fixed-tic
  simulation stays authoritative.

### Input, player feel, and physics

- Route Nugget player-feel/input (#9) and gyroscope input (#7) through normal
  command construction.
- Treat Doom Retro physics/feel (#8) as multiplayer-sensitive: gameplay physics
  changes need server-authoritative validation and netgame gating.

### Gameplay systems

- Predator Economy mode (#12), monster/enemy AI (#13), and skin taunt sounds
  (#21) use authority events, replicated mode state, and cosmetic/event
  separation as appropriate.

### Maintenance and release tooling

- Keep updater review (#18), console bug regressions (#19), the announcer fix
  (#29), SP dmflags CVAR (#30), Windows GL startup (#31), and bot respawn (#32)
  tracked as maintenance/tooling work, not feature-port experiments.

## Implementation rules

- Board items are roadmap inputs. Do not drop them from planning just because
  they come from different Doom-family ports.
- Classify each item before implementation: compatibility importer, engine
  service, presentation layer, command input, gameplay logic, networking,
  diagnostics, or maintenance.
- Anything that changes gameplay state must pass through the canonical HCDE
  runtime and respect server authority.
- Presentation improvements must not change fixed-tic playsim semantics.
- Imported data/script systems should translate into existing runtime concepts
  before adding new parallel state.
- Bug-fix cards should be handled with focused verification and should not be
  bundled with large feature-port refactors unless they are direct prerequisites.

## Maintaining this document

When the project board changes, update the **Current board snapshot** tables and
the **Last reconciled with the board** date above. Keep per-feature detail in the
`docs/HCDE_*_AUDIT.md` files; this roadmap should stay a concise, current
overview rather than a running changelog.
