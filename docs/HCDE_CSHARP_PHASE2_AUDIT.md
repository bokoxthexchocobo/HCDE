# HCDE C# Migration — Phase 2 Principal Audit

**Last updated:** 2026-08-11  
**Status:** In progress — Phase **2b** verification errors, start-game, and cross-language harness landed.  
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
| **2b — Pregame service** | `HCDE.Net.Pregame` | `i_net.cpp` PRE_* / HCDE service queue | Guest/host handshake against C++ `hcdeserv` |
| **2c — Live netcode (server)** | `HCDE.Net.Core` | `d_net.cpp`, `d_net_snapshot_*.inl` | `tests/netcode_step12/` Python harness |
| **2d — Map + gamedata** | `HCDE.MapLoader`, `HCDE.Gamedata` | `maploader/`, `gamedata/` | MBF21 / ID24 validation harnesses |
| **2e — Playsim (server)** | `HCDE.Playsim` | `playsim/`, `p_tick.cpp` | Dedicated server loads map + runs ticks |
| **2f — Server shell** | `HCDE.Server` → `hcdeserv` | `d_main.cpp` dedicated path | End-to-end dedicated hosting |

**Current work:** Phase **2b** — C# loopback handshake through WAITING setup (map/game/roster) complete; C++ `hcdeserv` cross-test remains the 2b gate.

## Boundaries (non-negotiable)

1. **Wire compatibility first.** Any C# packet codec must interoperate with the shipping C++ engine before C++ sources are removed.
2. **Single-threaded pump.** The C++ net layer owns `NetBuffer`, sockets, and pregame state on one thread (`i_net.cpp` contract). C# must mirror that until an explicit threading redesign is audited.
3. **No playsim in transport.** `HCDE.Net.Transport` must not reference actors, thinkers, WAD I/O, or renderer types.
4. **Authority-only server path.** Phase 2 targets `hcdeserv` / dedicated authority — not listen-host UI (`NetStartWindow`) or client prediction.
5. **Determinism deferred to 2e.** Transport and pregame may use managed memory; playsim port must document fixed-point / tick determinism requirements before merge.

## Phase 2a delivered

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Net constants | `NetConstants.cs` | `i_net.h`, `i_net.cpp` |
| Pregame enums + offsets | `PregameConstants.cs` | `i_net.cpp` lines 174–289 |
| HCDE connect block (`HCD3`) | `HcdeConnectInfo.cs` | `HCDEConnectMagic` in `i_net.cpp` |
| UDP transport wrapper | `UdpTransport.cs` | `CreateUDPSocket`, `GetPacket`, `SendPacket` |
| Address parsing | `NetworkEndpoint.cs` | `TryBuildAddress` |
| Server query codec + client | `ServerQueryCodec.cs`, `ServerQueryClient.cs` | `I_QueryServerInfo`, `TryReadServerQuerySnapshot` |

## Phase 2b delivered

### Codecs and wire (iteration 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| zlib CRC-32 | `Crc32.cs` | `m_crc32.h` `CalcCRC32` / `AddCRC32` |
| Setup wire envelope | `SetupPacketCodec.cs` | `SendPacket` / `GetPacket` CRC prefix |
| HCDE service packet | `HcdeServicePacket.cs` | `BeginHCDEPregameService`, 15-byte `NetBuffer` header |
| Service receive validation | `PregameServiceReceiver.cs` | `CheckHCDEPregameService` |
| Reliable service queue | `ReliableServiceQueue.cs`, `PregameServiceSender.cs` | `FHCDEPendingService`, `FlushHCDEReliableServices` |
| Connect admission ACK | `ConnectAckCodec.cs` | `PRE_CONNECT_ACK` / `DriveRuntimeSetupStateForClient` |
| Connection state | `PregameConnectionState.cs` | `FConnection` seq/ack fields |

