# HCDE C# Migration — Full Principal Audit

**Last updated:** 2026-08-18  
**Scope:** All code under `csharp/` (7 projects, 6 test suites)  
**Verification:** `dotnet build` and `dotnet test` in `csharp/` — **371 tests passing** (CI: `.github/workflows/csharp.yml`; optional soak: `.github/workflows/csharp-cross-language-soak.yml`)  
**Related:** [`HCDE_CSHARP_PHASE1_AUDIT.md`](HCDE_CSHARP_PHASE1_AUDIT.md) · [`HCDE_CSHARP_PHASE2_AUDIT.md`](HCDE_CSHARP_PHASE2_AUDIT.md) · [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md)

---

## 1. Executive summary

The C# tree is a **well-tested protocol and networking foundation** (~15,500 LOC source, 371 unit/integration tests) covering Phase 1 tools, Phase 2a–2c wire codecs, `HCDE.Server` live authority pump with HCDW/HCDA/HCDS/HCIV/HCAV tail shipping, BEHAVIOR bytecode walk, and Phase 2d unified binary map decode.

| Layer | Status | Confidence |
| --- | --- | --- |
| Phase 1 — tools & master protocol | **Complete** | High (loopback); medium without C++ binary soak |
| Phase 2a — UDP transport & query | **Complete** | High |
| Phase 2b — pregame handshake | **~95%** (fresh join loopback) | Medium (no recorded C++ interop) |
| Phase 2c — live netcode wire | **~60%** of `d_net` wire surface; apply stubs in place | Medium (playsim mutation deferred) |
| Phase 2d — map loader | **In progress** (unified `BinaryMapDecoder`; full binary lump decode through collision + BEHAVIOR script directory) | Medium |
| Phase 2f — server shell | **In progress** (`HCDE.Server` query/advertise + pregame + map-load bootstrap + live authority pump) | Low |
| Phase 3–4 — full sim & client | **Not started** | — |

**Overall migration progress (by engine LOC):** ~2% of HCDE-owned C++ (`src/` + `tools/` ≈ 672k LOC).  
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
    HCDE.Net.Pregame/      24 files, ~2,270 LOC   (pregame host/guest pumps + cross-language soak suite)
    HCDE.Net.Core/         72 files, ~8,450 LOC   (live protocol codecs + session glue + world-store stubs + authority map-load)
    HCDE.MapLoader/         21 files, ~1,550 LOC   (WAD directory + unified binary map decode + BEHAVIOR directory)
    HCDE.Server/             6 files,   ~420 LOC   (hcdeserv host + CLI + query/advertise)
    HCDE.PregameGuest.Cli/  5 files,   ~207 LOC   (hcde-pregame-guest CLI)
  tests/                   69 files, 371 tests
```

### 2.2 Test matrix

| Suite | Tests | Role |
| --- | ---: | --- |
| `HCDE.Protocol.Tests` | 15 | Master + NMS1 + RCON hash golden vectors |
| `HCDE.Master.Tests` | 1 | UDP heartbeat + list query integration |
| `HCDE.Rcon.Tests` | 6 | FNV-1a + loopback auth/ping/status |
| `HCDE.Net.Transport.Tests` | 10 | Constants, query, HCD3, gameplay CRC |
| `HCDE.Net.Pregame.Tests` | 69 | CRC, service queue, host/guest loopback, bootstrap/resync, cross-language soak + Passed export bundle |
| `HCDE.Net.Core.Tests` | 203 | Live headers, bodies, tail, DEM, sessions, world-store + HCDA/HCDS/HCAV/ECHO tail + checksum resync |
| `HCDE.MapLoader.Tests` | 59 | WAD directory, unified map decode, BEHAVIOR directory + bitwise shift bytecode walk |
| `HCDE.Server.Tests` | 9 | Dedicated host bind, CLI parser, query responder, master heartbeat, pregame→live bootstrap + guest apply E2E |
| **Total** | **356** | |

### 2.3 Dependency graph

```
HCDE.Protocol (no deps)
  ├── HCDE.Master
  ├── HCDE.Rcon
  └── (indirect via Transport)

