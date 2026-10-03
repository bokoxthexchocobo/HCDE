# HCDE C# Migration

This directory contains the C# rewrite of HCDE. The legacy engine remains in C++ under `src/` while new work lands here incrementally.

## Why migrate?

HCDE currently mixes C++, C (build tools and vendored libraries), and ZScript. A C# codebase gives us:

- One primary language for engine services, tools, and multiplayer code
- Safer memory management for netcode and server workflows
- Easier testing with xUnit and modern tooling
- Cross-platform builds without maintaining parallel CMake/VS project files for new code

The full engine is ~640k lines of C++. This is a long-running migration, not a big-bang rewrite.

## Current status

Latest: [gameplay phases 1–3 status and audit](docs/HCDE_CSHARP_PHASE123_STATUS_AUDIT.md).
Roadmap: [master conversion plan, completed work and remaining gates](docs/HCDE_CSHARP_MASTER_PLAN.md).
Failed snapshot writes now preserve pending state; armor/pitch replicate;
buffered Doom weapon selection and flat-sector weapon noise are implemented.
The continuation converts fractional sector planes, classic ceilings/crushers
and Doom stair chains, with backward-compatible v6 pose archives including actor pitch.
**5,365 tests pass; gameplay phases 1–3 remain incomplete.** The audit gives the
remaining completion gates. Historical phase numbers below refer to narrower
tools/protocol/server milestones, not full gameplay conversion.

The dedicated server accepts `--limit-pain` to stop Pain Elemental spawning when
21 Lost Souls exist, including retained dead souls. It is disabled by default.

The dedicated server accepts `--hexen-crush-defaults` to select Hexen default
converted crush and stop behavior. This is an explicit rule setting, independent
of map format; automatic game-family detection and full Hexen gameplay remain
unimplemented. Programmatic hosts can set `DedicatedServerOptions.Compatibility`.

Previous: [command-buffer audit](docs/HCDE_CSHARP_INPUT_BUFFER_AUDIT.md).
Accepted command bursts now retain their order in a bounded per-player queue,
with one command executed per tic and retry-safe admission when full.
**923 tests pass.** Phases 1–3 and the larger release gates remain incomplete.

Previous: [phase 1–3 movers/damage audit](docs/HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md).
Loaded maps now support additional floors, lifts and crushing actions, with rider
and obstruction handling. Mover checksums and green-armor rounding are corrected.
**920 tests pass; phases 1–3 remain incomplete.**

Previous: [phase 2/3 continuation audit](docs/HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md).
Native monster defaults, pitch-aware combat, PK3 resources, Hexen record loading,
locked doors and extended-map network bootstrap now have regression coverage.
The suite has 908 passing tests. **Phases 2 and 3 remain incomplete**: the report
lists the remaining native actor actions, geometry/specials, mod-runtime and
multiplayer gates. Earlier pass summaries below are historical.

The phase-3 maps/mods foundation adds ordered `--file` PWAD loading, embedded
DEHACKED application, safer map validation, spawn filters and format-specific
line activation with player use. See the
[maps/mods and combined audit](docs/HCDE_CSHARP_MAPS_MODS_AUDIT.md) for 858 passing
tests and the remaining compatibility/release gates. Full maps/mods conversion
is not yet complete; PK3, complete special translation and native mod runtimes
still need work.

The combat/AI pass adds weapon cadence, shotgun pellets, traveling projectiles,
rocket splash and generic monster sight/chase/attack behavior. See the
[combat/AI and combined audit](docs/HCDE_CSHARP_COMBAT_AI_AUDIT.md) for 832 passing
Release tests and the remaining blockers. Full native combat parity is not yet
signed off; sustained projectile traffic still needs snapshot paging/retirement.

The new gameplay-foundation pass adds XYZ momentum, gravity, jumping, swept
flat-sector collision, timed actor states and shared damage/death handling.
The [combined foundation audit](docs/HCDE_CSHARP_GAMEPLAY_FOUNDATION_AUDIT.md)
also reviews the preceding combat, invasion and networking work, records fixes
and lists release blockers. This is a tested foundation subset, not full native
physics parity. Its phase numbering is separate from the historical audits below.

