# HCDE C# Migration — Phase 2 Principal Audit

**Last updated:** 2026-08-18  
**Status:** In progress — Phase **2c** live protocol codecs (iteration 43 step 1). Phase **2b** verification errors, start-game, bootstrap/resync, and cross-language harness complete. Phase **2b** verification errors, start-game, bootstrap/resync, and cross-language harness complete.  
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

### Netcode Step 12 cross-language soak (Phase 2c — iteration 20)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Shared soak result types | `CrossLanguageSoakTypes.cs` | skip/pass/fail envelope for xUnit runners |
| Netcode cross-language soak | `NetcodeCrossLanguageSoak.cs` | `tests/netcode_step12/netcode_step12_stress.py` invasion smoke |
| Optional client join | `HCDE_HCDE_CLIENT_PATH` env | `--client-count 1` on Step 12 harness |

### Unified map decode + C++ sector metadata parity (Phase 2c/2d — iteration 28)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Unified binary map decode | `BinaryMapDecoder.cs` | aggregates core/geometry/surface/collision decoders |
| Map-load via full decode | `MapLoadBootstrap.cs` | single entry point for WAD→world store |
| C++ sector metadata flags | `d_net.cpp`, `d_net_snapshot_part1.inl` | `HCDEServerWorldDeltaSectorHasLight/Special` |
| Opt-in metadata replicate | `HCDEWorldDeltaReplicateSectorMetadata` | mirrors C# `replicateSectorMetadata` |

### Collision lump decode + sector metadata on HCDW wire (Phase 2c/2d — iteration 27)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| BLOCKMAP/REJECT decode | `CollisionMapLumpCodec.cs`, `BinaryMapCollisionDecoder.cs` | `blockmap_t`, `rejectmatrix` in `maploader.cpp` |
| Sector light/special wire | `SectorWorldDelta`, `WorldDeltaPoseCodec` | HCDE extension beyond C++ floor/ceiling flags |
| Authority metadata replicate | `LiveAuthoritySession.SetAuthorityWorldState(replicateSectorMetadata)` | guests without local WAD bootstrap |
| Guest-without-WAD E2E | `MapLoadBootstrapIntegrationTests` | HCDW metadata + HCKS checksum match |

### Surface lump decode + map-load bootstrap E2E (Phase 2c/2d — iteration 26)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| SIDEDEFS/SSECTORS decode | `SurfaceMapLumpCodec.cs`, `BinaryMapSurfaceDecoder.cs` | `mapsidedef_t`, `mapsubsector_t` |
| Map-load world store seed | `MapLoadBootstrap.cs` | `p_setup.cpp` sector init before playsim |
| Authority map-bootstrap E2E | `MapLoadBootstrapIntegrationTests` | authority HCDW+HCKS with map-seeded stores |

### Geometry lump decode + map sector bootstrap (Phase 2c/2d — iteration 25)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| VERTEXES/SEGS/NODES decode | `GeometryMapLumpCodec.cs`, `BinaryMapGeometryDecoder.cs` | `mapvertex_t`, `seg_t`, `node_t` |
| Sector bootstrap helper | `MapSectorBootstrap.cs` | `mapsector_t` read path before playsim |
| Guest world store seed | `GuestWorldStateBootstrap.cs`, `GuestWorldStateStore.SeedMapSector` | Phase 2e bridge: map sectors → HCDW checksum inputs |
| Cross-language soak CI | `.github/workflows/csharp-cross-language-soak.yml` | optional pregame + Step 12 soak when secrets set |

### Binary map lump decode + authority HCDW tail (Phase 2c/2d — iteration 24)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| THINGS/LINEDEFS/SECTORS decode | `BinaryMapLumpCodec.cs`, `BinaryMapLumpDecoder.cs` | `mapthing_t`, `maplinedef_t`, `mapsector_t` |
| World-state tail builder | `WorldStateTailBuilder.cs` | HCDW coop tail from `GuestWorldStateStore` |
| Authority HCDW send | `LiveAuthoritySession.SendToClient` | outbound snapshots embed world deltas + HCKS |

### WAD map lump catalog (Phase 2c/2d — iteration 23)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| WAD directory reader | `WadArchiveReader.cs` | `wadinfo_t` / `wadlump_t` in `file_wad.cpp` |
| Map lump catalog | `MapLumpCatalogReader.cs` | `ML_*` lump order in `doomdata.h` |
| UDMF probe | `UdmfMapProbe.cs` | `TEXTMAP` / `namespace` prefix in `udmf.cpp` |
| Authority checksum send | `LiveAuthoritySession.SetAuthorityWorldState` | outbound HCKS from authority world store |

### Guest world state wiring (Phase 2c — iteration 22)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Guest world state hook | `LiveGuestSession.SetGuestWorldState` | HCDW/HCDA apply + HCKS compute on receive |
| External tail snapshot build | `BuildServerSnapshotSinglePlayerWithExternalTail` | HCSR + custom coop/invasion tail |
| Sector-only world delta apply | `LiveGuestSession.TryApplyTailSections` | sector deltas without pose records |
| Phase 2d scaffold | `HCDE.MapLoader` | `maploader/`, `p_setup.cpp` entry point |
| CI gate | `.github/workflows/csharp.yml` | `dotnet test` on `csharp/` pushes |

### Playsim stub world state + soak evidence (Phase 2c — iteration 21)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| In-memory guest world store | `GuestWorldStateStore.cs` | Phase 2e bridge implementing `IWorldDeltaApplySink` / `IActorDeltaApplySink` |
| Checksum inputs from store | `SnapshotChecksumPlaysimInputs.cs` | `SnapshotChecksumSession` compute fed from applied HCDW/HCDA state |
| Cross-language soak evidence | `CrossLanguageSoakEvidence.cs` | JSON audit trail when `HCDE_SOAK_EVIDENCE_DIR` is set |

