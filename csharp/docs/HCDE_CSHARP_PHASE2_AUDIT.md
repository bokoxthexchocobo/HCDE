# HCDE C# Migration — Phase 2 Principal Audit

**Last updated:** 2026-08-11  
**Status:** In progress — Phase **2c** live protocol codecs (iteration 1) landed. Phase **2b** verification errors, start-game, and cross-language harness complete.  
**Prerequisite:** [Phase 1 audit](HCDE_CSHARP_PHASE1_AUDIT.md) (complete)  
**Related:** [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md) · [`HCDE_NETCODE.md`](../../docs/HCDE_NETCODE.md)

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

**Current work:** Phase **2c** — HLIV/HGPL/HCAP live wire codecs and per-lane sequence tracking. Phase **2b** cross-test harness ready; C++ `hcdeserv` cross-test runs when binaries/IWAD are present.

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

### Verification + start-game + cross-test harness (iteration 4 — this PR)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Verification error codec | `VerificationErrorCodec.cs` | `SendVerificationError` / `ReadVerificationError` |
| Host verification replies | `PregameHost.SendVerificationError` | `PRE_VERIFICATION_ERROR` on failed `Net_VerifyEngine` |
| Start-game service | `PregameHost.StartGame`, guest `HPS_START_GAME` handler | `HPS_START_GAME` / `HPS_START_GAME_ACK` |
| Guest CLI | `HCDE.PregameGuest.Cli` → `hcde-pregame-guest` | C++ `-join` guest path (pregame only) |
| Cross-language harness | `csharp/validation/pregame/pregame_guest_smoke.py` | manual `hcdeserv` + C# guest soak |

### Live netcode wire codecs (Phase 2c — iteration 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Live lanes, message types, capability bits | `LiveConstants.cs` | `d_net.h` `EHCDELiveLane`, `d_net.cpp` `HCDELiveCap*` |
| HLIV 15-byte live header | `LiveHeaderCodec.cs` | `BeginHCDELivePacket`, `HCDELiveBufferLooksLikePacket` |
| HGPL 12-byte gameplay envelope | `GameplayEnvelopeCodec.cs` | `WriteHCDEGameplayEnvelope`, `UnwrapHCDEGameplayEnvelope` |
| HCAP capability block + control base payload | `LiveControlCapabilitiesCodec.cs` | `HCDEAppendLiveControlCapabilities`, `HCDEApplyLiveControlCapabilities` |
| Capability negotiation | `LiveCapabilities.cs` | `HCDEApplyLiveControlCapabilities` negotiated mask |
| Per-lane sequence tracking | `LiveSequenceTracker.cs` | `HCDELiveSequenceIsFresh`, `AcceptHCDELiveSequence` |

### Live payload headers + routing (Phase 2c — iteration 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HCIN 29-byte client-input header | `ClientInputHeaderCodec.cs` | `HCDEClientInputHeaderSize`, `HCDETryApplyNativeClientInput` guards |
| HCIR 6-byte input records header | `ClientInputHeaderCodec.cs` | `HCDEClientInputRecordsHeaderSize` |
| HCSN 31-byte server-snapshot header | `ServerSnapshotHeaderCodec.cs` | `HCDEServerSnapshotHeaderSize` |
| HCSR 6-byte snapshot records header | `ServerSnapshotHeaderCodec.cs` | `HCDEServerSnapshotRecordsHeaderSize` |
| Authority routing predicates | `LiveAuthorityRouting.cs` | `I_ShouldSendHCDELive*`, `I_ShouldAcceptHCDELive*` |
| Live control packet builder | `LivePacketCodec.cs` | `BeginHCDELivePacket`, `SendHCDELiveControl` |
| 1 Hz control scheduler | `LiveControlScheduler` | `HCDELiveControlIntervalMS` |

