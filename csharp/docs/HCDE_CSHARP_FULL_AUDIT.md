# HCDE C# Migration — Full Principal Audit

**Last updated:** 2026-08-12  
**Scope:** All code under `csharp/` (7 projects, 6 test suites)  
**Verification:** `dotnet build` and `dotnet test` in `csharp/` — **174 tests passing**  
**Related:** [`HCDE_CSHARP_PHASE1_AUDIT.md`](HCDE_CSHARP_PHASE1_AUDIT.md) · [`HCDE_CSHARP_PHASE2_AUDIT.md`](HCDE_CSHARP_PHASE2_AUDIT.md) · [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md)

---

## 1. Executive summary

The C# tree is a **well-tested protocol and networking foundation** (~9,100 LOC source, ~131 unit/integration tests) covering Phase 1 tools and Phase 2a–2c wire codecs. It does **not** yet run a game: no map loader, playsim, or `hcdeserv` executable.

| Layer | Status | Confidence |
| --- | --- | --- |
| Phase 1 — tools & master protocol | **Complete** | High (loopback); medium without C++ binary soak |
| Phase 2a — UDP transport & query | **Complete** | High |
| Phase 2b — pregame handshake | **~95%** (fresh join loopback) | Medium (no recorded C++ interop) |
| Phase 2c — live netcode wire | **~50–55%** of `d_net` surface | Medium (encode-heavy; apply paths absent) |
| Phase 2d–2f — map, playsim, server | **Not started** | — |
| Phase 3–4 — full sim & client | **Not started** | — |

**Overall migration progress (by engine LOC):** ~5–10% of `src/` (~640k LOC).  
**Runnable C# dedicated server:** 0%.

---

## 2. Inventory

### 2.1 Solution layout

```
csharp/
  HCDE.sln
  src/
    HCDE.Protocol/          8 files,   ~923 LOC   (shared wire types)
    HCDE.Master/            2 files,   ~318 LOC   (hcdemaster binary)
    HCDE.Rcon/              2 files,   ~157 LOC   (hcdercon binary)
    HCDE.Net.Transport/    13 files,   ~812 LOC   (UDP, CRC, query, pregame constants)
    HCDE.Net.Pregame/      22 files, ~2,038 LOC   (pregame host/guest pumps)
    HCDE.Net.Core/         56 files, ~5,100 LOC   (live protocol codecs + session glue)
    HCDE.PregameGuest.Cli/  5 files,   ~207 LOC   (hcde-pregame-guest CLI)
  tests/                   38 files, 174 tests
```

### 2.2 Test matrix

| Suite | Tests | Role |
| --- | ---: | --- |
| `HCDE.Protocol.Tests` | 15 | Master + NMS1 + RCON hash golden vectors |
| `HCDE.Master.Tests` | 1 | UDP heartbeat + list query integration |
| `HCDE.Rcon.Tests` | 6 | FNV-1a + loopback auth/ping/status |
| `HCDE.Net.Transport.Tests` | 10 | Constants, query, HCD3, gameplay CRC |
| `HCDE.Net.Pregame.Tests` | 32 | CRC, service queue, host/guest loopback |
| `HCDE.Net.Core.Tests` | 110 | Live headers, bodies, tail, DEM, sessions, apply |
| **Total** | **174** | |

### 2.3 Dependency graph

```
HCDE.Protocol (no deps)
  ├── HCDE.Master
  ├── HCDE.Rcon
  └── (indirect via Transport)

HCDE.Net.Transport
  ├── HCDE.Net.Pregame ──► HCDE.Net.Core  ⚠ layering bleed
  └── HCDE.Net.Core
        └── HCDE.PregameGuest.Cli
```

**Architecture concern:** `HCDE.Net.Pregame` references `HCDE.Net.Core` for `LiveAuthoritySession` handoff (`TryCreateLiveAuthoritySession`, `PumpLiveClients`). Strict 2b isolation would move this to a glue project.

---

## 3. Phase 1 — Tools & protocol

### 3.1 HCDE.Protocol

**C++ mirrors:** `protocol/hcde_master_protocol.h`, `tools/hcdemaster/hcdemaster.cpp`, `src/common/engine/sv_master_nms1.*`, `tools/hcdercon/hcdercon.cpp`, `src/d_net_rcon.cpp`