### Host/guest pump (iteration 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| PRE_CONNECT codec | `ConnectPacketCodec.cs` | `TryProcessSetupConnectPacket` |
| Engine info wire layout | `EngineInfoCodec.cs` | `Net_SetEngineInfo` / `Net_VerifyEngine` |
| Session token minting | `SessionToken.cs` | `MakeSessionToken` |
| UDP send/receive helper | `PregameWire.cs` | `SendPacket` / `GetPacket` glue |
| Host admission pump | `PregameHost.cs` | `TryProcessSetupConnectPacket`, `DriveRuntimeSetupStateForClient` |
| Guest join pump | `PregameGuest.cs` | `JoinGame` guest setup loop |
| Wire compression | `SetupPacketCodec.EncodeCompressed` / decode path | `NCMD_COMPRESSED` in `SendPacket` / `GetPacket` |

### WAITING setup + verification (iteration 3 — this PR)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Protocol stream helpers | `ProtocolStreamCodec.cs` | `WriteString`, `WriteInt32`, `WriteInt8` in `i_protocol.cpp` |
| Engine CRC verification | `EngineInfoVerifier.cs` | `Net_VerifyEngine` CRC matching |
| Service payloads | `PregameServicePayloads.cs` | `Net_SetMapLoadInfo`, `Net_SetEngineInfo`, roster layout |
| Session snapshot config | `PregameSessionSnapshot.cs` | host map/game/roster data for WAITING driver |
| Host WAITING driver | `PregameHost.DriveWaitingClients` | `HostGame` WAITING loop |
| Guest WAITING handler | `PregameGuest` service switch | guest `HPS_*` handlers in `JoinGame` |

**Correction from 2a:** The initial `PregameServiceHeader` treated bytes 0–3 as CRC inside the 15-byte header. The C++ `NetBuffer` layout places `NCMD_SETUP` at byte 0; CRC is only on the wire in `TransmitBuffer[0..3]`. The incorrect struct was removed; the correct layout lives in `HcdeServicePacket`.

**Enum fix:** `PregameServiceType` now matches C++ `EHCDEPregameService` (starts at `Heartbeat = 1`).

**Bug fixed in guest pump:** `PRE_HCDE_SERVICE` (type 10) sits numerically between reject codes and must not be classified as a rejection by range check.

## Verification matrix (Phase 2a + 2b)

| Check | Method | Result |
| --- | --- | --- |
| Default game port 5029 | `NetConstantsTests` | Pass |
| HCD3 connect info round-trip | `HcdeConnectInfoTests` | Pass |
| Launcher challenge BE encoding | `ServerQueryCodecTests` | Pass |
| Server query response round-trip | `ServerQueryCodecTests` | Pass |
| Query client vs loopback UDP server | `ServerQueryIntegrationTests` | Pass |
| CRC-32 golden vector | `Crc32Tests` | Pass |
| Setup packet CRC wire round-trip | `SetupPacketCodecTests` | Pass |
| HCDE service 15-byte header layout | `HcdeServicePacketTests` | Pass |
| Duplicate seq treated as benign | `PregameServiceReceiverTests` | Pass |
| PRE_CONNECT_ACK round-trip | `ConnectAckCodecTests` | Pass |
| Reliable queue ack refresh on flush | `ReliableServiceQueueTests` | Pass |
| Connect ACK + console-player wire path | `PregameHandshakeIntegrationTests` | Pass |
| PRE_CONNECT round-trip with HCD3 | `ConnectPacketCodecTests` | Pass |
| Engine info 0-WAD layout | `EngineInfoCodecTests` | Pass |
| Session token never zero | `SessionTokenTests` | Pass |
| Compressed setup decode | `SetupPacketCompressionTests` | Pass |
| Host/guest UDP admission handshake | `PregameHostGuestLoopbackTests` | Pass |
| Host rejects missing HCD3 block | `PregameHostGuestLoopbackTests` | Pass |
| Non-blocking UDP drain (no throw) | `PregameHostGuestLoopbackTests` | Pass |
| Engine CRC verification rules | `EngineInfoVerifierTests` | Pass |
| Map/game/roster payload round-trips | `PregameServicePayloadTests` | Pass |
| Full WAITING setup to READY (loopback) | `GuestCompletesWaitingSetupHandshake` | Pass |

**Test count:** 55 passing (`dotnet test` in `csharp/`).