### Gameplay wire + live UDP pump (Phase 2c — iteration 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Shared CRC-32 | `HCDE.Net.Transport/Crc32.cs` | `m_crc32.h` (moved from Pregame) |
| GameID wire CRC | `GameplayWireCodec.cs` | `SendPacket` / `GetPacket` non-setup path |
| Empty HCIN/HCSN payloads | `GameplayPayloadBuilders.cs` | `HCDEBuildNativeClientInputPayload` / `HCDEBuildNativeServerSnapshotPayload` (zero-player) |
| HGPL-wrapped live packets | `LiveGameplayPacketBuilder` | `WriteHCDEGameplayEnvelope` + native payload |
| Live UDP send/receive | `LiveWire.cs` | `HSendPacket` gameplay path |
| Control endpoint pump | `LiveControlEndpoint` | `SendHCDELiveControl` |
| Gameplay endpoint pump | `LiveGameplayEndpoint` | `HLIVE_CLIENT_COMMANDS` / `HLIVE_SERVER_SNAPSHOT` send |

### DEM canonicalization + snapshot tail (Phase 2c — iteration 5)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| `EDemoCommand` subset + allow-lists | `DemoCommand.cs` | `HCDEIsAllowedTicEventType`, `HCDEIsAllowedClientInputEventType` |
| BE16 length-prefixed strings | `CanonicalStringCodec.cs` | `HCDEAppendCanonicalNullString` |
| Canonical event payload (subset) | `CanonicalEventPayloadCodec.cs` | `HCDEAppendCanonicalEventPayload` |
| Legacy DEM stream → HCIR/HCSR events | `DemEventStreamConverter.cs` | `HCDEAppendEventRecords` |
| HCDW V4 pose + sector records | `WorldDeltaPoseCodec.cs`, `WorldDeltaChunkCodec.cs` | `HCDEAppendServerWorldDeltas` |
| Empty HCDA block | `ActorDeltasCodec.cs` | `HCDEAppendEmptyActorDeltasV2` |
| HCSR tail (HCDW + HCDA) | `ServerSnapshotTailCodec.cs` | post-HCSR snapshot append path |
| Snapshot builder tail option | `GameplayPayloadBuilders.cs` | `includeMinimalTail` on `BuildServerSnapshot` |

### HCDA records, checksum tail, pregame handoff (Phase 2c — iteration 6)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Extended DEM payloads | `CanonicalEventPayloadCodec.cs`, `CanonicalCVarChangeCodec.cs`, `CanonicalRunArgsCodec.cs` | remaining `HCDEAppendCanonicalEventPayload` cases in enum |
| HCDA V2 record bodies | `ActorDeltaRecord.cs`, `ActorDeltaQuantization.cs`, `ActorDeltasCodec.cs` | `HCDEAppendActorDeltasV2` / `HCDEAppendSharedActorDeltasV2` |
| HCKS checksum block | `SnapshotChecksumCodec.cs` | `Net_ChecksumApplyServerChunk` |
| Tail with checksum | `ServerSnapshotTailCodec.cs` | post-HCDA `HCKS` append |
| Live snapshots with tail | `LiveWire.cs` | authority snapshot send path |
| Pregame → live host glue | `PregameHost.cs` | `Host_CheckStartGameAcks` + `I_NetDone` handoff |

### Invasion tail, presentation echo, full tail walker (Phase 2c — iteration 7)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HCIV invasion snapshot V2 | `InvasionSnapshotCodec.cs` | `HCDEAppendInvasionSnapshot` |
| HCDS coop dead spawns | `CoopDeadSpawnsCodec.cs` | `HCDEAppendCoopDeadSpawns` |
| ECHO presentation echo v8 | `PresentationEchoCodec.cs` | `d_net_diag.cpp` presentation echo |
| HCAV event skip walker | `AuthorityEventsCodec.cs` | authority event records in snapshot tail |
| Full tail order walker | `ServerSnapshotTailWalker.cs` | co-op vs invasion post-HCSR tail |
| Guest tail consumption | `LiveSession.cs` | guest receives HCDW+ tail via walker |
| Co-op shipping tail writer | `ServerSnapshotTailCodec.WriteCoopShipping` | HCDW + HCDA + HCDS + ECHO + [HCKS] |
| Minimal tail (HCDW+HCDA+ECHO) | `ServerSnapshotTailCodec.WriteMinimal` | default live snapshot tail |