HCDE.Net.Transport
  ├── HCDE.Net.Pregame ──► HCDE.Net.Core ──► HCDE.MapLoader  ⚠ layering bleed
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
| World-state stub | `GuestWorldStateStore`, `SnapshotChecksumPlaysimInputs` | In-memory HCDW/HCDA apply + checksum input bridge |
| Soak evidence | `CrossLanguageSoakEvidence`, pregame/netcode runners | JSON audit trail for cross-language runs |
| Session glue | `LiveWire`, `Live*Endpoint`, `Live*Session`, `LiveAuthorityClientRegistry` | UDP pump + multi-client authority |
| Routing | `LiveAuthorityRouting`, `LivePeerRoutingState` | `I_ShouldSend/AcceptHCDELive*` |

### 6.2 Partial / stubbed

| Item | What exists | What's missing |
| --- | --- | --- |
| HCAV authority events | Full record encode/decode + replay router | Playsim-backed sink implementations |
| HCKS checksum | Wire parse/write + ring compare + mixer session + world-store input builder | Real playsim category inputs; guest receive wiring |
| DEM payloads | ~50 event types incl. weapon slots | No reverse (canonical→legacy) |
| ECHO presentation | Full inventory/player encode-decode + apply session | Playsim-backed inventory/weapon follow |
| HCIV invasion | V2 header + embedded skip + spawn directory | Spawn spot payloads, full invasion state |
| Guest receive | HCSR/HCIN apply + tail sinks | Map-to-world-store bootstrap from decoded lumps |
| World-state wiring | `SetGuestWorldState` + `SetAuthorityWorldState` + `WorldStateTailBuilder` | Complete |

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

Wire-first codecs for the **core live envelope and record bodies** are in good shape. **LiveGuestSession.SetGuestWorldState** now applies HCDW/HCDA into an in-memory store and computes HCKS from applied state before compare. Full playsim execution remains deferred to Phase 2e. Estimated **~60%** of `d_net` wire surface; **~2%** of HCDE-owned C++ LOC migrated.

---

## 7. Cross-cutting quality assessment

### 7.1 Strengths

1. **Consistent patterns** — `TryRead`/`Write`, `BinaryPrimitives`, reject-reason strings, golden layout tests
2. **Layered design** — Protocol separate from Transport separate from Pregame/Core
3. **Test density** — ~1 test per 70 LOC source (high for protocol code)
4. **Documentation in code** — XML comments link to C++ reference functions
5. **Incremental migration** — C++ remains production; C# proves compatibility per layer

### 7.2 Weaknesses

1. **No C++ binary interop evidence recorded in audit** — soak runners and JSON evidence writer exist; CI/agent image still lacks `hcdeserv`/IWAD binaries
2. **Hand-maintained constants** — drift risk across C#, `.h`, JSON
3. **Single-player test bias** — multi-client scenarios under-tested
4. **Encode-heavy** — many codecs write well; full playsim-backed apply paths incomplete
5. **No CI soak evidence** — managed `dotnet test` CI landed; cross-language soak still needs `hcdeserv`/IWAD in agent image
6. **Layering leaks** — Transport has gameplay CRC; Pregame references Core for live handoff

### 7.3 Principal risk register

| Risk | Severity | Mitigation |
| --- | --- | --- |
| Wire constant drift | High | JSON codegen; expand golden vectors |
| No cross-language soak evidence | High | Set `HCDE_SOAK_EVIDENCE_DIR`; run pregame + Step 12 soaks vs C++ |
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
| `HCDE.MapLoader` | `maploader/`, `p_setup.cpp` | WAD + unified `BinaryMapDecoder` (~1,270 LOC) |
| `HCDE.Gamedata` | DEHACKED, MAPINFO, UDMF | Tens of thousands |
| `HCDE.Playsim` | `playsim/`, `p_tick.cpp` | **Hundreds of thousands** |
| `HCDE.Server` | `d_main.cpp` dedicated path | Medium (orchestration) |
| `HCDE.Client` | Full client | Majority of engine |
| In-engine RCON server | `d_net_rcon.cpp` server side | Small (client tool done) |
| ZScript VM, renderers, audio | Various | Keep native / P/Invoke |

