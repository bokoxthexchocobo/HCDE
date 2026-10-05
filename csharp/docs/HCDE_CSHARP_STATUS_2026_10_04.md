# HCDE C# conversion status - 2026-10-04

## Overall result

The C# migration is operational as a tested headless engine subset, but the
full engine conversion and gameplay phases 1-3 are not complete. This report
reviews the current repository, combined audit, roadmap, and the recent
inventory/health/action conversion work. It is not an exhaustive native parity
certification. Historical percentage and LOC estimates are not current measures
of completion; no defensible remaining-LOC or completion percentage is claimed.

## Changes completed in this review

- Found the native ACS ThingDamage2 opcode decoded but absent from execution.
  Added runtime support using native wire ID 335 rather than the historical
  enum alias. It accepts named damage types and returns the number of shootable
  actors processed, even if a processed actor did not lose health.
- Shared target processing with Thing_Damage special 119. Map-special success
  remains independent of target count; the ACS opcode returns the count.
- Added four actual bytecode regressions for zero-ID activation, missing IDs,
  zero damage, negative healing, and shootability filtering.
- Refreshed the README's current test count and linked this status report.

## Validation

Release solution tests: **6,493 passed, zero failed**. Playsim: 5,366;
MapLoader: 533; other suites: 594. Release solution build with
warnings-as-errors and no incremental compilation passed with zero warnings
and zero errors. Whitespace validation passed.

These checks establish managed regression coverage. Native engine comparison,
representative mod compatibility, rendered acceptance and multi-client invasion
sign-off are not established by these results.

## Current subsystem status

| Subsystem | Implemented scope | Remaining completion work |
| --- | --- | --- |
| Tools and protocols | Master/RCON tools, query/transport, pregame and live protocol foundations | Complete native interoperability and adverse-delivery acceptance |
| Dedicated server | Map loading, authoritative ticking, command buffering, snapshots and session integration | Full native server behavior, capacity, timeout/resync and multiplayer acceptance |
| Movement and geometry | Fixed-point actor movement, flat-sector collision, selected stepping/sliding, fractional planes and movers | Complete native collision, portals/advanced geometry, large dimensions and multi-tag behavior |
| Combat and AI | Doom weapon subset, projectiles/traces, damage/armor/death, selected monster actions and targeting | Full action/state catalog, custom actor definitions, native RNG streams, custom weapon/secondary ammo behavior |
| Inventory and pickups | Doom ammo/weapons, health/armor/backpacks/keys, grants/removal/tosses, retention rules and selected skill factors | Separate card/skull ownership, generic inventory objects, custom flags/classes, stacking and complete serialization |
| Health actions | GiveBody percentage/zero semantics; HealThing, DamageThing, Thing_Damage and ACS ThingDamage2 | Health upgrades, morph/voodoo rules, DeHackEd cap overrides and complete corpse revival semantics |
| Maps and resources | WAD/PK3 foundation, Doom/Hexen/UDMF loading, resource overlays and selected DEHACKED support | Full special translation, game-family detection, resource/mod compatibility matrix and representative map acceptance |
| ACS | Variables/arrays, arithmetic/control flow, waits, metadata and selected opcodes/specials | Complete opcode/module/function support; decoded instructions do not all execute |
| ZScript and mod runtime | Integer VM foundation and selected translated behavior | Full script VM, class/type/runtime semantics, actor definition execution and native mod compatibility |
| Networking and invasion | Authority/snapshot integration and invasion foundations | Large-world/adverse-network tests, original round/timer/enemy divergence reproduction and multi-client sign-off |
| Saves and persistence | Versioned pose/state archives and selected inventory/pickup extensions | Recreate complete worlds and actor inventories, custom classes, full replication/persistence parity |
| Client and media | Headless client/session foundation | Usable rendering/input/audio client, software/Vulkan parity and media integration |
| Packaging and release | Managed builds and tests | Cross-platform product packaging, native comparison fixtures and all release gates |

## Recent implemented conversions awaiting commit

Since GitHub checkpoint b59ceb4f, work includes drop-factor scope correction;
Actor.GiveBody with percentage and zero-health results; HealThing and
DamageThing map/ACS actions; Thing_Damage targeting and healing; shared numeric
damage types; and the missing ACS ThingDamage2 runtime operation. Regression
tests and audit records accompany these changes. This report and README refresh
are also local changes. No commit or push is performed by this review.

## Audit findings and limitations

1. The README's current test count was stale. Updated the current checkpoint;
   historical audit sections remain historical.
2. Opcode decoding was being mistaken for runtime conversion in the damage
   path. This specific omission is fixed; other decoded-only instructions
   still require a runtime support inventory.
3. Managed tests do not prove native behavioral parity. Several prior slices
   intentionally use overflow-safe arithmetic, which can differ from native
   overflow behavior at extreme values.
4. Save support is incremental. Actor-property tests do not imply complete
   world recreation or generic inventory persistence.
5. The master plan records unresolved live-session timeout intermittency and
   native invasion divergence. This review did not reproduce or close them.
6. Custom damage states, skill/configuration loading, health upgrades, key
   identity and custom inventory behavior remain explicit gameplay gaps.

## Work required before claiming completion

Build a native-to-managed support matrix for opcodes, actions, actor classes
and persistence fields; classify each as executed, partial, decoded-only or
absent. Continue converting behavior with loaded-map and bytecode regressions.
Develop reproducible native/managed fixtures, especially for movement, combat
RNG and invasion. Resolve multiplayer timeout/delivery/capacity issues. Complete
advanced geometry and mod runtime support, then full persistence and the usable
client. Close phase gates only with their required runtime evidence.

Sources: HCDE_CSHARP_MASTER_PLAN.md, HCDE_CSHARP_PHASE123_STATUS_AUDIT.md,
HCDE_CSHARP_FULL_AUDIT.md (historical), native p_acs.cpp, p_lnspec.cpp,
p_things.cpp and the current managed dispatch and regression files.