### Cross-language pregame soak + checksum tail (Phase 2c — iteration 19)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Pregame cross-language soak runner | `PregameCrossLanguageSoak.cs` | `csharp/validation/pregame/pregame_guest_smoke.py` |
| Snapshot checksum tail on send | `GameplayPayloadBuilders`, `LiveWire.TrySendServerSnapshot` | HCKS block appended after ECHO in minimal tail |
| Guest checksum integration | `GuestChecksumApplyIntegrationTests` | HCKS tail compare via `SetChecksumSession` on receive |
| Full snapshot + HCKS round-trip | `ServerSnapshotChecksumTailTests` | `ServerSnapshotTailWalker` after HCSR + tail |

### Bootstrap/resync services (Phase 2c — iteration 18)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Bootstrap control payload | `BootstrapControlPayload`, `PregameServicePayloads` | `QueueHCDEBootstrapControlService` 13-byte body |
| Runtime join bootstrap | `PregameHost` + `AdmitAsRuntimeJoin` | `HPS_BOOTSTRAP_BEGIN` / `HPS_BOOTSTRAP_ACK` |
| Guest resync lane | `PregameGuest.RequestResync` | `HPS_RESYNC_REQUEST` / `HPS_RESYNC_BEGIN` / `HPS_RESYNC_ACK` |

### Invasion spawn directory + checksum apply (Phase 2c — iteration 17)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Spawn directory parse | `InvasionSpawnDirectoryCodec.cs` | `InvasionSpawnDirectory` header mirror fields |
| Spawn directory apply | `IInvasionSnapshotApplySink.ApplySpawnDirectory` | `HCDEApplyInvasionSnapshot` spawn metadata |
| Checksum compare apply | `SnapshotChecksumApplySession.cs` | `Net_ChecksumReadAndCompare` |
| Guest checksum wiring | `LiveGuestSession.SetChecksumSession` | HCKS tail compare on receive |

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
| Invasion spawn directory parse | `InvasionSpawnDirectoryCodecTests` | Pass |
| Invasion spawn count validation | `InvasionSnapshotApplySessionTests` | Pass |
| Snapshot checksum apply session | `SnapshotChecksumApplySessionTests` | Pass |
| Bootstrap control payload | `BootstrapControlPayloadTests` | Pass |
| Runtime bootstrap loopback | `BootstrapResyncLoopbackTests` | Pass |
| Guest resync request loopback | `BootstrapResyncLoopbackTests` | Pass |
| Pregame cross-language soak runner | `PregameCrossLanguageSoakTests` | Pass (skip when unset) |
| Server snapshot HCKS tail build | `ServerSnapshotChecksumTailTests` | Pass |
| Guest checksum mismatch on receive | `GuestChecksumApplyIntegrationTests` | Pass |
| Netcode Step 12 cross-language soak | `NetcodeCrossLanguageSoakTests` | Pass (skip when unset) |
| Guest world state store | `GuestWorldStateStoreTests` | Pass |
| Checksum inputs from world store | `SnapshotChecksumPlaysimInputsTests` | Pass |
| Soak evidence JSON writer | `CrossLanguageSoakEvidenceTests` | Pass |
| Guest world state checksum E2E | `GuestWorldStateChecksumIntegrationTests` | Pass |
| Map loader scaffold | `MapLoaderConstantsTests` | Pass |
| WAD directory parse | `WadArchiveReaderTests` | Pass |
| Map lump catalog + UDMF probe | `MapLumpCatalogReaderTests` | Pass |
| Authority outbound HCKS | `AuthorityWorldStateChecksumIntegrationTests` | Pass |
| Binary map lump decode | `BinaryMapLumpDecoderTests` | Pass |
| World-state tail builder | `WorldStateTailBuilderTests` | Pass |
| Authority→guest HCDW+HCKS E2E | `AuthorityWorldStateChecksumIntegrationTests` | Pass |

**Test count:** 196 passing (`dotnet test` in `csharp/`).

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

## LOC migration ledger (2026-08-15)

| Tree | LOC | Migration intent |
| --- | ---: | --- |
| `csharp/src/` | **~14,550** | C# delivered so far |
| `src/` (engine) | **~659,000** | Primary migration target |
| `tools/` | **~13,300** | Partial (master/rcon ported; build tools stay) |
| `libraries/` (vendored) | **~891,000** | Stay native / P/Invoke |
| **HCDE-owned C++ remaining** | **~672,000** | `src/` + non-vendored `tools/` |

**Progress by LOC:** C# is ~2.2% of HCDE-owned C++ surface area. Wire/protocol layers mirror ~55–60% of `d_net` message surface but ~0% of playsim execution.

### BEHAVIOR lump probe (Phase 2c/2d — iteration 29 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| ACS BEHAVIOR probe | `MapBehaviorCodec.cs`, `BinaryMapBehaviorDecoder.cs` | `FBehavior::Init` ACS magic in `p_acs.cpp` |
| Catalog BEHAVIOR lump | `MapLumpNames.BinaryMapLumpOrder` | `ML_BEHAVIOR` in `doomdata.h` |
| Unified decode extension | `BinaryMapDecoder` + `BinaryMapBehavior` | optional Hexen scripts entry point |

### Authority map-load hook (Phase 2c/2d — iteration 29 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Authority WAD bootstrap | `AuthorityMapLoadBootstrap.cs` | `p_setup.cpp` map load before net pump |
| Map-load E2E | `AuthorityMapLoadBootstrapTests` | authority seeds store + metadata replicate + guest receive |

### Cross-language soak suite (Phase 2c — iteration 29 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Combined soak runner | `CrossLanguageSoakSuite.cs` | pregame guest smoke + Step 12 invasion harness |
| Evidence recording | `CrossLanguageSoakEvidence.Finalize` | JSON audit trail via `HCDE_SOAK_EVIDENCE_DIR` |
| Suite tests | `CrossLanguageSoakSuiteTests` | skip-when-unconfigured + evidence file count |

