# HCDE C# Migration — Phase 2 Principal Audit

**Last updated:** 2026-08-10  
**Status:** In progress — transport layer started (`HCDE.Net.Transport`).  
**Prerequisite:** [Phase 1 audit](HCDE_CSHARP_PHASE1_AUDIT.md) (complete)  
**Related:** [`docs/HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md) · [`docs/HCDE_NETCODE.md`](HCDE_NETCODE.md)

## What Phase 2 means

Phase 2 delivers a **headless dedicated server path in C#** that can:

1. Accept UDP connections and run the HCDE pregame reliable-service handshake
2. Serve launcher/server-info queries compatible with existing tooling
3. Eventually run authoritative netcode (`d_net*.cpp` server paths) and a minimal playsim tick loop

Phase 2 does **not** include rendering, audio, ZScript VM, or client prediction. The C++ `hcde` / `hcdeserv` binaries remain production until Phase 2 acceptance harnesses pass.

## Sub-phases (strict order)

| Sub-phase | C# project | C++ reference | Acceptance gate |
| --- | --- | --- | --- |
| **2a — Transport** | `HCDE.Net.Transport` | `i_net.cpp` (sockets, query, constants) | Unit + loopback integration tests |
| **2b — Pregame service** | `HCDE.Net.Pregame` (or transport subfolder) | `i_net.cpp` PRE_* / HCDE service queue | Guest/host handshake against C++ `hcdeserv` |
| **2c — Live netcode (server)** | `HCDE.Net.Core` | `d_net.cpp`, `d_net_snapshot_*.inl` | `tests/netcode_step12/` Python harness |
| **2d — Map + gamedata** | `HCDE.MapLoader`, `HCDE.Gamedata` | `maploader/`, `gamedata/` | MBF21 / ID24 validation harnesses |
| **2e — Playsim (server)** | `HCDE.Playsim` | `playsim/`, `p_tick.cpp` | Dedicated server loads map + runs ticks |
| **2f — Server shell** | `HCDE.Server` → `hcdeserv` | `d_main.cpp` dedicated path | End-to-end dedicated hosting |

**Current work:** Phase **2a** (this PR iteration).

## Boundaries (non-negotiable)

1. **Wire compatibility first.** Any C# packet codec must interoperate with the shipping C++ engine before C++ sources are removed.
2. **Single-threaded pump.** The C++ net layer owns `NetBuffer`, sockets, and pregame state on one thread (`i_net.cpp` contract). C# must mirror that until an explicit threading redesign is audited.
3. **No playsim in transport.** `HCDE.Net.Transport` must not reference actors, thinkers, WAD I/O, or renderer types.
4. **Authority-only server path.** Phase 2 targets `hcdeserv` / dedicated authority — not listen-host UI (`NetStartWindow`) or client prediction.
5. **Determinism deferred to 2e.** Transport and pregame may use managed memory; playsim port must document fixed-point / tick determinism requirements before merge.

## Phase 2a delivered (this iteration)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Net constants | `NetConstants.cs` | `i_net.h`, `i_net.cpp` |
| Pregame enums + service layout | `PregameConstants.cs`, `PregameServiceHeader.cs` | `i_net.cpp` lines 174–268 |
| HCDE connect block (`HCD3`) | `HcdeConnectInfo` | `HCDEConnectMagic` in `i_net.cpp` |
| UDP transport wrapper | `UdpTransport.cs` | `CreateUDPSocket`, `GetPacket`, `SendPacket` |
| Address parsing | `NetworkEndpoint.cs` | `TryBuildAddress` |
| Server query codec + client | `ServerQueryCodec.cs`, `ServerQueryClient.cs` | `I_QueryServerInfo`, `TryReadServerQuerySnapshot` |

## Verification matrix (Phase 2a)

| Check | Method | Result |
| --- | --- | --- |
| Default game port 5029 | `NetConstantsTests` | Pass |
| Pregame service header 15-byte layout | `PregameServiceHeaderTests` | Pass |
| HCD3 connect info round-trip | `HcdeConnectInfoTests` | Pass |
| Launcher challenge BE encoding | `ServerQueryCodecTests` | Pass |
| Server query response round-trip | `ServerQueryCodecTests` | Pass |
| Query client vs loopback UDP server | `ServerQueryIntegrationTests` | Pass |

## Not yet in Phase 2a

| Item | Target sub-phase |
| --- | --- |
| `SendPacket` / `GetPacket` DEM envelope | 2b |
| PRE_CONNECT / PRE_HCDE_SERVICE handling | 2b |
| Reliable service queue + retransmit | 2b |
| CRC over service payload | 2b |
| Live gameplay lanes (`HGP_*`, `HLANE_*`) | 2c |
| Map loader / playsim | 2d–2e |

## Principal risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| `i_net.cpp` is ~4.7k LOC with UI coupling | High | Port in layers; keep C++ as reference; cross-test each layer |
| `d_net.cpp` monolith (~20k LOC with `.inl`) | Extreme | Server-only path first; reuse Python stress harness |
| Struct layout / endian drift | High | Golden-vector tests per message type |
| Premature C++ deletion | High | Phase 2 audit gates removal per sub-phase |
| Playsim determinism | Extreme | Defer to 2e; document before implementation |

## Sign-off criteria for Phase 2 (full)

Phase 2 is complete when **all** hold:

- [ ] C# `hcdeserv` loads a map and runs authoritative ticks without renderer/audio
- [ ] `tests/netcode_step12/netcode_step12_stress.py` passes against C# server + C++ client (and vice versa where applicable)
- [ ] `tests/mbf21_validation/` and `tests/id24_validation/` pass on C# server path
- [ ] Principal audit updated with verification evidence
- [ ] C++ dedicated server remains available until one release cycle with C# server validated

## Phase 2b entry recommendation

Next implementation slice:

1. **Service payload CRC** (`m_crc32.h` equivalent or P/Invoke)
2. **`PregameServiceQueue`** — reliable send/ack/retry mirroring `FHCDEPendingService`
3. **PRE_CONNECT parser** — guest admission without full engine verification initially (mock verifier in tests)
4. **Cross-language soak** — C# guest against C++ `hcdeserv` pregame only

Do **not** start `d_net` snapshot lanes until 2b pregame handshake is green against C++.

## Source map

| Concern | C++ | C# (Phase 2) |
| --- | --- | --- |
| UDP sockets | `i_net.cpp` | `HCDE.Net.Transport/UdpTransport.cs` |
| Server info query | `I_QueryServerInfo` | `ServerQueryClient.cs` |
| Pregame constants | `i_net.cpp` | `PregameConstants.cs` |
| Service header | `HCDEServiceHeaderSize` etc. | `PregameServiceHeader.cs` |
| Pregame state machine | `FConnection`, `HostGame` | Not started (2b) |
| Live netcode | `d_net*.cpp` | Not started (2c) |
| RCON server | `d_net_rcon.cpp` | Phase 1 client only; server stays C++ until 2f |

## Audit conclusion (interim)

**Phase 2a transport foundation is landed** with tests. Pregame handshake and live netcode remain in C++. This audit will be updated at each sub-phase sign-off.

Next audit checkpoint: **Phase 2b principal audit** when pregame reliable service interoperates with C++ `hcdeserv`.
