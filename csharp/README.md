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
| Pregame guest CLI | C++ `-join` guest path | `HCDE.PregameGuest.Cli` | Done (pregame + `--live-ticks`) |
| Engine core | `src/` | — | Not started |
| Dedicated server | `hcdeserv` target | — | Planned |
| Playsim | `src/playsim/` | — | Planned |
| Renderer (Vulkan/SW) | `src/rendering/` | — | Keep native or P/Invoke initially |
| ZScript VM | `src/common/scripting/` | — | Keep native or P/Invoke initially |
| Audio (ZMusic) | `libraries/ZMusic/` | — | Keep native via P/Invoke |
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

## Validation

Managed wire compatibility is gated by `dotnet test` (177 tests). Cross-language checks live under `validation/`:

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

Full native live gameplay stress remains under [`tests/netcode_step12/`](../tests/netcode_step12/). The xUnit suite also includes `NetcodeCrossLanguageTests`, which skips unless `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` are set.

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
    HCDE.PregameGuest.Cli/  hcde-pregame-guest CLI
  tests/
    HCDE.*.Tests/        xUnit regression tests (177 passing)
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

### Phase 3 — Full simulation

- Complete playsim, save/load, compatibility layers
- HCDE invasion, rewind, RCON server side in-engine

### Phase 4 — Client

- ZScript VM (likely native interop initially)
- Software renderer, then Vulkan via Silk.NET/Veldrid or retained C++ interop
- Audio via ZMusic P/Invoke
- Launcher UI (Avalonia or similar)

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