September 26 follow-up: combat now traces walls and closed openings, dead pawns
stop acting, and invasion waves wait for their enemies to be cleared. The guest
ignores stale snapshot tails and keeps invasion timers and enemy counts from the
same authority snapshot. See [combat and invasion follow-up](docs/HCDE_CSHARP_COMBAT_INVASION_FOLLOWUP.md)
for validation and remaining migration gaps. These are still engine subsets.

The dedicated server now enables the managed director with `--gamemode 4`, publishes
its state in live snapshots and server queries, and runs at 35 simulation tics per
second. Network attacks, enemy poses, finite-wave victory and shared per-tic invasion
payloads are covered by loopback tests. Example (from repository root):

```powershell
dotnet run --project csharp/src/HCDE.Server -- --iwad C:/Games/doom2.wad --gamemode 4 --invasion-waves 8 --invasion-countdown 30 --invasion-intermission 1
```

This command runs the C# subset; it does not launch a complete graphical client.

| Component | C++ location | C# project | Status |
| --- | --- | --- | --- |
| Master protocol constants | `protocol/hcde_master_protocol.h` | `HCDE.Protocol` | Done |
| Master list packets | `tools/hcdemaster/` | `HCDE.Protocol` + `HCDE.Master` | Done |
| RCON client | `tools/hcdercon/` | `HCDE.Protocol` + `HCDE.Rcon` | Done |
| NMS1 packet codec | `src/common/engine/sv_master_nms1.*` | `HCDE.Protocol` | Done |
| Phase 1 principal audit | — | `docs/HCDE_CSHARP_PHASE1_AUDIT.md` | Done |
| Full C# audit (all projects) | — | `docs/HCDE_CSHARP_FULL_AUDIT.md` | Done |
| UDP transport + server query | `common/engine/i_net.cpp` (subset) | `HCDE.Net.Transport` | Done (Phase 2a) |
| Pregame handshake | `i_net.cpp` PRE_* / HCDE services | `HCDE.Net.Pregame` | Done loopback (Phase 2b) |
| Live netcode wire codecs | `d_net*.cpp` | `HCDE.Net.Core` | In progress (Phase 2c wire + apply stubs) |
| Map loader | `maploader/`, `p_setup.cpp` | `HCDE.MapLoader` | In progress (binary and UDMF level build) |
| Pregame guest CLI | C++ `-join` guest path | `HCDE.PregameGuest.Cli` | Done (pregame + `--live-ticks`) |
| Engine core | `src/` | — | Not started |
| Dedicated server | `hcdeserv` (`HCDE.Server`) | Headless map boot, tick, snapshot publish, in-engine RCON | Phase 3 subset |
| Playsim | `src/playsim/` | `HCDE.Playsim` | Gameplay foundation subset: physics, states, damage, hitscan and melee |
| Renderer (Vulkan/SW) | `src/rendering/` | `HCDE.Client` | Phase 4 subset (column view + recorded quads) |
| ZScript VM | `src/common/scripting/vm/` | `HCDE.Scripting` | Phase 8 integer core. The client still has the Phase 4 stack stand-in |
| Audio (ZMusic) | `libraries/ZMusic/` | `HCDE.Client` | Phase 4 subset (PCM mix; ZMusic probed, not bound) |
| Build tools (re2c, lemon, zipdir) | `tools/` | — | Replace or wrap later |

