# HCDE C# Migration — Phase 1 Principal Audit

**Last updated:** 2026-08-10  
**Status:** Complete (tools + protocol layer). Ready to begin Phase 2 (dedicated server).  
**Related:** [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md) · [`README.md`](../README.md) · PR [#66](https://github.com/bokoxthexocobo/HCDE/pull/66)

## What Phase 1 means

Phase 1 establishes the C# solution and replaces the **smallest standalone binaries** that have no engine dependencies:

- Shared protocol constants and binary codecs
- `hcdemaster` (UDP master list server)
- `hcdercon` (TCP RCON client)
- Regression tests proving wire compatibility with the C++ implementations

Phase 1 does **not** touch the UzDoom-derived engine (`src/`), rendering, playsim, or netcode. The C++ CMake build remains the production game path.

## Boundaries (non-negotiable)

1. **Protocol-only sharing.** C# code mirrors `protocol/hcde_master_protocol.json` and `protocol/hcde_master_protocol.h`. No engine headers, gameplay types, or launcher UI logic cross the boundary.
2. **Wire compatibility.** C# packet codecs must produce/consume the same bytes as the C++ reference for the covered message types. Drift is a release blocker for Phase 1 sign-off.
3. **No gameplay coupling.** `HCDE.Protocol`, `HCDE.Master`, and `HCDE.Rcon` must build and test without linking ZMusic, ZVulkan, or any `src/` object files.
4. **Coexistence.** C++ tools under `tools/hcdemaster/` and `tools/hcdercon/` remain until the C# replacements are validated in CI and a release notes entry ships the switch.
5. **NMS1 scope.** Phase 1 ports the **packet codec** from `src/common/engine/sv_master_nms1.{cpp,h}` (client/advertiser side). A full NMS1-capable master server is Phase 2+ infrastructure work.

## Delivered artifacts

| Artifact | Location | C++ reference | Notes |
| --- | --- | --- | --- |
| Legacy master constants | `HCDE.Protocol/MasterProtocol.cs` | `protocol/hcde_master_protocol.h` | Version 2 constants |
| Legacy master packets | `HCDE.Protocol/MasterPackets.cs` | `tools/hcdemaster/hcdemaster.cpp` | Heartbeat, list query, list response |
| NMS1 types + codec | `HCDE.Protocol/Nms1Types.cs`, `Nms1Packets.cs` | `src/common/engine/sv_master_nms1.*` | Read + write paths |
| RCON framing + FNV-1a | `HCDE.Protocol/RconProtocol.cs` | `tools/hcdercon/hcdercon.cpp`, `src/d_net_rcon.cpp` | 4-byte BE length prefix, 4096 cap |
| Master server | `HCDE.Master` → `hcdemaster` | `tools/hcdemaster/hcdemaster.cpp` | UDP, TTL prune, list query |
| RCON client | `HCDE.Rcon` → `hcdercon` | `tools/hcdercon/hcdercon.cpp` | Nonce auth, `ping`/`status` |
| Unit tests | `csharp/tests/HCDE.*.Tests/` | — | 22 tests at Phase 1 completion |
| Migration plan | `csharp/docs/HCDE_CSHARP_MIGRATION.md` | — | Phases 2–4 defined |

## Verification matrix

| Check | Method | Result |
| --- | --- | --- |
| Legacy master heartbeat encode/decode | `MasterPacketTests` | Pass |
| Legacy master list response round-trip | `MasterPacketTests` | Pass |
| NMS1 challenge request golden vector | `Nms1PacketTests` | Pass (21-byte packet vs hand-computed bytes) |
| NMS1 register/heartbeat/unregister round-trip | `Nms1PacketTests` | Pass |
| NMS1 error response parsing | `Nms1PacketTests` | Pass |
| RCON FNV-1a hash format | `RconProtocolTests` | Pass (`%08x` lower hex) |
| RCON client vs loopback server (`ping`) | `RconClientIntegrationTests` | Pass |
| RCON client vs loopback server (`status`) | `RconClientIntegrationTests` | Pass |
| RCON bad password rejected | `RconClientIntegrationTests` | Pass |
| Master server heartbeat + query integration | `MasterServerTests` | Pass |

### Not yet verified in Phase 1

| Check | Blocker | Target phase |
| --- | --- | --- |
| `hcdercon` (C#) against live `hcdeserv` (C++) | Requires built engine + IWAD-free RCON-only harness | Phase 2 entry |
| C# `hcdemaster` vs C++ `hcdemaster` on-wire interop soak | No automated cross-binary soak yet | Phase 1 follow-up or CI job |
| JSON → C#/C++ codegen | Manual sync today | Phase 1 follow-up |
| NMS1 master server (register/list storage) | Out of Phase 1 scope | Phase 2+ |

## Principal risks and mitigations

| Risk | Severity | Mitigation |
| --- | --- | --- |
| Protocol constant drift (JSON / `.h` / C#) | High | Phase 1 audit flags; add codegen before Phase 2 netcode |
| RCON auth is FNV-1a, not HMAC | Medium | Matches current C++ (`d_net_rcon.cpp`); document as known debt per `HCDE_RCON.md` |
| Dual toolchains (CMake + `dotnet`) | Low | Independent builds; wire C# publish into CI as optional job |
| Premature engine port | High | Phase 2 gated on this audit; playsim/netcode stay C++ until harnesses pass |

## Gaps accepted for Phase 1 close

1. **No JSON codegen** — `MasterProtocol.cs` is hand-maintained against `hcde_master_protocol.h`. Acceptable for Phase 1; must be addressed before netcode constants multiply in Phase 2.
2. **No live `hcdeserv` RCON soak** — loopback server mirrors `d_net_rcon.cpp` framing; live cross-language test deferred to Phase 2 when `HCDE.Server` lands.
3. **NMS1 master server not implemented in C#** — codec only; the public `hcdemaster` still speaks the legacy 4-byte marker protocol.
4. **C++ tools not removed** — intentional; removal follows a release where C# binaries are packaged and validated.

## Sign-off criteria for Phase 2

Phase 2 (dedicated server) may start when all of the following hold:

- [x] `csharp/` solution builds on .NET 8 with zero errors
- [x] All Phase 1 xUnit tests pass
- [x] NMS1 codec ported with golden-vector coverage
- [x] RCON client integration tests cover auth + `ping`/`status`
- [x] Principal audit document published (this file)
- [ ] Optional: CI job running `dotnet test` on `csharp/` (recommended before Phase 2 merge)

## Phase 2 entry recommendation

Start with **`HCDE.Net.Transport`** (UDP primitives from `common/engine/i_net.cpp`), not map loading or playsim. Rationale:

1. Netcode is the highest HCDE-specific value and the hardest compatibility surface.
2. Transport has a narrow API and can be tested without IWADs.
3. Existing Python harnesses (`tests/netcode_step12/`) become the Phase 2 acceptance gate.

Do **not** begin renderer, ZScript VM, or ZMusic ports in Phase 2.

## Source map

| Concern | C++ | C# |
| --- | --- | --- |
| Master protocol constants | `protocol/hcde_master_protocol.h` | `HCDE.Protocol/MasterProtocol.cs` |
| Legacy master server | `tools/hcdemaster/hcdemaster.cpp` | `HCDE.Master/MasterServer.cs` |
| NMS1 packet helpers | `src/common/engine/sv_master_nms1.*` | `HCDE.Protocol/Nms1Packets.cs` |
| RCON client | `tools/hcdercon/hcdercon.cpp` | `HCDE.Rcon/RconClient.cs` |
| RCON server (reference) | `src/d_net_rcon.cpp` | Loopback test server only |
| RCON design | `docs/HCDE_RCON.md` | Unchanged |

## Audit conclusion

**Phase 1 is complete** for the agreed scope: protocol layer, standalone tools, tests, and documentation. The C# tree is a valid foundation for Phase 2. The engine core remains C++; no production binary switch has occurred yet.

Next principal audit: **`csharp/docs/HCDE_CSHARP_PHASE2_AUDIT.md`** (to be created when Phase 2 scoping begins).