### HCAV bodies + checksum ring compare (Phase 2c — iteration 8)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Authority event record bodies | `AuthorityEventRecord.cs`, `AuthorityEventsCodec.cs` | `HCDEAppendAuthorityEvents` / `HCDEApplyAuthorityEvents` |
| Replicated actor enums | `AuthorityEventRecord.cs` | `EHCDEReplicatedActorCategory`, `EHCDEReplicatedActorSource` |
| Co-op tail with HCAV | `ServerSnapshotTailCodec.WriteCoopShipping` | HCDW + HCDA + [HCDS] + HCAV + ECHO + [HCKS] |
| Checksum tic ring buffer | `SnapshotChecksumRing.cs` | `TicHashHistory` / `FindTicHashBucket` |
| Client checksum compare | `SnapshotChecksumRing.TryReadAndCompare` | `Net_ChecksumReadAndCompare` |

### ECHO full bodies + checksum mixer (Phase 2c — iteration 9)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| ECHO inventory + player records | `PresentationEchoRecord.cs`, `PresentationEchoCodec.cs` | `HCDEAppendPresentationEcho` / `HCDEReadPresentationEcho` |
| Checksum mix helpers | `SnapshotChecksumMixer.cs` | `MixU32`, `MixDouble`, category hash mixers |
| Checksum compute session | `SnapshotChecksumSession.cs` | `Net_ChecksumComputeIfStale` + `Net_ChecksumApplyServerChunk` |

### Quitter prefix + multi-client authority pump (Phase 2c — iteration 10)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HCSN quitter prefix | `ServerSnapshotQuitterCodec.cs` | `NCMD_QUITTERS` byte list after HCSN header |
| Snapshot builder quitters | `GameplayPayloadBuilders.BuildServerSnapshot` | quitter prefix before HCSR body |
| Guest quitter-aware parse | `LiveGuestSession.TryReceiveServerSnapshot` | skip `quitterBytes` before HCSR |
| Authority client registry | `LiveAuthorityClientRegistry.cs` | tracked live client slots |
| Multi-client authority pump | `LiveAuthoritySession.PumpAllClients` | one gametic, all acked clients |
| Pregame live pump fix | `PregameHost.PumpLiveClients` | single `AdvanceTick` per host pump |

### Multi-player HCSR + admin DEM payloads (Phase 2c — iteration 11)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HCSR duplicate-offset guards | `ServerSnapshotBodyCodec.cs` | parity with `HCDEApplyNativeServerSnapshot` |
| Multi-player/multi-tic tests | `ServerSnapshotBodyCodecTests` | 2 players × 2 command tics round-trip |
| Expanded `DemoCommand` enum | `DemoCommand.cs` | admin/cheat DEM types from `d_protocol.h` |
| Admin DEM canonicalization | `CanonicalEventPayloadCodec.cs` | summon/savegame/addbot/etc. |
| Cross-language netcode gate | `NetcodeCrossLanguageTests` | env-gated soak prerequisites |

### Weapon-slot DEM + guest quitter apply (Phase 2c — iteration 12)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Weapon index canonicalization | `CanonicalWeaponIndexCodec.cs` | `HCDEAppendCanonicalWeaponIndex` |
| SetSlot/AddSlot DEM payloads | `CanonicalEventPayloadCodec.cs` | `DEM_SETSLOT`, `DEM_SETSLOTPNUM`, `DEM_ADDSLOT*` |
| Guest peer slot tracker | `LivePeerSlotTracker.cs` | `DisconnectClient` on quitter broadcast |
| Guest quitter apply | `LiveGuestSession.TryReceiveServerSnapshot` | `NCMD_QUITTERS` prefix before HCSR |
| Snapshot send with quitters | `LiveWire.TrySendServerSnapshot` | authority quitter prefix injection |

### ECHO/HCAV apply stubs (Phase 2c — iteration 13)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Weapon-change policy | `PresentationEchoWeaponChangePolicy.cs` | `HCDEComputeWeaponChangeFlags` |
| ECHO apply session | `PresentationEchoApplySession.cs` | `HCDEReadPresentationEcho` inventory + weapon follow |
| HCAV replay router | `AuthorityEventsApplySession.cs` | `HCDEApplyAuthorityEvents` dispatch table |
| Parsed tail blocks | `ServerSnapshotTailWalker` | full ECHO + HCAV records for apply |
| Guest tail apply hook | `LiveGuestSession.SetApplySinks` | optional sink-driven apply on receive |