### BEHAVIOR script directory decode (Phase 2c/2d — iteration 30 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| ACS script directory | `MapBehaviorDirectoryCodec.cs` | `FBehavior::LoadScriptsDirectory` |
| Script entry record | `MapBehaviorScriptEntry` | `ScriptPtr`, `ScriptPtr1`, `ScriptPtr2`, `ScriptPtr3` |
| Unified behavior decode | `BinaryMapBehavior.Scripts` | `BinaryMapBehaviorDecoder` after ACS probe |

## Phase 2c next slice (iteration 34 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — HUD message / inventory direct specials (`MoreHudMessage`, `Lspec*DirectB`)
2. ~~**HCDE.Server playsim pump**~~ — wire `LiveAuthoritySession` tick after map-load bootstrap
3. ~~**Cross-language soak gate**~~ — require Passed evidence in release checklist when secrets configured

## Phase 2c next slice (iteration 35 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — continue ZDoom/Skulltag PCD table (music/sound direct, more stack ops)
2. ~~**Authority playsim tick polish**~~ — snapshot tail builders on authority pump, guest apply wiring
3. ~~**Cross-language Passed soak gate**~~ — document release checklist + enforce in main CI when secrets configured

## Phase 2c next slice (iteration 36 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — gravity/air-control direct, more ZDoom stack ops
2. ~~**Authority playsim tick polish**~~ — player pose deltas on authority pump, guest checksum mismatch policy
3. ~~**Cross-language soak evidence**~~ — re-record Passed manifest in CI and commit refreshed templates

## Phase 2c next slice (iteration 37 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — call/discard stack ops, more global array PCDs
2. ~~**Authority playsim tick polish**~~ — actor delta tails on authority pump, guest resync on mismatch
3. ~~**Cross-language soak evidence**~~ — automate committing refreshed Passed templates from CI artifacts

## Phase 2c next slice (iteration 38 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — map/world array PCDs, translation range ops
2. ~~**Authority playsim tick polish**~~ — coop dead-spawn tails on authority pump, guest gap resync wiring
3. ~~**Cross-language soak evidence**~~ — weekly Passed template commit automation from artifact bundle

## Phase 2c next slice (iteration 39 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — script array PCDs, more ZDoom stack ops
2. ~~**Authority playsim tick polish**~~ — invasion tail on authority pump, guest presentation echo apply wiring
3. ~~**Cross-language soak evidence**~~ — manifest staleness gate for committed templates

## Phase 2c next slice (iteration 40 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — script char-range PCDs, more Eternity stack ops
2. ~~**Authority playsim tick polish**~~ — authority event tails on pump, guest invasion apply wiring
3. ~~**Cross-language soak evidence**~~ — evidence file freshness gate for committed templates

## Phase 2c next slice (iteration 41 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — more ZDoom/Skulltag PCD table entries, enhanced-format operand coverage
2. ~~**Authority playsim tick polish**~~ — invasion spawn-directory apply on guest pump, authority checksum tail polish
3. ~~**Cross-language soak evidence**~~ — evidence staleness enforcement in main CI workflow

## Phase 2c next slice (iteration 42 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — actor property/getter PCDs, more enhanced-format direct specials
2. ~~**Authority playsim tick polish**~~ — embedded HCIV authority events on guest apply, coop+invasion tail merge policy
3. ~~**Cross-language soak evidence**~~ — stale evidence rejection in weekly template commit workflow

## Phase 2c next slice (iteration 43)

1. ~~**BEHAVIOR bytecode operands**~~ — actor inventory PCDs, enhanced-format translation operands
2. **Authority playsim tick polish** — invasion embedded HCDA apply on guest pump, authority tail checksum mismatch polish
3. **Cross-language soak evidence** — manifest+evidence dual staleness in weekly commit gate

### BEHAVIOR actor inventory + translation PCD operands (Phase 2c/2d — iteration 43 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Actor inventory opcodes | `AcsPcode.cs` | `PCD_CLEARACTORINVENTORY`…`PCD_CHECKACTORINVENTORY` |
| Translation range opcodes | `AcsPcode.cs` | `PCD_TRANSLATIONRANGE3`…`PCD_TRANSLATIONRANGE5` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | old + little-enhanced actor-inventory/translation skips |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsActorInventoryOps`, `TryWalkScript_LittleEnhanced_ReadsTranslationRangeOps` |

### Stale export bundle rejection (Phase 2c — iteration 42 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Bundle freshness gate | `CrossLanguageSoakGate.EvaluateExportBundleEvidenceFreshness` | reject stale/missing export evidence before apply |
| Apply integration | `CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates` | throws when bundle evidence is stale |
| CI apply workflow | `.github/workflows/csharp-cross-language-soak.yml` | stale bundle test before weekly commit |
| Archive tests | `CrossLanguageSoakEvidenceArchiveTests` | `ApplyExportedTemplates_RejectsStaleBundleEvidence` |
| Release checklist | `validation/soak/README.md` | weekly apply stale-bundle docs |

### Embedded HCIV authority events + coop/invasion merge (Phase 2c — iteration 42 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HCIV writer | `InvasionSnapshotCodec.WriteV2` | embedded HCAV/HCDA inside invasion payload |
| Merge policy | `WorldStateTailMergePolicy.ShouldMergeCoopIntoInvasion` | ship HCDW+HCDS with HCIV when store has payload |
| Merged tail builder | `WorldStateTailBuilder.TryBuildMergedInvasionCoopTail` | coop world delta + embedded HCAV in invasion tail |
| Guest authority apply | `GuestAuthorityEventState` | auto-wire `IAuthorityEventSink` on `SetGuestWorldState` |
| Authority pump send | `LiveAuthoritySession.SendToClient` | merged invasion/coop tail selection |
| E2E tests | `LiveSessionTests`, `WorldStateTailBuilderTests` | embedded HCAV apply + merged HCDW/HCIV pump |

### BEHAVIOR actor property/getter + enhanced direct PCD operands (Phase 2c/2d — iteration 42 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Actor getter opcodes | `AcsPcode.cs` | `PCD_GETACTORZ`…`PCD_GETACTORCEILINGZ` |
| Actor property opcodes | `AcsPcode.cs` | `PCD_SETACTORPROPERTY`, `PCD_GETACTORPROPERTY` |
| Enhanced direct specials | `AcsPcode.cs` | `PCD_SETGRAVITYDIRECTB`, `PCD_SETAIRCONTROLDIRECTB` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | old + little-enhanced actor/direct skips |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_*_ReadsActorPropertyAndGetterOps`, `ReadsDirectSpecialOps` |