## Build

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd csharp
dotnet build
dotnet test
```

Published binaries (same names as the C++ tools):

```bash
dotnet publish src/HCDE.Master/HCDE.Master.csproj -c Release -o ../bin/csharp
dotnet publish src/HCDE.Rcon/HCDE.Rcon.csproj -c Release -o ../bin/csharp
```

Outputs: `hcdemaster` and `hcdercon`.

## Documentation

| Doc | Topic |
| --- | --- |
| [`docs/HCDE_CSHARP_MIGRATION.md`](docs/HCDE_CSHARP_MIGRATION.md) | Engineering plan and phase breakdown |
| [`docs/HCDE_CSHARP_PHASE1_AUDIT.md`](docs/HCDE_CSHARP_PHASE1_AUDIT.md) | Phase 1 principal audit (tools + protocol) |
| [`docs/HCDE_CSHARP_PHASE2_AUDIT.md`](docs/HCDE_CSHARP_PHASE2_AUDIT.md) | Phase 2 principal audit (dedicated server path) |
| [`docs/HCDE_CSHARP_FULL_AUDIT.md`](docs/HCDE_CSHARP_FULL_AUDIT.md) | Full codebase audit (all projects) |
| [`docs/HCDE_CSHARP_PHASE3_AUDIT.md`](docs/HCDE_CSHARP_PHASE3_AUDIT.md) | Phase 3 principal audit (simulation subset) |
| [`docs/HCDE_CSHARP_PHASE4_AUDIT.md`](docs/HCDE_CSHARP_PHASE4_AUDIT.md) | Phase 4 principal audit (client subset) |
| [`docs/HCDE_CSHARP_PHASE5_AUDIT.md`](docs/HCDE_CSHARP_PHASE5_AUDIT.md) | Phase 5 principal audit (pickups and floors) |
| [`docs/HCDE_CSHARP_PHASE6_AUDIT.md`](docs/HCDE_CSHARP_PHASE6_AUDIT.md) | Phase 6 principal audit (firing) |
| [`docs/HCDE_CSHARP_PHASE7_AUDIT.md`](docs/HCDE_CSHARP_PHASE7_AUDIT.md) | Phase 7 principal audit (ACS core) |
| [`docs/HCDE_CSHARP_PHASE8_AUDIT.md`](docs/HCDE_CSHARP_PHASE8_AUDIT.md) | Phase 8 principal audit (ZScript integer core) |

## Validation

Managed regression checks run through `dotnet test` (832 tests; CI via `.github/workflows/csharp.yml`). Passing these does not prove full native runtime compatibility. Optional cross-language soak CI runs `FullyQualifiedName~CrossLanguageSoak` when `HCDE_HCDESERV_PATH` / `HCDE_IWAD_PATH` secrets are configured; both workflows enforce a Passed manifest gate when those secrets are present. Cross-language checks live under `validation/`:

| Harness | Purpose |
| --- | --- |
| [`validation/pregame/`](validation/pregame/) | C# pregame guest vs C++ `hcdeserv` smoke test |
| [`validation/netcode/`](validation/netcode/) | Wire codec validation notes and optional soak pointers |

Pregame cross-language soak (requires built `hcdeserv` and IWAD; skips gracefully when missing):

```bash
python3 csharp/validation/pregame/pregame_guest_smoke.py \
  --server /path/to/hcdeserv \
  --iwad /path/to/doom2.wad \
  --wad-crc <iwad-crc>
```

Full native live gameplay stress remains under [`tests/netcode_step12/`](../tests/netcode_step12/). The xUnit suite includes `NetcodeCrossLanguageTests`, `NetcodeCrossLanguageSoakTests`, `PregameCrossLanguageSoakTests`, `CrossLanguageSoakSuiteTests`, and `CrossLanguageSoakEvidenceArchiveTests`, which skip unless `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` are set. Set `HCDE_HCDE_CLIENT_PATH` to add a joining client during the Step 12 soak. Set `HCDE_SOAK_EVIDENCE_DIR` to write JSON audit evidence from soak runners (`CrossLanguageSoakSuite.RunAll()` or `CrossLanguageSoakEvidenceArchive.RecordDefaultEvidence()`). Skipped evidence templates live under [`validation/soak/evidence/`](validation/soak/evidence/).

## Solution layout

```
csharp/
  HCDE.sln
  docs/                  Migration plan and principal audits
  validation/
    pregame/             Cross-language pregame guest smoke harness
    netcode/             Managed wire codec validation notes
  src/
    HCDE.Protocol/       Shared protocol types and packet codecs
    HCDE.Master/         hcdemaster — UDP master server
    HCDE.Rcon/           hcdercon — TCP RCON client
    HCDE.Net.Transport/  UDP, CRC, server query, net constants
    HCDE.Net.Pregame/    Pregame host/guest handshake pumps
    HCDE.Net.Core/       Live protocol codecs (HLIV/HGPL/HCIN/HCSN/…)
    HCDE.MapLoader/      WAD directory + binary map lump decode (Phase 2d)
    HCDE.Playsim/        Authority tick subset (Phase 3)
    HCDE.Client/         hcde — headless client subset (Phase 4)
    HCDE.Scripting/      ZScript integer VM (Phase 8)
    HCDE.PregameGuest.Cli/  hcde-pregame-guest CLI
  tests/
    HCDE.*.Tests/        xUnit regression tests (776 passing)