### HCSR/HCIN apply sessions (Phase 2c — iteration 14)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Per-player net state | `LivePeerNetRegistry`, `LivePlayerNetState` | `ClientStates[]` sequence/consistency cursors |
| HCSR apply session | `ServerSnapshotApplySession.cs` | `HCDETryApplyNativeServerSnapshotPayload` |
| HCIN apply session | `ClientInputApplySession.cs` | `HCDETryApplyNativeClientInputPayload` |
| Command apply sinks | `IServerSnapshotCommandSink`, `IClientInputCommandSink` | command/event executor hooks |
| Guest/authority wiring | `LiveGuestSession`, `LiveAuthoritySession` | apply on snapshot/input receive |

### World-delta apply stubs (Phase 2c — iteration 15)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Parsed tail blocks | `ServerSnapshotTailWalker` | HCDW/HCDA/HCDS full records exposed |
| HCDW apply session | `WorldDeltaApplySession.cs` | `HCDEValidateServerWorldDeltas` pose validation |
| HCDA apply session | `ActorDeltasApplySession.cs` | `HCDEApplyActorDeltasV2` dispatch |
| HCDS apply session | `CoopDeadSpawnsApplySession.cs` | `HCDEApplyCoopDeadSpawns` retire indices |
| Guest tail apply hook | `LiveGuestSession.SetApplySinks` | world/actor/coop sinks on receive |

### HCIV invasion apply (Phase 2c — iteration 16)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Parsed invasion embedded blocks | `InvasionSnapshotCodec.TryReadBlock` | HCAV/HCDA inside HCIV payload budget |
| Wave monotonic policy | `InvasionSnapshotWavePolicy.cs` | `HCDEApplyInvasionSnapshot` spawn/cleared merge |
| HCIV apply session | `InvasionSnapshotApplySession.cs` | `HCDEApplyInvasionSnapshot` mirror + embedded replay |
| Guest invasion wiring | `LiveGuestSession.SetApplySinks` | invasion sink + capability gate on receive |