### Evidence staleness CI enforcement (Phase 2c — iteration 41 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Main CI gate | `.github/workflows/csharp.yml` | `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` on soak gate step |
| Soak CI gate | `.github/workflows/csharp-cross-language-soak.yml` | dedicated evidence freshness enforce step |
| Gate tests | `CrossLanguageSoakGateTests` | `Evaluate_EnforcesEvidenceFreshnessWhenSecretsConfigured` |
| Release checklist | `validation/soak/README.md` | main + soak workflow env docs |

### Invasion spawn-directory + checksum tail polish (Phase 2c — iteration 41 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Checksum helper | `WorldStateTailBuilder.TryComputeChecksumHashes` | shared HCKS hash compute for authority tails |
| Invasion tail builder | `WorldStateTailBuilder.TryBuildInvasionTailWithChecksum` | HCIV + HCKS when world store wired |
| Authority pump send | `LiveAuthoritySession.SendToClient` | invasion pump uses checksum helper |
| Guest invasion apply | `GuestInvasionState.ApplySpawnDirectory` | V2 spawn metadata on guest receive |
| E2E tests | `LiveSessionTests`, `WorldStateTailBuilderTests` | spawn directory apply + invasion HCKS pump |

### BEHAVIOR inventory/global-array + enhanced PCD operands (Phase 2c/2d — iteration 41 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Inventory opcodes | `AcsPcode.cs` | `PCD_TAKEINVENTORY`, `PCD_CHECKINVENTORY` |
| Skulltag player opcodes | `AcsPcode.cs` | `PCD_ISNETWORKGAME`…`PCD_PLAYERHEALTH` |
| Global array opcodes | `AcsPcode.cs` | `PCD_SUBGLOBALARRAY`…`PCD_DECGLOBALARRAY` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | old + little-enhanced inventory/global-array skips |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_*_ReadsInventoryAndGlobalArrayOps` |

### Evidence file freshness gate (Phase 2c — iteration 40 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Evidence file reader | `CrossLanguageSoakManifest.TryReadHarnessEvidenceFiles` | `manifest.json` `EvidenceFile` entries |
| Freshness policy | `CrossLanguageSoakGate.EvaluateEvidenceFreshness` | reject evidence older than max age |
| Gate integration | `CrossLanguageSoakGate.Evaluate` | evidence check after manifest staleness |
| Gate tests | `CrossLanguageSoakGateTests` | missing + stale + fresh evidence coverage |
| Release checklist | `validation/soak/README.md` | `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` docs |

### Authority event tail + guest invasion apply (Phase 2c — iteration 40 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Authority event queue | `GuestWorldStateStore.QueueAuthorityEvent` | `HCDEAppendAuthorityEvents` pending list |
| Tail builder | `WorldStateTailBuilder.WriteCoopTailFromStore` | ships HCAV from pending authority events |
| Guest invasion state | `GuestInvasionState` | `HCDEApplyInvasionSnapshot` mirror apply |
| Guest wiring | `LiveGuestSession.SetGuestWorldState` | auto-wire `IInvasionSnapshotApplySink` |
| E2E tests | `LiveSessionTests`, `WorldStateTailBuilderTests` | HCAV pump + HCIV invasion apply |

### BEHAVIOR script char-range + Eternity stack PCD operands (Phase 2c/2d — iteration 40 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Char-range opcodes | `AcsPcode.cs` | `PCD_PRINTMAPCHARARRAY`…`PCD_STRCPYTOSCRIPTCHARARRAY` |
| Eternity stack opcodes | `AcsPcode.cs` | `PCD_CALLFUNC`, `PCD_SAVESTRING`, `PCD_SCRIPTWAITNAMED` |
| Script array logic ops | `AcsPcode.cs` | `PCD_ANDSCRIPTARRAY`…`PCD_ORSCRIPTARRAY` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | 2-word `CallFunc`, stack-only char-range ops |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsScriptCharRangeAndEternityOps` |

### Manifest staleness gate (Phase 2c — iteration 39 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Manifest reader | `CrossLanguageSoakManifest.TryReadRecordedAtUtc` | `validation/soak/manifest.json` freshness |
| Staleness policy | `CrossLanguageSoakGate.EvaluateManifestStaleness` | reject manifests older than max age |
| Gate integration | `CrossLanguageSoakGate.Evaluate` | staleness check after Passed status |
| Gate tests | `CrossLanguageSoakGateTests` | stale + fresh manifest coverage |
| Release checklist | `validation/soak/README.md` | `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` docs |

### Invasion tail + guest presentation echo (Phase 2c — iteration 39 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Invasion tail writer | `ServerSnapshotTailCodec.WriteInvasionShipping` | `HCDEAppendInvasionSnapshot` shipping tail |
| Tail builder | `WorldStateTailBuilder.TryBuildInvasionTail` | HCIV before ECHO on authority pump |
| Authority pump send | `LiveAuthoritySession.SetAuthorityInvasionSnapshot` | outbound invasion snapshots |
| Guest echo state | `GuestPresentationEchoState` | `HCDEReadPresentationEcho` inventory/weapon apply |
| Guest wiring | `LiveGuestSession.SetGuestWorldState` | auto-wire `IPresentationEchoApplySink` |
| E2E tests | `LiveSessionTests`, `WorldStateTailBuilderTests` | invasion HCIV pump + echo apply |