```

## Migration phases

### Phase 1 — Tools and protocol (complete)

- Protocol constants and binary codecs (legacy + NMS1)
- `hcdemaster` and `hcdercon`
- Unit tests proving wire compatibility with the C++ implementations
- Principal audit: [`docs/HCDE_CSHARP_PHASE1_AUDIT.md`](docs/HCDE_CSHARP_PHASE1_AUDIT.md)

### Phase 2 — Dedicated server shell (in progress)

- `HCDE.Net.Transport` — UDP sockets, net constants, server query client (Phase 2a, done)
- `HCDE.Net.Pregame` — pregame host/guest handshake pumps (Phase 2b, done loopback)
- `HCDE.Net.Core` — live wire codecs and apply-session stubs (Phase 2c, in progress)
- Minimal playsim tick loop without rendering (Phase 2e, planned)
- Map loader and gamedata parsers (DEHACKED, MAPINFO, UDMF)
- Principal audit: [`docs/HCDE_CSHARP_PHASE2_AUDIT.md`](docs/HCDE_CSHARP_PHASE2_AUDIT.md)

### Phase 3 — Simulation subset (complete for the server gates)

- Line specials, a word-format ACS subset, bots, `HCSV` save/load
- MBF21 / ID24 / Eternity flags, invasion waves, authority rewind, in-engine RCON
- Principal audit: [`docs/HCDE_CSHARP_PHASE3_AUDIT.md`](docs/HCDE_CSHARP_PHASE3_AUDIT.md)

### Phase 4 — Client subset (complete for the headless gates)

- Managed ZScript stand-in, software column view, recorded hardware blit
- PCM mixer, launcher argument plan, `hcde --self-test`
- asmjit, Vulkan, and ZMusic are probed and not called
- Principal audit: [`docs/HCDE_CSHARP_PHASE4_AUDIT.md`](docs/HCDE_CSHARP_PHASE4_AUDIT.md)

### Phase 5 — Pickups and floors (complete for this slice)

- Pistol-start inventory and vanilla touch pickups
- Blockmap line tests, solid actors, and an 8-unit floor raise, lower, and lift
- Principal audit: [`docs/HCDE_CSHARP_PHASE5_AUDIT.md`](docs/HCDE_CSHARP_PHASE5_AUDIT.md)

### Phase 6 — Firing (complete for this slice)

- Pistol, shotgun, plasma, fist, and chainsaw with fixed damage
- Player armor absorbs a percent of the hit
- Principal audit: [`docs/HCDE_CSHARP_PHASE6_AUDIT.md`](docs/HCDE_CSHARP_PHASE6_AUDIT.md)

### Phase 7 — ACS core (started, largest source file)

- Arithmetic, compares, script and map variables, thing count, timer
- Principal audit: [`docs/HCDE_CSHARP_PHASE7_AUDIT.md`](docs/HCDE_CSHARP_PHASE7_AUDIT.md)
- The ZScript VM under `src/common/scripting` is the next largest component

### Phase 8 — ZScript integer core (started, largest subsystem)

- Register word format from `vmexec.cpp`: arithmetic, compares, jumps, integer return
- Principal audit: [`docs/HCDE_CSHARP_PHASE8_AUDIT.md`](docs/HCDE_CSHARP_PHASE8_AUDIT.md)
- Calls and memory loads are the next part of the same VM

### Native code to retain (initially)

These are poor candidates for a first-pass C# rewrite:

- `libraries/ZVulkan/` — Vulkan + glslang
- `libraries/ZMusic/` — many C audio backends
- `libraries/asmjit/` — ZScript JIT
- Texture scalers with hand-written ASM in `common/textures/`

## Coexistence with C++

During migration both trees build independently:

- **C++:** `cmake -S . -B build && cmake --build build` (unchanged)
- **C#:** `cd csharp && dotnet build`

The C# `HCDE.Protocol` types mirror `protocol/hcde_master_protocol.json`. When protocol constants change, update the JSON header and the C# `MasterProtocol` class together until we add code generation.

## Regression safety

Reuse existing Python harnesses under `tests/` for native netcode stress. C# cross-language and wire validation live under `validation/`. Add C# integration tests beside them as each subsystem ports.

See also: [`docs/HCDE_CSHARP_MIGRATION.md`](docs/HCDE_CSHARP_MIGRATION.md) for the detailed engineering plan.
