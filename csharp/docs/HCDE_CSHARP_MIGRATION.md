# HCDE C# migration plan

This document describes how HCDE moves from its UzDoom-derived C++/C codebase to a uniform C# engine. It complements [`README.md`](../README.md).

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
- Principal audit: [`HCDE_CSHARP_PHASE1_AUDIT.md`](HCDE_CSHARP_PHASE1_AUDIT.md)

**Follow-ups (non-blocking):**

- CI job running `dotnet test` on `csharp/`
- JSON → C#/C++ codegen for protocol constants
- Live `hcdercon` (C#) vs `hcdeserv` (C++) soak test

## Phase 2 — Dedicated server (in progress)

**Goal:** A headless `hcdeserv` in C# that loads a map, runs ticks, and serves HCDE netcode.

**Phase 2a delivered (transport):**

- `HCDE.Net.Transport` — UDP sockets, net/pregame constants, server query codec/client
- Principal audit: [`HCDE_CSHARP_PHASE2_AUDIT.md`](HCDE_CSHARP_PHASE2_AUDIT.md)

**Order (remaining):**

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

### Mini-phases (18)

Sub-phases 2c–2f are too large to finish in one step. Phase 2 is **18 mini-phases**. All 18 have a C# gate. MBF21 and ID24 harnesses currently launch the C++ client and a ZScript validator; their C# server gate is “the dedicated server loads the fixture and ticks.” That load-and-tick gate is covered in-process by mini-phase 18. The Python harnesses were not run. A ZScript `PASS` stays Phase 3–4. Phase 2 sign-off in [`HCDE_CSHARP_PHASE2_AUDIT.md`](HCDE_CSHARP_PHASE2_AUDIT.md) still requires those harnesses against a C# server and an IWAD. Audit for 7 and 12–18: [`HCDE_CSHARP_PHASE2_MINI_7_12_18_AUDIT.md`](HCDE_CSHARP_PHASE2_MINI_7_12_18_AUDIT.md).

| # | Mini-phase | Status | Gate |
| --- | --- | --- | --- |
| 1 | **2a** Transport: UDP, query, constants | Done | Loopback query tests |
| 2 | **2b** Pregame handshake | Done | Host/guest loopback through start-game |
| 3 | **2c-1** Live wire codecs and apply stubs | Done | xUnit round-trips for HLIV/HGPL/HCIN/HCSN tails |
| 4 | **2d-1** Binary map lumps | Done | THINGS through BEHAVIOR decode tests |
| 5 | **2f-1** Dedicated host scaffold | Done | Bind, query, advertise, pregame→live handoff |
| 6 | **2d-2** UDMF TEXTMAP | Done | Parse vertex, linedef, sidedef, sector, thing |
| 7 | **2d-3** Level build | Done | Sectors, lines, sides, vertices, things from binary or UDMF |
| 8 | **2d-4** MAPINFO | Done | Map name, next map, sky, cluster enough to boot a map |
| 9 | **2d-5** DEHACKED | Done | Thing, state, and sound patches for vanilla actors |
| 10 | **2e-1** Fixed point | Done | 16.16 numbers, angles, tic clock |
| 11 | **2e-2** Thinkers | Done | Thinker list and tick order |
| 12 | **2e-3** Actor spawn | Done | Map things become actors |
| 13 | **2e-4** Player | Done | Player pawn consumes usercmds |
| 14 | **2e-5** Collision | Done | Slide movement against lines |
| 15 | **2e-6** Authority tick | Done | `P_Ticker` subset updates checksums from simulated state |
| 16 | **2f-2** Headless map boot | Done | `hcdeserv` loads a map lump and ticks with no renderer |
| 17 | **2f-3** Sim snapshots | Done | Snapshot tails carry the ticked player position |
| 18 | **2-accept** In-process load and tick | Done | Minimal map loads, spawns, and ticks. Python harnesses not run |

## Phase 3 — Simulation completeness (server subset complete)

The bullets below are the headless-server gates, not a port of `p_acs.cpp` or `p_saveg.cpp`. Principal audit: [`HCDE_CSHARP_PHASE3_AUDIT.md`](HCDE_CSHARP_PHASE3_AUDIT.md).

| Gate | Status | What landed |
| --- | --- | --- |
| Line specials | Done | Door raise/open, exit, teleport, ACS execute. Special 1 repeats. Other one-shots clear. |
| ACS subset | Done | Word pcode: push, add/sub, delay, goto, drop, `Lspec1Direct`. Unknown opcodes stop the fiber. |
| Bots | Done | `BotPawn` walks forward. Invasion spawns them. |
| Save/load | Done | `HCSV` pose archive: tic, actors by id, sector floor/ceiling, exit flags. |
| Compat facades | Done | MBF21 special 270, ID24 secret exit 243, Eternity sector special 200. Flags, not full specs. |
| Invasion | Done | One wave at thing type 3004, or at (64, 64). Default off. |
| Rewind | Done | Authority keyframe ring using the save codec. No client resync. Default off. |
| In-engine RCON | Done | Loopback TCP, nonce auth, allowlist `ping` / `status` / `map`. `--rcon-password` on `hcdeserv`. |

## Phase 4 — Client (headless subset complete)

The rows below are the headless-client gates, not a port of `swrenderer`, the Vulkan backend, ZMusic, or the ZScript VM. Principal audit: [`HCDE_CSHARP_PHASE4_AUDIT.md`](HCDE_CSHARP_PHASE4_AUDIT.md).

| Gate | Status | What landed |
| --- | --- | --- |
| ZScript subset | Done | Managed opcodes: load, add, jump-if-zero, return, call-native. Unknown opcode or native stops the fiber. asmjit is probed and not called. |
| Software view | Done | One ray per column against blocking lines. Ceiling and floor fill the rest. Not `rendering/swrenderer`. |
| Hardware path | Done | Recorded quads, blitted on the CPU. Vulkan is probed and not bound. |
| Audio | Done | PCM sum with int16 clamp. Mute yields silence. ZMusic is probed and not called. |
| Launcher | Done | Validates the start selection and builds `hcde` arguments. `--self-test` draws the demo room. No window. |

## Phase 5 — Pickups and floors (slice complete)

The rows below are the first C#-only slice. They are not a port of `a_pickups.cpp` or `p_map.cpp`, and no C++ file was deleted. Principal audit: [`HCDE_CSHARP_PHASE5_AUDIT.md`](HCDE_CSHARP_PHASE5_AUDIT.md).

| Gate | Status | What landed |
| --- | --- | --- |
| Inventory | Done | Pistol start: 50 bullets, fist, and pistol. Armor, ammo, keys, and the other weapons. |
| Pickups | Done | Stimpack through BFG, by DoomEdNum. A gift that changes nothing stays on the map. |
| Blockmap | Done | A loaded `BLOCKMAP` limits line tests to the blocks the actor crosses. No blockmap still scans every line. |
| Actor blocking | Done | Overlapping solids are put back. Pickups, teleport destinations, and invasion spots do not block. |
| Floors | Done | Specials 18, 19, and 62 step 8 units. The lift waits 5 tics and returns. |

## Phase 6 — Firing (slice complete)

Fixed-damage attacks, not `p_pspr.cpp`. Principal audit: [`HCDE_CSHARP_PHASE6_AUDIT.md`](HCDE_CSHARP_PHASE6_AUDIT.md).

| Gate | Status | What landed |
| --- | --- | --- |
| Hitscan | Done | Pistol, chaingun, shotgun, plasma, and BFG. One ammo each. A miss still spends it. |
| Melee | Done | Fist and chainsaw, 64 units in front, no ammo. |
| Armor | Done | A player saves `damage * percent / 100` until the armor pool is empty. |

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