### BEHAVIOR script-array + stack PCD operands (Phase 2c/2d — iteration 39 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Script array opcodes | `AcsPcode.cs` | `PCD_PUSHSCRIPTARRAY`…`PCD_DECSCRIPTARRAY` |
| Stack opcodes | `AcsPcode.cs` | `PCD_PUSHFUNCTION`, `PCD_CALLSTACK`, `PCD_GOTOSTACK` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | 1-word script array indices + stack-only ops |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsScriptArrayAndStackOps` |

### Weekly soak template apply + commit (Phase 2c — iteration 38 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Apply helper | `CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates` | copy CI artifact bundle into `validation/soak/` |
| CI apply step | `.github/workflows/csharp-cross-language-soak.yml` | `HCDE_APPLY_SOAK_TEMPLATES=1` after export |
| CI commit step | `.github/workflows/csharp-cross-language-soak.yml` | weekly auto-commit refreshed templates |
| Archive tests | `CrossLanguageSoakEvidenceArchiveTests` | apply bundle + env-gated CI apply |
| Release checklist | `validation/soak/README.md` | weekly apply + commit workflow docs |

### Coop dead-spawn tail + guest gap resync (Phase 2c — iteration 38 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Dead-spawn queue | `GuestWorldStateStore.QueueCoopDeadSpawn` | `HCDEAppendCoopDeadSpawns` authority retire list |
| Tail builder | `WorldStateTailBuilder.WriteCoopTailFromStore` | ships HCDS from pending authority store indices |
| Gap resync flags | `ServerSnapshotApplyResult`, `ClientInputApplyResult` | `TryResyncSnapshotGap` / `TryResyncInputGap` |
| Guest wiring | `LiveGuestSession.NeedsNetGapResync` | reset guest net registry on snapshot gap resync |
| Authority wiring | `LiveAuthoritySession.TryReceiveClientInput` | reset client registry on input gap resync |
| E2E tests | `LiveSessionTests`, `WorldStateTailBuilderTests`, `SnapshotAndInputApplyTests` | HCDS pump apply + gap resync reporting |

### BEHAVIOR map/world array + translation PCD operands (Phase 2c/2d — iteration 38 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Translation opcodes | `AcsPcode.cs` | `PCD_STARTTRANSLATION`, `PCD_TRANSLATIONRANGE1`…`5`, `PCD_ENDTRANSLATION` |
| Map/world array opcodes | `AcsPcode.cs` | `PCD_PUSHMAPARRAY`…`PCD_DECMAPARRAY`, `PCD_PUSHWORLDARRAY`…`PCD_DECWORLDARRAY` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | stack-only translation ops + 1-word map/world array indices |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsMapWorldArrayAndTranslationOps` |

### Soak template export bundle (Phase 2c — iteration 37 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Export helper | `CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates` | CI artifact bundle for commit |
| CI export step | `.github/workflows/csharp-cross-language-soak.yml` | `HCDE_SOAK_TEMPLATE_EXPORT_DIR` |
| Archive tests | `CrossLanguageSoakEvidenceArchiveTests` | export + CI bundle coverage |
| Release checklist | `validation/soak/README.md` | artifact copy instructions |

### Actor delta tail + checksum resync (Phase 2c — iteration 37 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Actor seed from map | `GuestWorldStateBootstrap.SeedPlayersFromMapThings` | player-start actors in HCDA tail |
| Tail builder | `WorldStateTailBuilder.WriteCoopTailFromStore` | ships `ActorDeltaRecord` from authority store |
| Resync policy | `SnapshotChecksumMismatchPolicy.ResyncNetStateOnMismatch` | reset guest net registry on HCKS mismatch |
| Guest state | `LiveGuestSession.NeedsChecksumResync` | post-snapshot mismatch resync flag |
| E2E tests | `LiveSessionTests`, `GuestWorldStateChecksumIntegrationTests` | actor HCDA apply + resync on mismatch |

### BEHAVIOR call/global-array PCD operands (Phase 2c/2d — iteration 37 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Call stack opcodes | `AcsPcode.cs` | `PCD_CALL`, `PCD_CALLDISCARD`, `PCD_RETURNVOID`, `PCD_RETURNVAL` |
| Global array opcodes | `AcsPcode.cs` | `PCD_PUSHGLOBALARRAY`, `PCD_ASSIGNGLOBALARRAY`, `PCD_ADDGLOBALARRAY` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | call byte arg + global array index |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsCallDiscardAndGlobalArrayOps` |

### Passed soak evidence refresh (Phase 2c — iteration 36 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Passed refresh helper | `CrossLanguageSoakEvidenceArchive.TryRecordPassedValidationEvidence` | refresh + gate evaluate |
| CI verify step | `.github/workflows/csharp-cross-language-soak.yml` | `HCDE_RECORD_PASSED_VALIDATION_EVIDENCE=1` |
| Archive tests | `CrossLanguageSoakEvidenceArchiveTests` | Passed gate when binaries present |
| Release checklist | `validation/soak/README.md` | commit refreshed `validation/soak/` tree |

### Player pose tail + checksum policy (Phase 2c — iteration 36 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Player seed from map | `GuestWorldStateBootstrap.SeedPlayersFromMapThings` | `p_setup.cpp` player starts |
| Map-load bootstrap | `MapLoadBootstrap.TrySeedGuestWorldState` | seeds sectors + player poses |
| Mismatch policy | `SnapshotChecksumMismatchPolicy` | guest HCKS compare policy |
| Guest apply state | `LiveGuestSession.LastChecksumApplyState` | post-snapshot checksum result |
| E2E tests | `LiveSessionTests`, `GuestWorldStateChecksumIntegrationTests` | player pose tail + mismatch report |

### BEHAVIOR gravity/global PCD operands (Phase 2c/2d — iteration 36 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Gravity/air-control opcodes | `AcsPcode.cs` | `PCD_SETGRAVITY`, `PCD_SETAIRCONTROLDIRECT` |
| Global var stack ops | `AcsPcode.cs` | `PCD_PUSHGLOBALVAR`, `PCD_ASSIGNGLOBALVAR` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | direct gravity/air-control + global var indices |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsGravityAirControlAndGlobalVarOps` |

### Passed soak gate in main CI (Phase 2c — iteration 35 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| CI enforce helper | `CrossLanguageSoakGate.ShouldEnforceInCi` | `HCDE_ENFORCE_SOAK_GATE=1` contract |
| Main CI step | `.github/workflows/csharp.yml` | gate test when soak secrets configured |
| Gate tests | `CrossLanguageSoakGateTests` | `ShouldEnforceInCi_ReturnsTrueWhenEnvSet` |
| Release checklist | `validation/soak/README.md` | main CI + soak workflow gate docs |

### Authority snapshot tail on pump (Phase 2c — iteration 35 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Tail build helper | `WorldStateTailBuilder.TryBuildCoopTailFromStore` | `HCDEServerSnapshotAppendWorldDelta` shipping tail |
| Authority pump send | `LiveAuthoritySession.SendToClient` | external HCDW tail when world store seeded |
| Pump E2E | `LiveSessionTests` | `AuthorityPump_SendsWorldStateTailViaPump` |
| Host E2E | `DedicatedServerHostTests` | guest sector apply after `hcdeserv` pump |

### BEHAVIOR music/stack PCD operands (Phase 2c/2d — iteration 35 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Music opcodes | `AcsPcode.cs` | `PCD_MUSICCHANGE`, `PCD_SETMUSIC`, `PCD_LOCALSETMUSICDIRECT` |
| Stack push opcodes | `AcsPcode.cs` | `PCD_PUSHBYTE`, `PCD_PUSHBYTES`, `PCD_PUSH2BYTES`…`PCD_DUP`/`PCD_SWAP` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | music direct specials + variable `PushBytes` skip |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | music direct + little-enhanced push-byte fixtures |

### Cross-language Passed soak gate (Phase 2c — iteration 34 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Manifest gate | `CrossLanguageSoakGate.cs` | require `Passed` per harness when soak secrets set |
| Gate tests | `CrossLanguageSoakGateTests` | NotRequired / Passed / Skipped-manifest failure |
| CI enforce step | `.github/workflows/csharp-cross-language-soak.yml` | `HCDE_ENFORCE_SOAK_GATE=1` after evidence refresh |
| Release checklist | `validation/soak/README.md` | gate evaluation + CI invocation |

### LiveAuthoritySession tick pump (Phase 2f — iteration 34 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Authority pump | `LiveAuthoritySession.Pump` | `HCDEAuthorityPumpLiveClients` receive + tick advance |
| Host wiring | `DedicatedServerHost.Pump` | `_liveSession.Pump()` after map-load bootstrap |
| Client sync | `DedicatedServerHost.SyncLiveClients` | track guests with `HasStartGameAck` |
| Pump tests | `LiveSessionTests` | `AuthorityPump_ReceivesClientInputAndAdvancesGameTic` |
| Host E2E | `DedicatedServerHostTests` | bootstrap live session + guest input pump |

### BEHAVIOR HUD/inventory PCD operands (Phase 2c/2d — iteration 34 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| HUD message opcodes | `AcsPcode.cs` | `PCD_MOREHUDMESSAGE`, `PCD_OPTHUDMESSAGE`, `PCD_ENDHUDMESSAGE` |
| Direct-byte specials | `AcsPcode.cs` | `PCD_LSPEC1DIRECTB`…`PCD_LSPEC5DIRECTB`, `PCD_DELAYDIRECTB`, `PCD_RANDOMDIRECTB` |
| Inventory opcodes | `AcsPcode.cs` | `PCD_GIVEINVENTORY`, `PCD_CLEARINVENTORY`, `PCD_SETFONTDIRECT` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | HUD stack pops + `Lspec*DirectB` byte args |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | `TryWalkScript_OldFormat_ReadsHudMessageAndDirectByteSpecials` |

### BEHAVIOR print stack + direct specials (Phase 2c/2d — iteration 33 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Print stack opcodes | `AcsPcode.cs` | `PCD_PRINTNUMBER`, `PCD_PRINTCHARACTER`, `PCD_PRINTNAME` |
| Logical/bitwise range | `MapBehaviorBytecodeWalker.cs` | `PCD_ORLOGICAL`…`PCD_RSHIFT` |
| Direct specials | `MapBehaviorBytecodeWalker.cs` | `GiveInventoryDirect`, `ConsoleCommandDirect`, `SpawnDirect` |
| Walk tests | `MapBehaviorBytecodeWalkerTests` | print stack + direct-special bytecode |

### HCDE.Server master CLI polish (Phase 2f — iteration 33 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| CLI parser | `DedicatedServerCommandLine.cs` | `--master`, `--server-name`, `--skill`, `--no-query` |
| Query snapshot fields | `DedicatedServerHost.BuildQuerySnapshot` | `BuildServerQuerySnapshot` player rows + game mode |
| CLI tests | `DedicatedServerCommandLineTests` | master host/port + public query fields |

### Committed soak evidence refresh (Phase 2c — iteration 33 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Evidence prune + refresh | `CrossLanguageSoakEvidenceArchive.RefreshCommittedEvidence` | replace stale harness JSON under `validation/soak/evidence/` |
| Template refresh test | `CrossLanguageSoakEvidenceArchiveTests` | `HCDE_REFRESH_SOAK_TEMPLATES=1` |
| CI template upload | `.github/workflows/csharp-cross-language-soak.yml` | refresh step + committed manifest/evidence artifacts |

## Phase 2c next slice (iteration 33 — delivered)

1. ~~**BEHAVIOR bytecode operands**~~ — continue ZDoom/Skulltag PCD table (print stack, more direct specials)
2. ~~**HCDE.Server master advertise polish**~~ — CLI flags for `--master`, public query snapshot fields
3. ~~**Commit Passed soak evidence**~~ — refresh `validation/soak/evidence/` templates when CI secrets run green