### Record bodies + lane headers (Phase 2c — iteration 4)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| 16-byte explicit `usercmd_t` | `UserCmdCodec.cs` | `HCDEAppendUserCmdFields` |
| HCDE event-record block | `EventRecordsCodec.cs` | `HCDEAppendEventRecords` |
| HCIR player-record body | `ClientInputBodyCodec.cs` | `HCDEBuildNativeClientInputPayload` |
| HCSR player-record body | `ServerSnapshotBodyCodec.cs` | `HCDEBuildNativeServerSnapshotPayload` (records section) |
| HCDW / HCAV chunk headers | `LaneChunkHeaderCodec.cs` | world delta + authority event headers |
| Live authority/guest sessions | `LiveSession.cs` | post-pregame live pump glue |
| Guest CLI `--live-ticks` | `HCDE.PregameGuest.Cli` | pregame `Starting` → live guest pump |

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
| Verification error wire round-trip | `VerificationErrorCodecTests` | Pass |
| Host sends `PRE_VERIFICATION_ERROR` on CRC mismatch | `VerificationErrorCodecTests` | Pass |
| Start-game service loopback | `StartGameServiceTests` | Pass |
| Cross-language env gate (optional) | `CrossLanguageIntegrationTests` | Pass (skip when unset) |
| HLIV header golden layout | `LiveHeaderCodecTests` | Pass |
| HGPL envelope round-trip + validation | `GameplayEnvelopeCodecTests` | Pass |
| HCAP capability block (14/18 byte forms) | `LiveControlCapabilitiesTests` | Pass |
| Control payload matches `SendHCDELiveControl` | `LiveControlCapabilitiesTests` | Pass |
| Per-lane sequence independence | `LiveSequenceTrackerTests` | Pass |
| HCIN header layout + validation guards | `ClientInputHeaderCodecTests` | Pass |
| HCSN header layout + length validation | `ServerSnapshotHeaderCodecTests` | Pass |
| Authority/guest routing predicates | `LiveAuthorityRoutingTests` | Pass |
| Live control packet build/parse | `LiveControlPacketTests` | Pass |
| GameID gameplay wire CRC round-trip | `GameplayWireCodecTests` | Pass |
| Empty HCIN/HCSN payload builders | `GameplayPayloadBuilderTests` | Pass |
| UDP live control + empty HCIN loopback | `LiveWireLoopbackTests` | Pass |
| UserCmd 16-byte wire round-trip | `UserCmdCodecTests` | Pass |
| HCIR single-player command body | `ClientInputBodyCodecTests` | Pass |
| HCSR single-player command body | `ServerSnapshotBodyCodecTests` | Pass |
| HCDW/HCAV header layout | `LaneChunkHeaderCodecTests` | Pass |
| Live session client-input exchange | `LiveSessionTests` | Pass |
| Canonical DEM event payload conversion | `CanonicalEventPayloadCodecTests` | Pass |
| Legacy DEM stream → event records | `DemEventStreamConverterTests` | Pass |
| HCDW pose/sector chunk round-trip | `WorldDeltaChunkCodecTests` | Pass |
| HCSR minimal tail (HCDW + HCDA) | `ServerSnapshotTailCodecTests` | Pass |
| HCDA single-record round-trip | `ActorDeltasCodecTests` | Pass |
| HCKS checksum tail block | `ServerSnapshotTailCodecTests` | Pass |
| Extended DEM payload conversion | `CanonicalEventPayloadCodecTests` | Pass |
| Pregame start-game ack → live session | `PregameLiveHandoffTests` | Pass |
| Full snapshot tail walker (co-op + invasion) | `ServerSnapshotTailWalkerTests` | Pass |
| ECHO minimal header round-trip | `PresentationEchoCodecTests` | Pass |
| HCDS coop dead spawns round-trip | `CoopDeadSpawnsCodecTests` | Pass |
| Guest receives tailed server snapshot | `LiveSessionTests` | Pass |
| HCAV authority event record round-trip | `AuthorityEventsCodecTests` | Pass |
| Co-op tail with embedded HCAV block | `AuthorityEventsCodecTests` | Pass |
| Snapshot checksum ring compare | `SnapshotChecksumRingTests` | Pass |
| ECHO full inventory/player round-trip | `PresentationEchoFullCodecTests` | Pass |
| Checksum mixer + compute-if-stale session | `SnapshotChecksumMixerTests` | Pass |
| HCSN quitter prefix round-trip | `ServerSnapshotQuitterCodecTests` | Pass |
| Multi-client authority pump | `LiveSessionTests` | Pass |
| Multi-player HCSR round-trip | `ServerSnapshotBodyCodecTests` | Pass |
| Admin DEM payload canonicalization | `CanonicalEventPayloadCodecTests` | Pass |
| Cross-language netcode gate | `NetcodeCrossLanguageTests` | Pass (skip when unset) |
| Weapon-slot DEM canonicalization | `CanonicalEventPayloadCodecTests` | Pass |
| Weapon index codec round-trip | `CanonicalWeaponIndexCodecTests` | Pass |
| Guest quitter apply on snapshot | `LiveSessionTests` | Pass |
| Peer slot disconnect tracker | `LivePeerSlotTrackerTests` | Pass |
| Weapon-change policy | `PresentationEchoWeaponChangePolicyTests` | Pass |
| ECHO apply session | `PresentationEchoApplySessionTests` | Pass |
| HCAV replay routing | `AuthorityEventsApplySessionTests` | Pass |
| Parsed tail ECHO/HCAV blocks | `ServerSnapshotTailWalkerTests` | Pass |
| HCSR apply session | `ServerSnapshotApplySessionTests` | Pass |
| HCIN apply session | `ClientInputApplySessionTests` | Pass |
| Peer net registry reset | `LivePeerNetRegistryTests` | Pass |
| World-delta apply session | `WorldDeltaApplySessionTests` | Pass |
| Actor delta apply session | `ActorDeltasApplySessionTests` | Pass |
| Coop dead spawns apply | `CoopDeadSpawnsApplySessionTests` | Pass |
| Parsed tail world/actor blocks | `ServerSnapshotTailParsedBlocksTests` | Pass |
| HCIV invasion apply session | `InvasionSnapshotApplySessionTests` | Pass |
| Invasion wave monotonic policy | `InvasionSnapshotWavePolicyTests` | Pass |
| Parsed invasion embedded HCAV/HCDA | `InvasionSnapshotTailParsedBlocksTests` | Pass |