---

## 9. Recommended actions (prioritized)

### P0 — Trust & CI
1. ~~Add GitHub Actions job: `cd csharp && dotnet test`~~ (done — `.github/workflows/csharp.yml`)
2. ~~Optional cross-language soak workflow~~ (done — `.github/workflows/csharp-cross-language-soak.yml`; skips when secrets missing)
3. Record cross-language pregame + Step 12 soaks with `HCDE_SOAK_EVIDENCE_DIR`
4. Export C++ golden vectors for session token, NMS1 writes, one full HCIN/HCSN capture

### P1 — Phase 2d entry
1. ~~VERTEXES/SEGS/NODES lump decode in `HCDE.MapLoader`~~ (done — iteration 25)
2. ~~Map sector bootstrap into `GuestWorldStateStore`~~ (done — `GuestWorldStateBootstrap`)
3. ~~SSECTORS/SIDEDEFS decode + map-load bootstrap E2E~~ (done — iteration 26)
4. ~~BLOCKMAP/REJECT decode + sector light/special on HCDW wire~~ (done — iteration 27)
5. ~~Unified `BinaryMapDecoder` + C++ sector-metadata flag parity~~ (done — iteration 28)
6. ~~BEHAVIOR lump probe + authority map-load bootstrap + cross-language soak suite~~ (done — iteration 29)
7. ~~BEHAVIOR script directory + pregame map-load handoff + soak evidence archive~~ (done — iteration 30)
8. ~~BEHAVIOR bytecode operands + HCDE.Server master advertise + Passed soak evidence in CI~~ (done — iteration 32)
9. ~~Print-stack PCD operands + hcdeserv master CLI + committed soak template refresh~~ (done — iteration 33)
10. ~~HUD/inventory PCD operands + LiveAuthoritySession pump + Passed soak gate~~ (done — iteration 34)
11. ~~Music/stack PCD operands + authority HCDW tail on pump + main CI soak gate~~ (done — iteration 35)
12. ~~Gravity/global PCD operands + player pose tail + Passed soak refresh~~ (done — iteration 36)
13. ~~Call/global-array PCD operands + HCDA actor tail + soak export bundle~~ (done — iteration 37)

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

1. **Record passed cross-language evidence** — re-run `CrossLanguageSoakEvidenceArchive.RecordDefaultEvidence()` when `hcdeserv`/IWAD are available
2. **Map loader read path** — first UDMF/BSP lump parser in `HCDE.MapLoader`
3. **Authority-side checksum generation** on outbound snapshots
4. **Port playsim** (2e) — the bulk of remaining work (~98k LOC in `src/playsim/` alone)

Until 2d–2e land, C# cannot replace `hcdeserv`. The current tree is the right foundation; the mountain is simulation, not more UDP headers.

---

## 12. LOC migration ledger (2026-08-15)

| Tree | LOC | Notes |
| --- | ---: | --- |
| `csharp/src/` | ~15,100 | All C# delivered |
| `src/` | ~659,000 | Engine — primary target |
| `tools/` | ~13,300 | Master/rcon ported; lemon/re2c/zipdir stay |
| `libraries/` | ~891,000 | Vendored — stay native |
| **HCDE-owned C++ remaining** | **~672,000** | `src/` + `tools/` |

| `src/` subdir | LOC | Phase |
| --- | ---: | --- |
| `src/common/` | ~319,000 | Mixed (net, engine, scripting) |
| `src/playsim/` | ~98,000 | 2e |
| `src/rendering/` | ~47,000 | Keep native initially |
| `src/gamedata/` | ~26,000 | 2d |
| `src/maploader/` | ~14,000 | 2d |

| Net C++ reference | LOC |
| --- | ---: |
| `d_net.cpp` | ~10,700 |
| `d_net_snapshot_part*.inl` | ~9,600 |
| `i_net.cpp` | ~4,700 |