### BEHAVIOR bytecode operands (Phase 2c/2d — iteration 32 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Expanded PCD enum | `AcsPcode.cs` | `p_acs.cpp` ZDoom/Skulltag opcodes through `Lspec6` |
| Operand skip table | `MapBehaviorBytecodeWalker.cs` | `IfNotGoto`, `CaseGoto`, `ScriptWaitDirect`, `PlayerCount`… |
| Bytecode fixture helper | `TestWadBuilder.BuildWordBytecode` | flat int32 script body writer for tests |
| Operand tests | `MapBehaviorBytecodeWalkerTests` | `IfNotGoto` + `Lspec6` walk coverage |

### HCDE.Server query/advertise (Phase 2f — iteration 32 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Raw inbound interceptor | `IPregameInboundInterceptor`, `PregameHost.DrainInbound` | `TryHandleServerQuery` before setup decode |
| Query responder | `DedicatedServerQueryResponder.cs` | `SendLauncherInfo` / `LAUNCHER_CHALLENGE` |
| Master heartbeat | `DedicatedServerAdvertiser.cs` | `MasterPackets.CreateServerHeartbeat` |
| Host wiring | `DedicatedServerHost.cs` | query snapshot + optional master advertise pump |
| Query client bind fix | `ServerQueryClient.cs` | ephemeral bind before loopback receive |
| Server tests | `DedicatedServerQueryResponderTests`, `DedicatedServerHostTests` | launcher query + master heartbeat |

### Passed soak evidence in CI (Phase 2c — iteration 32 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Validation recorder | `CrossLanguageSoakEvidenceArchive.RecordValidationEvidence` | re-record when binaries present |
| Passed-path test | `CrossLanguageSoakEvidenceArchiveTests.RecordValidationPassedEvidence_WhenRequested` | `_Passed.json` + manifest status |
| CI re-record step | `.github/workflows/csharp-cross-language-soak.yml` | runs when `HCDE_HCDESERV_PATH` + `HCDE_IWAD_PATH` secrets set |

### BEHAVIOR bytecode walk (Phase 2c/2d — iteration 31 step 1)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| ACS P-code enum | `AcsPcode.cs` | `p_acs.cpp` Hexen/Skulltag opcode table |
| Bytecode walker | `MapBehaviorBytecodeWalker.cs` | `NEXTWORD` / little-enhanced decode loop |
| Script bodies on decode | `BinaryMapBehavior.ScriptBodies` | per-script instruction disassembly |

### HCDE.Server scaffold (Phase 2f — iteration 31 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Dedicated host pump | `DedicatedServerHost.cs` | `d_main.cpp` dedicated path (scaffold) |
| `hcdeserv` CLI | `HCDE.Server/Program.cs` | `--iwad`, `--map`, `--port` bootstrap |
| Server E2E | `DedicatedServerHostTests` | pregame handoff → live session + guest snapshot |

### Soak manifest + CI artifacts (Phase 2c — iteration 31 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Harness manifest | `CrossLanguageSoakManifest.cs` | `validation/soak/manifest.json` status rollup |
| CI evidence upload | `.github/workflows/csharp-cross-language-soak.yml` | artifact upload for Passed/Skipped JSON |
| Manifest tests | `CrossLanguageSoakEvidenceArchiveTests` | manifest + evidence file pairing |

### Dedicated server map-load integration (Phase 2c/2f — iteration 30 step 2)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Bootstrapped live handoff | `PregameHost.TryCreateBootstrappedLiveAuthoritySession` | `p_setup.cpp` before `I_NetDone` live pump |
| Pregame map-load E2E | `PregameMapLoadBootstrapTests` | pregame start-game ack → authority HCDW+HCKS |

### Cross-language soak evidence archive (Phase 2c — iteration 30 step 3)

| Artifact | Location | C++ reference |
| --- | --- | --- |
| Evidence archive runner | `CrossLanguageSoakEvidenceArchive.cs` | `HCDE_SOAK_EVIDENCE_DIR` JSON audit trail |
| Validation evidence | `validation/soak/evidence/*.json` | skipped harness records when binaries absent |
| Archive tests | `CrossLanguageSoakEvidenceArchiveTests` | evidence file count + skip status |

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
| Live netcode | `d_net*.cpp` | `HCDE.Net.Core/*` (wire codecs + authority pump on `hcdeserv`) |
| RCON server | `d_net_rcon.cpp` | Phase 1 client only; server stays C++ until 2f |

## Audit conclusion (interim)

**Phase 2b C# pregame stack is feature-complete for fresh dedicated joins** — loopback WAITING setup, verification-error replies, start-game, and a cross-language guest CLI/harness are in place. The remaining 2b gate is executing the harness against a real `hcdeserv` build and recording the result.

**Phase 2c iteration 42** adds actor property/getter and enhanced direct-special PCD operand coverage, embedded HCIV authority events on guest apply with coop+invasion tail merge policy, and stale export-bundle rejection before weekly soak template commits.

**Phase 2c iteration 41** adds inventory/global-array/Skulltag PCD operand coverage with little-enhanced walker support, invasion spawn-directory apply on guest pump with HCIV+HCKS checksum tail polish, and evidence staleness enforcement in main/soak CI workflows.

**Phase 2c iteration 40** adds script char-range/Eternity stack PCD operand coverage, HCAV authority event tails on authority pump with guest invasion apply wiring, and evidence file freshness enforcement in the Passed soak gate.

**Phase 2c iteration 39** adds script-array/stack PCD operand coverage, HCIV invasion tails on authority pump with guest presentation echo apply wiring, and manifest staleness enforcement in the Passed soak gate.

**Phase 2c iteration 38** adds map/world array and translation-range PCD operand coverage, HCDS coop dead-spawn tails on authority pump with guest gap resync wiring, and weekly `ApplyExportedTemplates` CI apply+commit automation for Passed soak templates.

**Phase 2c iteration 37** adds call/discard/global-array PCD operand coverage, HCDA actor delta tails on authority pump with checksum mismatch resync policy, and `ExportCommittedTemplates` CI artifact bundles for committing Passed soak templates.