**Test count:** 170 passing (`dotnet test` in `csharp/`).

## Not yet in Phase 2b (sign-off blockers)

| Item | Notes |
| --- | --- |
| Automated cross-language soak in CI | Requires built `hcdeserv` + IWAD + real WAD CRCs in the agent image |
| `HPS_BOOTSTRAP_*` / `HPS_RESYNC_*` runtime late-join services | Not needed for fresh dedicated join |
| Cross-language evidence recorded in audit | Run `pregame_guest_smoke.py` against local `hcdeserv` build |
| Live gameplay lanes (`HGP_*`, `HLANE_*`) | Phase 2c (in progress — wire codecs landed) |

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
- [x] `PRE_VERIFICATION_ERROR` wire codec and host replies
- [x] `HPS_START_GAME` / `HPS_START_GAME_ACK` loopback
- [x] Guest CLI + Python cross-language harness (skips when `hcdeserv`/IWAD absent)
- [ ] C# guest completes pregame admission against shipping C++ `hcdeserv` (harness ready; needs binaries)
- [ ] Principal audit updated with cross-language evidence

## Phase 2c next slice

1. **Cross-language netcode soak** — run `tests/netcode_step12/` against C++ authority/guest when binaries available
2. **Playsim-backed world sinks** — wire `IWorldDeltaApplySink` / `IActorDeltaApplySink` to real pose/actor mutation in Phase 2e
3. **Checksum playsim inputs** — wire `SnapshotChecksumSession` to real world state in Phase 2e
4. **Invasion spawn spot payloads** — decode HCIV embedded spawn metadata beyond header mirror

## Phase 2b next slice

1. **Run cross-language soak** — `python3 csharp/validation/pregame/pregame_guest_smoke.py` with local `hcdeserv` + IWAD CRCs
2. **Bootstrap/resync services** — runtime late-join path

Do **not** port snapshot encode/decode bodies until HCIN/HCSN headers are green.

## Source map

| Concern | C++ | C# (Phase 2) |
| --- | --- | --- |
| UDP sockets | `i_net.cpp` | `HCDE.Net.Transport/UdpTransport.cs` |
| Server info query | `I_QueryServerInfo` | `ServerQueryClient.cs` |
| Pregame constants | `i_net.cpp` | `PregameConstants.cs` |
| Service packet + queue | `BeginHCDEPregameService`, `FHCDEPendingService` | `HCDE.Net.Pregame/*` |
| PRE_CONNECT admission | `TryProcessSetupConnectPacket` | `PregameHost.cs`, `ConnectPacketCodec.cs` |
| Guest join loop | `JoinGame` | `PregameGuest.cs`, `HCDE.PregameGuest.Cli` |
| Live netcode | `d_net*.cpp` | `HCDE.Net.Core/*` (wire codecs; pump not started) |
| RCON server | `d_net_rcon.cpp` | Phase 1 client only; server stays C++ until 2f |

## Audit conclusion (interim)

**Phase 2b C# pregame stack is feature-complete for fresh dedicated joins** — loopback WAITING setup, verification-error replies, start-game, and a cross-language guest CLI/harness are in place. The remaining 2b gate is executing the harness against a real `hcdeserv` build and recording the result.

**Phase 2c iteration 16** adds HCIV invasion apply (`InvasionSnapshotApplySession`, wave monotonic policy, embedded HCAV/HCDA replay) with parsed invasion tail blocks and guest receive wiring.

**Phase 2c iteration 15** adds world-delta apply stubs (HCDW pose validation, HCDA record routing, HCDS spawn retire) with parsed tail blocks and injectable sinks wired into guest receive.

**Phase 2c iteration 14** adds HCSR/HCIN apply sessions with per-player sequence/consistency tracking, idempotent snapshot handling, gap-resync policy, and injectable command sinks wired into guest/authority receive paths.

Next audit checkpoint: **Phase 2c iteration 17** when cross-language `netcode_step12` evidence is recorded or invasion spawn-spot payloads begin.