## Not yet in Phase 2b (sign-off blockers)

| Item | Notes |
| --- | --- |
| Full `Net_VerifyEngine` version-byte rejection + verification-error replies | Host sends `PRE_PROTOCOL_ERROR` today; `PRE_VERIFICATION_ERROR` payload not ported |
| `HPS_START_GAME` / bootstrap / resync services | Runtime late-join path |
| Cross-language soak (C# guest vs C++ `hcdeserv`) | **Phase 2b acceptance gate** |
| Live gameplay lanes (`HGP_*`, `HLANE_*`) | Phase 2c |

## Principal risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| `i_net.cpp` is ~4.7k LOC with UI coupling | High | Port in layers; keep C++ as reference; cross-test each layer |
| `d_net.cpp` monolith (~20k LOC with `.inl`) | Extreme | Server-only path first; reuse Python stress harness |
| Struct layout / endian drift | High | Golden-vector tests per message type |
| Premature C++ deletion | High | Phase 2 audit gates removal per sub-phase |
| Playsim determinism | Extreme | Defer to 2e; document before implementation |
| Engine verification mismatch | Medium | Cross-test against C++ before claiming 2b sign-off |

## Sign-off criteria for Phase 2 (full)

Phase 2 is complete when **all** hold:

- [ ] C# `hcdeserv` loads a map and runs authoritative ticks without renderer/audio
- [ ] `tests/netcode_step12/netcode_step12_stress.py` passes against C# server + C++ client (and vice versa where applicable)
- [ ] `tests/mbf21_validation/` and `tests/id24_validation/` pass on C# server path
- [ ] Principal audit updated with verification evidence
- [ ] C++ dedicated server remains available until one release cycle with C# server validated

## Sign-off criteria for Phase 2b (pregame only)

Phase 2b is complete when **all** hold:

- [x] C# codecs for PRE_CONNECT, PRE_CONNECT_ACK, PRE_HCDE_SERVICE, CRC wire envelope
- [x] Reliable service queue with benign duplicate handling
- [x] C# host/guest loopback admission through console-player assignment
- [x] C# host/guest loopback through WAITING setup (user-info, map-load, game-info, roster)
- [x] Engine CRC list verification (`EngineInfoVerifier`)
- [ ] C# guest completes pregame admission against shipping C++ `hcdeserv`
- [ ] Principal audit updated with cross-language evidence

## Phase 2b next slice

1. **Cross-language soak** — C# `PregameGuest` against C++ `hcdeserv` on loopback with real WAD CRCs
2. **`PRE_VERIFICATION_ERROR` replies** — wire-compatible rejection payloads
3. **Runtime services** — `HPS_START_GAME`, bootstrap, resync (late-join path)

Do **not** start `d_net` snapshot lanes until 2b pregame handshake is green against C++.

## Source map

| Concern | C++ | C# (Phase 2) |
| --- | --- | --- |
| UDP sockets | `i_net.cpp` | `HCDE.Net.Transport/UdpTransport.cs` |
| Server info query | `I_QueryServerInfo` | `ServerQueryClient.cs` |
| Pregame constants | `i_net.cpp` | `PregameConstants.cs` |
| Service packet + queue | `BeginHCDEPregameService`, `FHCDEPendingService` | `HCDE.Net.Pregame/*` |
| PRE_CONNECT admission | `TryProcessSetupConnectPacket` | `PregameHost.cs`, `ConnectPacketCodec.cs` |
| Guest join loop | `JoinGame` | `PregameGuest.cs` |
| Live netcode | `d_net*.cpp` | Not started (2c) |
| RCON server | `d_net_rcon.cpp` | Phase 1 client only; server stays C++ until 2f |

## Audit conclusion (interim)

**Phase 2b C# loopback pregame setup is working end-to-end through WAITING** — guest admission, user-info exchange, map-load/game-info/roster services, and host READY promotion all pass in loopback tests. The remaining 2b gate is interoperability with the shipping C++ dedicated server.

Next audit checkpoint: **Phase 2b sign-off** when `PregameGuest` interoperates with C++ `hcdeserv`.