**Phase 2c iteration 36** adds gravity/air-control/global-var PCD operand coverage, player-pose HCDW tail seeding from map THINGS with guest checksum mismatch policy, and `TryRecordPassedValidationEvidence` to re-record Passed soak templates in CI.

**Phase 2c iteration 35** adds music/stack PCD operand coverage, `WorldStateTailBuilder.TryBuildCoopTailFromStore` on authority pump with guest sector apply E2E, and Passed soak gate enforcement in main CI when secrets are configured.

**Phase 2c iteration 34** adds HUD/inventory PCD operand skip coverage, `LiveAuthoritySession.Pump` wired into `DedicatedServerHost` after map-load bootstrap, and `CrossLanguageSoakGate` to require Passed manifest entries when soak secrets are configured.

**Phase 2c iteration 33** adds print-stack/direct-special PCD operand coverage, `hcdeserv` `--master` CLI with public query snapshot fields, and `RefreshCommittedEvidence` to prune/refresh committed soak templates in CI.

**Phase 2c iteration 32** adds expanded ACS PCD operand skip coverage (`IfNotGoto`, `CaseGoto`, `Lspec6`), `DedicatedServerQueryResponder` + `DedicatedServerAdvertiser` on `DedicatedServerHost`, and CI validation evidence re-record when soak secrets are configured.

**Phase 2c iteration 31** adds ACS PCD bytecode walk (`MapBehaviorBytecodeWalker`), `HCDE.Server` dedicated host scaffold (`DedicatedServerHost` + `hcdeserv` CLI), and cross-language soak manifest/CI artifact upload.

**Phase 2c iteration 30** adds BEHAVIOR script directory decode (`MapBehaviorDirectoryCodec`), `PregameHost.TryCreateBootstrappedLiveAuthoritySession` for pregame→live map-load handoff, and `CrossLanguageSoakEvidenceArchive` with skipped harness JSON under `validation/soak/evidence/`.

**Phase 2c iteration 29** adds BEHAVIOR lump ACS magic probe (`BinaryMapBehaviorDecoder`), `AuthorityMapLoadBootstrap` for authority WAD→world-store seeding with metadata replicate, and `CrossLanguageSoakSuite` to run pregame + Step 12 soaks with JSON evidence recording.

**Phase 2c iteration 28** adds unified `BinaryMapDecoder`, routes `MapLoadBootstrap` through full map decode, and lands C++ parity for sector light/special HCDW flags (`HCDEWorldDeltaReplicateSectorMetadata`).

**Phase 2c iteration 27** adds BLOCKMAP/REJECT collision decode, optional sector light/special on HCDW wire (`replicateSectorMetadata`), and guest-without-WAD checksum E2E.

**Phase 2c iteration 26** adds SIDEDEFS/SSECTORS binary surface decode, `MapLoadBootstrap` to seed world stores from WAD map sectors, and authority→guest HCDW+HCKS E2E when both sides bootstrap from the same map.

**Phase 2c iteration 25** adds VERTEXES/SEGS/NODES binary geometry decode, `GuestWorldStateBootstrap` to seed `GuestWorldStateStore` sectors from decoded `mapsector_t`, and an optional cross-language soak GitHub Actions workflow.

**Phase 2c iteration 24** adds binary map lump record decode (THINGS/LINEDEFS/SECTORS), `WorldStateTailBuilder` for authority HCDW tails, and end-to-end authority→guest checksum match when world store is wired on both sides.

**Phase 2c iteration 23** adds WAD directory + map lump catalog readers (`WadArchiveReader`, `MapLumpCatalogReader`), UDMF `TEXTMAP` probe, and authority outbound HCKS via `SetAuthorityWorldState`.

**Phase 2c iteration 22** wires `GuestWorldStateStore` into `LiveGuestSession` (`SetGuestWorldState`), adds external-tail snapshot building, sector-only world-delta apply, `HCDE.MapLoader` scaffold, and a GitHub Actions `dotnet test` CI job.

**Phase 2c iteration 21** adds in-memory guest world state (`GuestWorldStateStore`), checksum input builder from applied HCDW/HCDA state (`SnapshotChecksumPlaysimInputs`), and JSON soak evidence recording (`CrossLanguageSoakEvidence` via `HCDE_SOAK_EVIDENCE_DIR`).

**Phase 2c iteration 20** adds a managed netcode Step 12 cross-language soak runner (`NetcodeCrossLanguageSoak`) with shared soak result types and optional client join via `HCDE_HCDE_CLIENT_PATH`.

**Phase 2c iteration 19** adds a managed pregame cross-language soak runner (`PregameCrossLanguageSoak`), optional HCKS checksum hashes on minimal server-snapshot tails, and guest checksum mismatch integration tests.

**Phase 2c iteration 18** adds bootstrap/resync pregame services (`HPS_BOOTSTRAP_*`, `HPS_RESYNC_*`) with 13-byte control payloads and runtime-join loopback tests.

**Phase 2c iteration 17** adds invasion spawn-directory mirror (`InvasionSpawnDirectoryCodec`, `ApplySpawnDirectory`) and snapshot checksum apply (`SnapshotChecksumApplySession`) wired into guest receive.

**Phase 2c iteration 16** adds HCIV invasion apply (`InvasionSnapshotApplySession`, wave monotonic policy, embedded HCAV/HCDA replay) with parsed invasion tail blocks and guest receive wiring.

**Phase 2c iteration 15** adds world-delta apply stubs (HCDW pose validation, HCDA record routing, HCDS spawn retire) with parsed tail blocks and injectable sinks wired into guest receive.

**Phase 2c iteration 14** adds HCSR/HCIN apply sessions with per-player sequence/consistency tracking, idempotent snapshot handling, gap-resync policy, and injectable command sinks wired into guest/authority receive paths.

Next audit checkpoint: **Phase 2c iteration 40** when script char-range PCD operands land or authority event tail builders wire into the tick pump.
