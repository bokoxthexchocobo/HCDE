# HCDE C# migration plan

This document describes how HCDE moves from its UzDoom-derived C++/C codebase to a uniform C# engine. It complements [`csharp/README.md`](../csharp/README.md).

## Scope reality check

| Area | Approx. size | Migration difficulty |
| --- | --- | --- |
| `src/` engine | ~640k LOC, 612 `.cpp` files | Extreme |
| `libraries/` vendored deps | ~1,800 files | Mostly keep native |
| `tools/` standalone utilities | Small | Easy — started |
| `protocol/` shared schema | Tiny | Done in C# |
| `wadsrc/` ZScript assets | 305 `.zs` files | Keep as data; reimplement VM/runtime |

A full client rewrite is not practical as a single effort. The recommended path is **server-first**: prove dedicated-server netcode and playsim in C#, then expand toward the client while keeping rendering and audio native until replacements exist.

## Architecture target

```text
csharp/
  HCDE.Protocol          Wire formats, shared constants
  HCDE.Master            hcdemaster (done)
  HCDE.Rcon              hcdercon client (done)
  HCDE.Net               UDP session + DEM streams (planned)
  HCDE.Playsim           Actor simulation (planned)
  HCDE.MapLoader         UDMF, nodes, slopes (planned)
  HCDE.Gamedata          DEHACKED, MAPINFO parsers (planned)
  HCDE.Server            hcdeserv executable (planned)
  HCDE.Client            hcde executable (long-term)
  HCDE.Native            P/Invoke shims for ZMusic, Vulkan, asmjit
```

The UzDoom-derived C++ tree remains buildable until each subsystem has a tested C# replacement.

## Phase 1 — Tools and protocol (complete)

**Goal:** Establish the C# solution, prove wire compatibility, replace the smallest standalone binaries.

**Delivered:**

- `HCDE.Protocol` — mirrors `protocol/hcde_master_protocol.h` (legacy + NMS1 codecs)
- `HCDE.Master` — `hcdemaster` UDP master server
- `HCDE.Rcon` — `hcdercon` TCP RCON client
- xUnit tests for packet codecs, NMS1 golden vectors, RCON loopback integration, and master server round-trip
- Principal audit: [`docs/HCDE_CSHARP_PHASE1_AUDIT.md`](HCDE_CSHARP_PHASE1_AUDIT.md)

**Follow-ups (non-blocking):**

- CI job running `dotnet test` on `csharp/`
- JSON → C#/C++ codegen for protocol constants
- Live `hcdercon` (C#) vs `hcdeserv` (C++) soak test

## Phase 2 — Dedicated server

**Goal:** A headless `hcdeserv` in C# that loads a map, runs ticks, and serves HCDE netcode.

**Order:**

1. `HCDE.Net.Transport` — port `common/engine/i_net.cpp` UDP primitives
2. `HCDE.Net.Core` — port `d_net.cpp` server paths (snapshots, commands, late join)
3. `HCDE.MapLoader` — `maploader/`, `p_setup.cpp`
4. `HCDE.Gamedata` — DEHACKED, MAPINFO, UDMF
5. `HCDE.Playsim` (server subset) — `p_tick`, `p_mobj`, `p_map`, thinkers
6. `HCDE.Server` — game loop without rendering/audio

**Acceptance:** Existing Python harnesses pass:

- `tests/netcode_step12/`
- `tests/mbf21_validation/`
- `tests/id24_validation/`

## Phase 3 — Simulation completeness

- Full playsim including `p_acs.cpp`, specials, bots
- Save/load (`p_saveg.cpp`)
- Compatibility facades (`hcde_mod_compat`, Eternity/MBF21/ID24 surfaces)
- Invasion, rewind, in-engine RCON server

## Phase 4 — Client

- ZScript VM — start with P/Invoke to existing bytecode/JIT; rewrite compiler later if needed
- Software renderer (`rendering/swrenderer/`)
- Hardware renderer — Vulkan via native interop or Silk.NET
- Audio — ZMusic P/Invoke
- Launcher/widgets — Avalonia or ImGui.NET

## What stays C/C++ (for now)

| Component | Reason |
| --- | --- |
| ZVulkan + glslang | Mature GPU pipeline; high rewrite cost |
| ZMusic + backends | Many C synthesizer implementations |
| asmjit | ZScript JIT on x86_64 |
| HQnx/xBR texture scalers | Hand-tuned ASM |
| re2c/lemon generated parsers | Regenerate or port grammars to C# later |

## C and old C++ in the tree

**C sources (not engine core):**

- `tools/re2c/` — lexer generator (build-time)
- `tools/lemon/` — parser generator (build-time)
- `tools/zipdir/zipdir.c` — PK3 packer (build-time)
- Vendored libs under `libraries/` (timidity, fluidsynth, etc.)

**Strategy:**

- Build tools: replace with `dotnet` CLI tools or NuGet packages where equivalents exist
- Vendored libs: keep as native DLLs behind `HCDE.Native` P/Invoke
- Engine C++: port module by module into `csharp/src/`

## Protocol sync

`protocol/hcde_master_protocol.json` is the neutral schema. Today both C++ (`hcde_master_protocol.h`) and C# (`MasterProtocol.cs`) are hand-maintained. Future work: generate both from JSON to prevent drift.

## Testing strategy

1. **Unit tests** — packet codecs, parsers, hash functions (xUnit in `csharp/tests/`)
2. **Integration tests** — C# server vs. existing C++ client (and vice versa)
3. **Python harnesses** — keep `tests/*_validation/` as end-to-end regression
4. **Determinism checks** — state hashing for playsim once ported

## Build integration (future)

Options to wire C# into the main build:

- CMake `add_custom_target` invoking `dotnet publish`
- GitHub Actions matrix job for `csharp/`
- Eventually replace `HCDE.sln` (VS C++) with `csharp/HCDE.sln` for application code

For now, C# builds independently to avoid disrupting the existing CMake pipeline.

## Contributing

When porting a C++ module:

1. Read the existing module and its audit doc under `docs/`
2. Add a C# project or folder under `csharp/src/`
3. Port behavior, not line-by-line structure — use idiomatic C#
4. Add tests that compare output against the C++ implementation
5. Update the status table in `csharp/README.md`

Do not delete C++ sources until the C# replacement passes regression tests and ships in a release.