| Module | Public surface | Completeness | Tests |
| --- | --- | --- | --- |
| `MasterProtocol.cs` | Constants, NMS1 enums | 100% constants in scope | Subset asserted |
| `MasterPackets.cs` | Legacy heartbeat/list | 100% | Round-trip |
| `Nms1Packets.cs` | NMS1 client + **server writers** (C#-only) | Client parity; server writers untested vs C++ | Challenge golden vector (21 B) |
| `RconProtocol.cs` | BE length framing + FNV-1a | 100% client path | Hash golden vector |

**Strengths**
- Idiomatic `Try*` APIs, `BinaryPrimitives` endianness, duplicate-field rejection on NMS1
- NMS1 challenge request matches C++ byte-for-byte

**Gaps**
- No JSON/codegen — C#, `.h`, and JSON are hand-synced
- NMS1 `ListRequest`/`ListResponse`/`Entries`/`PublicIp` — enum only, no codec (same as C++ client port)
- RCON framing not unit-tested directly (only via `HCDE.Rcon` integration)
- NMS1 register/heartbeat/unregister **write** paths lack C++ golden vectors

**Wire risks**

| Risk | Severity |
| --- | --- |
| Constant drift across C#/`.h`/JSON | High |
| NMS1 server-side writers (C#-only) | Medium |
| RCON empty-frame rejection (C# only) | Very low |

### 3.2 HCDE.Master (`hcdemaster`)

**C++ mirror:** `tools/hcdemaster/hcdemaster.cpp`

| Behavior | Match |
| --- | --- |
| CLI flags, defaults, TTL prune, heartbeat key `{IP}:{port}` | ✓ |
| Legacy UDP only (no NMS1) | ✓ (same as C++) |
| List entry order | Non-deterministic (both use hash-map iteration) |

**Tests:** 1 integration test (heartbeat + list). Missing: TTL expiry, `--max-packets`, multi-server, C++ cross-binary soak.

### 3.3 HCDE.Rcon (`hcdercon`)

**C++ mirror:** `tools/hcdercon/hcdercon.cpp` · server: `src/d_net_rcon.cpp`

| Behavior | Match |
| --- | --- |
| `nonce` → `auth {fnv8}` → `OK*` → command | ✓ |
| Max frame 4096, BE length prefix | ✓ |

**Tests:** 6 (hash + loopback `RconLoopbackServer` — simplified server, not full `d_net_rcon.cpp`).

**Gap:** No live `hcdeserv` soak. `IPAddress.Parse` accepts IPv6; C++ client is IPv4-only.

### 3.3 Phase 1 verdict

**Sign-off:** Complete for agreed scope. Acceptable to ship C# `hcdemaster`/`hcdercon` as tool replacements with documented follow-ups (CI, golden vectors, live RCON soak).

---

## 4. Phase 2a — HCDE.Net.Transport

**C++ mirror:** `src/common/engine/i_net.cpp` (sockets, query, constants), `m_crc32.h`

| File | Purpose | Completeness | Tests |
| --- | --- | --- | --- |
| `NetConstants.cs` | Ports, command enums, buffer sizes | 98% | ✓ |
| `PregameConstants.cs` | PRE_*, HCDE service offsets | 100% enums | Indirect |
| `HcdeConnectInfo.cs` | `HCD3` block | 100% | ✓ |
| `UdpTransport.cs` | Socket wrapper | 75% | Loopback only |
| `NetworkEndpoint.cs` | `host:port` parse | 85% | 2 cases |
| `ServerQueryCodec.cs` | Launcher query | 92% | ✓ |
| `ServerQueryClient.cs` | One-shot query | 90% | ✓ integration |
| `Crc32.cs` | zlib CRC-32 | 100% | ✓ (in Pregame.Tests) |
| `GameplayWireCodec.cs` | GameID CRC envelope | 70% | ✓ (2c bleed) |

**Gaps**
- No outbound zlib compression on send helpers (`PregameWire` decode-only)
- IPv6 not supported (matches C++ `AF_INET` intent)
- Timeout constants in `PregameConstants` exist but pumps don't enforce them
- `GameplayWireCodec` lives in Transport but is live-netcode concern

**Verdict:** Production-ready for UDP query, constants, and gameplay CRC primitives.

---

## 5. Phase 2b — HCDE.Net.Pregame

**C++ mirror:** `i_net.cpp` PRE_* / HCDE service queue / `JoinGame` / `HostGame`

### 5.1 Codecs (high confidence)

| Component | Completeness | Tests |
| --- | --- | --- |
| `SetupPacketCodec` | 85% (encode compressed exists; send path doesn't use it) | ✓ |
| `HcdeServicePacket` | 100% layout | ✓ golden 15-byte header |
| `PregameServiceReceiver` | 90% | ✓ accept + duplicate |
| `ReliableServiceQueue` | 88% (no hard timeout expiry) | ✓ ack refresh |
| `ConnectPacketCodec` / `ConnectAckCodec` | 90–95% | ✓ |
| `EngineInfoCodec` / `EngineInfoVerifier` | 85–90% | ✓ CRC rules |
| `VerificationErrorCodec` | 90% | ✓ + host integration |
| `PregameServicePayloads` | 88% | ✓ map/game/roster |
| `SessionToken` | 95% | ✓ non-zero; no C++ golden |

### 5.2 Pumps (partial)

| Component | Completeness | What's missing |
| --- | --- | --- |
| `PregameHost` | ~68% | Timeouts, heartbeats, ban/in-progress, bootstrap/resync, multi-guest roster, disconnect cleanup |
| `PregameGuest` | ~70% | Connect resend, heartbeats, bootstrap/resync, `PRE_DISCONNECT`, setup timeout |
| `PregameWire` | 80% | Outbound compression |

### 5.3 Loopback-verified path

```
PRE_CONNECT → PRE_CONNECT_ACK → console-player
  → client-user-info → user-info-ack
  → map-load → map-load-ack
  → game-info → game-info-ack
  → roster → roster-ack
  → READY → HPS_START_GAME → HPS_START_GAME_ACK → Starting
  → (optional) LiveAuthoritySession handoff
```

### 5.4 HCDE.PregameGuest.Cli

| Feature | Status |
| --- | --- |
| `--server`, `--wad-crc`, `--timeout-ms` | ✓ |
| `--live-ticks` (2c bleed) | ✓ |
| CLI unit tests | ✗ |
| Python `pregame_guest_smoke.py` | Present; skips without `hcdeserv`/IWAD |

### 5.5 Phase 2b verdict

**Feature-complete for fresh dedicated join in C#-only loopback.** Not principal-signed until:
- Cross-language soak vs shipping `hcdeserv` is recorded
- Runtime timeouts and late-join services are implemented or explicitly deferred

---

## 6. Phase 2c — HCDE.Net.Core

**C++ mirror:** `d_net.cpp` (~10.7k LOC) + `d_net_snapshot_part1.inl` (~8k) + `d_net_snapshot_part2.inl` (~1.6k)

### 6.1 Complete (encode + decode + tests)

| Layer | Types | C++ reference |
| --- | --- | --- |
| Live envelope | `LiveHeader`, `LivePacket`, `GameplayEnvelope` | `HLIV`, `HGPL` |
| Control | `LiveControlCapabilities`, scheduler, sequence tracker | `HCAP`, `SendHCDELiveControl` |
| HCIN/HCSN headers | `ClientInputHeader`, `ServerSnapshotHeader` | 29 B / 31 B layouts |
| HCIR/HCSR bodies | `ClientInputBodyCodec`, `ServerSnapshotBodyCodec` | Player records |
| User command | `UserCmd`, `UserCmdCodec` | 16-byte `usercmd_t` |
| Event records | `EventRecordsCodec` | BE16 count block |
| DEM canonicalization | `DemoCommand`, `Canonical*`, `DemEventStreamConverter` | Subset of `HCDEAppendCanonicalEventPayload` |
| HCDW | `WorldDeltaPoseCodec`, `WorldDeltaChunkCodec` | V4 pose (38 B) + sector (11 B) |
| HCDA | `ActorDeltaRecord`, `ActorDeltasCodec` | V2 masked records |
| HCKS | `SnapshotChecksumCodec` | 34-byte wire block (parse/write only) |
| HCIV | `InvasionSnapshotCodec` | V2 header + embedded HCAV/HCDA skip |
| HCDS | `CoopDeadSpawnsCodec` | Co-op dead spawn index list |
| ECHO | `PresentationEchoCodec`, `PresentationEchoRecord` | v8 inventory + player records |
| HCAV | `AuthorityEventsCodec`, `AuthorityEventRecord` | V1 header + spawn/despawn/damage/cosmetic records |
| Checksum compare | `SnapshotChecksumRing`, `SnapshotChecksumSession` | Ring buffer + mixer + compute-if-stale |
| Tail walker | `ServerSnapshotTailWalker` | Co-op vs invasion tail order |
| Tail assembler | `ServerSnapshotTailCodec` | HCDW + HCDA + ECHO + optional HCKS |
| HCSN quitter prefix | `ServerSnapshotQuitterCodec`, `LivePeerSlotTracker` | `NCMD_QUITTERS` + guest disconnect tracking |
| Weapon-slot DEM | `CanonicalWeaponIndexCodec`, slot payloads in `CanonicalEventPayloadCodec` | `DEM_SETSLOT*`, `DEM_ADDSLOT*` |
| ECHO apply | `PresentationEchoApplySession`, `PresentationEchoWeaponChangePolicy` | inventory reconcile + weapon follow policy |
| HCAV apply router | `AuthorityEventsApplySession`, `IAuthorityEventSink` | invasion/coop/pickup dispatch table |
| HCSR/HCIN apply | `ServerSnapshotApplySession`, `ClientInputApplySession` | sequence/consistency + command sinks |
| World-delta apply | `WorldDeltaApplySession`, `ActorDeltasApplySession`, `CoopDeadSpawnsApplySession` | HCDW/HCDA/HCDS validation + sinks |
| Invasion apply | `InvasionSnapshotApplySession`, `InvasionSnapshotWavePolicy` | HCIV mirror + embedded HCAV/HCDA replay |
| Spawn directory | `InvasionSpawnDirectoryCodec` | HCIV V2 spawn metadata mirror |
| Checksum apply | `SnapshotChecksumApplySession` | HCKS ring compare + mismatch sink |
| Session glue | `LiveWire`, `Live*Endpoint`, `Live*Session`, `LiveAuthorityClientRegistry` | UDP pump + multi-client authority |
| Routing | `LiveAuthorityRouting`, `LivePeerRoutingState` | `I_ShouldSend/AcceptHCDELive*` |

### 6.2 Partial / stubbed

| Item | What exists | What's missing |
| --- | --- | --- |
| HCAV authority events | Full record encode/decode + replay router | Playsim-backed sink implementations |
| HCKS checksum | Wire parse/write + ring compare + mixer session | Playsim-fed category inputs |
| DEM payloads | ~50 event types incl. weapon slots | No reverse (canonical→legacy) |
| ECHO presentation | Full inventory/player encode-decode + apply session | Playsim-backed inventory/weapon follow |
| HCIV invasion | V2 header + embedded skip | Spawn spot payloads, full invasion state |
| Guest receive | HCSR/HCIN apply + tail sinks (ECHO/HCAV/HCDW/HCDA/HCDS/HCIV) | Playsim-backed mutation + invasion spawn spots |

### 6.3 Missing entirely

| Feature | C++ location |
| --- | --- |
| **Apply paths** (`HCDETryApplyNative*`) | World-delta mutation + playsim command execution |
| Lag comp, prediction, rewind hooks | `d_net.cpp` |
| Lane budget enforcement | `HCDELiveLaneBudget*` |
| Actor baseline repair handler | `HCDEBeginActorBaselineRepair` |
| Predator snapshot codec | Capability bit only |

### 6.4 C++ vs C# snapshot tail order

```
C++ (co-op):     HCSR → HCDW → HCDA → HCDS → HCAV → ECHO → HCKS
C++ (invasion):  HCSR → HCDW → HCIV (embeds HCAV+HCDA) → ECHO → HCKS
C# (minimal):    HCSR → HCDW → HCDA → ECHO → [HCKS]
C# (walker):     Parses co-op and invasion order; skips HCAV/HCDA inside HCIV
```

**Interop risk:** Tail parse/skip is implemented; apply/reconciliation still absent. Full ECHO inventory bodies not yet encoded.

### 6.5 Phase 2c verdict

Wire-first codecs for the **core live envelope and record bodies** are in good shape. **HCSR/HCIN apply sessions** now track sequence/consistency and dispatch commands via injectable sinks; world-delta mutation and playsim execution remain deferred to Phase 2e. Estimated **~55–60%** of `d_net` wire surface; **~0%** of playsim integration.

---

## 7. Cross-cutting quality assessment

### 7.1 Strengths

1. **Consistent patterns** — `TryRead`/`Write`, `BinaryPrimitives`, reject-reason strings, golden layout tests
2. **Layered design** — Protocol separate from Transport separate from Pregame/Core
3. **Test density** — ~1 test per 70 LOC source (high for protocol code)
4. **Documentation in code** — XML comments link to C++ reference functions
5. **Incremental migration** — C++ remains production; C# proves compatibility per layer

### 7.2 Weaknesses

1. **No C++ binary interop evidence** — all compatibility inferred from unit tests
2. **Hand-maintained constants** — drift risk across C#, `.h`, JSON
3. **Single-player test bias** — multi-client scenarios under-tested
4. **Encode-heavy** — many codecs write well; apply/decode paths for full snapshots incomplete
5. **README outdated** — `csharp/README.md` still lists netcode/playsim as "Planned"
6. **No CI job** — `dotnet test` not in automated pipeline (recommended since Phase 1)
7. **Layering leaks** — Transport has gameplay CRC; Pregame references Core for live handoff

### 7.3 Principal risk register

| Risk | Severity | Mitigation |
| --- | --- | --- |
| Wire constant drift | High | JSON codegen; expand golden vectors |
| No cross-language soak | High | Run `pregame_guest_smoke.py` + `netcode_step12` vs C++ |
| Tail order mismatch | Medium | Walker parses co-op/invasion; finish ECHO/HCAV bodies |
| Missing apply paths | High | Expected until 2e; document boundary |
| Pregame timeout gaps | Medium | Wire existing constants into pumps |
| Compression send gap | Medium | Use `SetupPacketCodec.EncodeCompressed` in `PregameWire` |
| DEM enum drift | Medium | Expand `DemoCommand` + golden tests |
| Architecture coupling | Low | Extract pregame→live glue project |

---

## 8. What's not in C# at all

| Planned project | C++ reference | LOC order of magnitude |
| --- | --- | --- |
| `HCDE.MapLoader` | `maploader/`, `p_setup.cpp` | Tens of thousands |
| `HCDE.Gamedata` | DEHACKED, MAPINFO, UDMF | Tens of thousands |
| `HCDE.Playsim` | `playsim/`, `p_tick.cpp` | **Hundreds of thousands** |
| `HCDE.Server` | `d_main.cpp` dedicated path | Medium (orchestration) |
| `HCDE.Client` | Full client | Majority of engine |
| In-engine RCON server | `d_net_rcon.cpp` server side | Small (client tool done) |
| ZScript VM, renderers, audio | Various | Keep native / P/Invoke |

---

## 9. Recommended actions (prioritized)

### P0 — Trust & CI
1. Add GitHub Actions job: `cd csharp && dotnet test`
2. Record cross-language pregame soak (`pregame_guest_smoke.py` + `hcdeserv`)
3. Export C++ golden vectors for session token, NMS1 writes, one full HCIN/HCSN capture

### P1 — Phase 2c completion
4. HCIV header codec + invasion tail path
5. ECHO parse (read-first; write later)
6. Full snapshot tail walker (HCDS → HCAV → ECHO → HCKS)
7. `LiveGuestSession` tail consumption

### P1 — Phase 2b hardening
8. Enforce `ServiceTimeoutMilliseconds` / `GuestSetupProgressTimeoutMilliseconds` in pumps
9. Wire outbound compression in `PregameWire.TrySend`
10. Decouple `PregameHost` live handoff to glue layer

### P2 — Phase 2d entry
11. Begin `HCDE.MapLoader` with UDMF/BSP subset
12. JSON → C# codegen for `MasterProtocol` + net constants

### P3 — Documentation hygiene
13. Update `csharp/README.md` status table
14. Keep this audit updated at each iteration checkpoint

---

## 10. Per-file index (quick reference)

### HCDE.Protocol (5 source files)
`MasterProtocol.cs` · `MasterPackets.cs` · `Nms1Types.cs` · `Nms1Packets.cs` · `RconProtocol.cs`

### HCDE.Master (2)
`MasterServer.cs` · `Program.cs`

### HCDE.Rcon (2)
`RconClient.cs` · `Program.cs`

### HCDE.Net.Transport (13)
`NetConstants.cs` · `PregameConstants.cs` · `HcdeConnectInfo.cs` · `UdpTransport.cs` · `NetworkEndpoint.cs` · `ServerQueryModels.cs` · `ServerQueryCodec.cs` · `ServerQueryClient.cs` · `Crc32.cs` · `GameplayWireCodec.cs`

### HCDE.Net.Pregame (22)
`SetupPacketCodec.cs` · `HcdeServicePacket.cs` · `PregameServiceReceiver.cs` · `ReliableServiceQueue.cs` · `PregameServiceSender.cs` · `PregameConnectionState.cs` · `ConnectAckCodec.cs` · `ConnectPacketCodec.cs` · `EngineInfoCodec.cs` · `EngineInfoVerifier.cs` · `VerificationErrorCodec.cs` · `ProtocolStreamCodec.cs` · `PregameServicePayloads.cs` · `PregameSessionSnapshot.cs` · `SessionToken.cs` · `PregameWire.cs` · `PregameClient.cs` · `PregameHost.cs` · `PregameGuest.cs` · `PregameHostOptions` (in Host) · enums in Transport

### HCDE.Net.Core (35)
`LiveConstants.cs` · `LiveHeaderCodec.cs` · `GameplayEnvelopeCodec.cs` · `LiveControlCapabilitiesCodec.cs` · `LiveCapabilities.cs` · `LiveSequenceTracker.cs` · `LivePacketCodec.cs` · `ClientInputHeaderCodec.cs` · `ServerSnapshotHeaderCodec.cs` · `ClientInputBodyCodec.cs` · `ServerSnapshotBodyCodec.cs` · `UserCmd.cs` · `UserCmdCodec.cs` · `EventRecordsCodec.cs` · `DemoCommand.cs` · `CanonicalStringCodec.cs` · `CanonicalEventPayloadCodec.cs` · `CanonicalCVarChangeCodec.cs` · `CanonicalRunArgsCodec.cs` · `DemEventStreamConverter.cs` · `WorldDeltaPoseCodec.cs` · `WorldDeltaChunkCodec.cs` · `LaneChunkHeaderCodec.cs` · `ActorDeltaQuantization.cs` · `ActorDeltaRecord.cs` · `ActorDeltasCodec.cs` · `SnapshotChecksumCodec.cs` · `ServerSnapshotTailCodec.cs` · `GameplayPayloadBuilders.cs` · `LiveWire.cs` · `LiveAuthorityRouting.cs` · `LiveSession.cs`

### HCDE.PregameGuest.Cli (5)
`Program.cs` · `GuestCliOptions.cs` · project file

---

## 11. Audit conclusion

The C# migration has produced a **credible, well-tested networking stack** that mirrors C++ wire layouts for master protocol, pregame handshake, and live packet envelopes. Code quality is high: consistent APIs, good test coverage for the scope covered, and clear C++ traceability.

What exists today is **infrastructure**, not a game server. The next meaningful milestones are:

1. **Prove interop** with shipping C++ binaries (pregame + netcode harnesses)
2. **Finish live tail codecs** (HCIV, ECHO, full tail walker)
3. **Start map loader** (2d) — the gate to any authoritative tick loop
4. **Port playsim** (2e) — the bulk of remaining work

Until 2d–2e land, C# cannot replace `hcdeserv`. The current tree is the right foundation; the mountain is simulation, not more UDP headers.
