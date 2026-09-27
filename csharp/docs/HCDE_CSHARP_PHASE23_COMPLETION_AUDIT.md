# Phase 2 and 3 continuation — code audit and completion gates

> Continued by the [phase 1–3 movers/damage audit](HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md).
> The outstanding gates below remain open unless that report explicitly updates them.

Date: 2026-09-26. Baseline: 858 passing tests after the maps/mods foundation.
This report supersedes the current-status portions of the combat/AI and maps/mods
audits. The earlier reports remain historical evidence.

## Verdict

**Neither phase is fully finished.** The changes below close specific gaps, but
do not constitute full native combat/AI or maps/mods conversion. The request to
finish both phases remains larger than the implemented surface. In particular,
native actor action/state conversion, remaining geometry/specials and integrated
mod scripting are substantial implementation work, not just pending tests.

The new code has a focused self-audit and solution-wide regression/build checks.
This is not an independent review, a complete native-source audit, or a real-map
compatibility certification. All changes remain local, uncommitted and unpushed.

## Implemented in this continuation

| Area | New behavior and limits |
| --- | --- |
| Actor defaults | Eighteen Doom monster editor IDs use native health, dimensions, speed and pain-chance data instead of generic health 30. Floating defaults disable gravity. This does not provide the missing specialized brains, animation states or floating pursuit. |
| Pain / movement | Damage observes a seeded pain-probability gate. Existing chase movement uses the actor's configured speed, scaled to the current per-tic approximation and capped to the physics maximum. This is not native RNG/state-timing parity. |
| DEHACKED | Patch results clone actors, states and sounds instead of mutating the baseline. Supported partial actor patches retain native dimensions/health, and editor-number remaps retain the original brain/default identity. The actor table and executable field coverage remain subsets. |
| Aim | UserCmd pitch reaches the player, clamps to +/-89 degrees and participates in checksums. Hitscan intersects the ray with both the cylinder's horizontal interval and vertical interval; wall openings are tested at the crossing height. Player projectiles honor pitch and preserve speed at steep angles. No autoaim, vertical pellet spread or native pitch replication/presentation is claimed. |
| Pose archive | Version 4 stores pitch and the use latch alongside existing v3 fields. Readers retain versions 1–3. This still is a pose archive, not a full savegame. |
| PK3/WAD resources | Repeated `--file` accepts WAD or PK3. Resource paths stay in memory, case-insensitive lookup uses the last loaded entry, and directory paths keep distinct resources distinct. `maps/*.wad` entries become map groups named by their filename. Root lumps such as `dehacked.txt` join the ordered WAD stream. Resource bytes are available through `DedicatedServerOptions.Resources`; textures/audio/scripts are not all consumed by the client/runtime. |
| Archive limits | Input and expanded byte budgets are 256 MiB, with a 65,536-entry limit; nested map bytes are conservatively charged again when decoded. Paths containing traversal, absolute roots or drive syntax are rejected and nothing is extracted to disk. Invalid zip/lump data returns a diagnostic. Peak process memory is higher than the byte budget because there are multiple representations. |
| Hexen records | Separate 20-byte thing / 16-byte linedef decoding retains TIDs, height, arguments, skill/mode and supported activation flags. Vertex/side/sector records go through checked codecs and level validation. BEHAVIOR bytes are retained but not automatically executed. Class filters and the complete Hexen gameplay/script/activation model remain missing. |
| Line activation | Classic blue/yellow/red locked doors require the matching inventory key and retain repeat/one-shot behavior. Loaded classic doors use speed 2 and wait 150 tics. Supported ZDoom doors use speed/wait arguments (fractional speeds are approximated by integral sector motion). Held use acts on a new press; supported use-through lines allow a use trace to continue. |
| Multiplayer bootstrap | Seeding now uses LevelBuilder rather than the Doom-only decoder, allowing validated UDMF/Hexen geometry to reach the live authority bootstrap. Sector indexing uses an int loop with an explicit ushort wire-range guard. This does not solve participant lifecycle, native interop or snapshot size limits. |

## Audit findings fixed before delivery

1. `InvalidDataException` is not covered by the loader's original IOException
   filter. Malformed ZIP, path rejection and expanded-size errors escaped instead
   of returning false. Added the explicit exception case and failure regressions.
2. A DEHACKED editor-number remap lost its AI because brain selection used the
   replacement number. Preserved original class identity for default/brain lookup.
3. Chained patches reused mutable baseline actor/state/sound objects. Added copies
   and tests proving earlier results remain unchanged.
4. Hexen activation masks initially conflated projectile and player activation.
   Reviewed `MapLoader::LoadLineDefs2`: encoded 7 means projectile touch, not the
   internal AnyCross flag. All eight encoded activations now have regression cases.
5. Local extended-map loading did not imply live bootstrap support: the networking
   bootstrap still called BinaryMapDecoder. Replaced that path with LevelBuilder,
   with separate UDMF and Hexen bootstrap regressions.
6. The existing ushort sector loop could wrap at 65,536 sectors. Changed it to an
   int loop and reject counts outside the protocol's index space before mutation.
7. Applying pitch only to projectile Z speed would cap aim at about 45 degrees.
   Normalize the horizontal component for player pitch; steep-angle tests verify
   direction and total speed.
8. Pose restore would retain later pitch/use-latch state. Version 4 restores both,
   including validation of pitch before mutating the simulation; a separate v3
   byte fixture verifies cooldown/RNG backward compatibility.
9. WAD resource limits were initially checked after directory materialization.
   Validate the entry count from the header first; regression uses 65,537 entries.
10. Audit of numeric inputs bounded DEHACKED chase speed to the physics maximum
    before fixed-point conversion and clamps negative door delays to zero.

Earlier trace tests were tied to the old generic imp/demon health. Their expected
health now reflects native defaults; geometry-only fixtures disable AI during
trace tests. The pain-cancellation test explicitly requests guaranteed pain,
while separate tests cover zero/guaranteed pain gates. Assertions were not removed
to conceal behavior changes.

## Outstanding completion gates

| Phase / priority | Work still required |
| --- | --- |
| Phase 2 — high | Full actor state/action tables, boss attacks, resurrection, lost-soul/pain-elemental behavior, sound propagation and native path/float logic. Some cataloged actors currently have dimensions/health but no specialized brain. |
| Phase 2 — high | Exact weapon/refire/psprite behavior, autoaim, damage and RNG streams, BFG spray, vertical pellet spread, and network weapon-selection events. Added pitch does not close these gaps. |
| Phase 3 — high | Remaining Doom/Boom/Hexen/UDMF line and sector specials, complete floors/lifts/crushers, switch textures, front/back activation rules, monster/projectile activation, topology/resource diagnostics, slopes, portals and 3D floors. |
| Phase 3 — high | Integrated MAPINFO map progression, complete resource namespaces/search semantics, client texture/audio consumption, DECORATE/ZScript definitions and complete DEHACKED/BEX. PK3 byte loading is not mod execution. |
| Shared — high | Snapshot tails still have actor-count and 8,000-byte limits, with retained tombstones. Large maps or sustained projectiles can terminate the server. Paging/retirement acknowledgements remain required. |
| Shared — high | Native class/resource negotiation, participant lifecycle and input queues. Player-start seeding is still a provisional roster. Native-client interoperability remains unverified. |
| Persistence — high | Full dynamic membership, inventory, AI/projectile state, movers, ACS and invasion saves. v4 pose data does not replace the persistence phase. |
| Validation — high | A build/run of this native checkout and representative maps/mods, native-versus-managed comparisons and multiplayer invasion wave/timer/enemy tests including loss, late join and wipe/retry. No native executable/IWAD fixture was available for those checks. |

These are not waived by passing unit tests. The original invasion synchronization
report still needs native runtime confirmation; this pass retains the managed
invasion regressions and does not claim the native bug is fully cleared.

## Verification

- Release solution: **908 passed, zero failures/skips**, 50 cases above baseline.
  Breakdown: Protocol 15; Gamedata 10; Transport 10; Master 1; RCON 6; MapLoader
  145; Playsim 155; Client 12; Net.Core 340; Pregame 83; Server 31.
- Release solution build with warnings as errors: **zero warnings/errors**.
- NuGet vulnerability scan including transitive packages: no known vulnerable
  packages reported by the configured NuGet source.
- `git diff --check`: no whitespace errors. Previous local work is preserved.
- Commands run from the repository root, SDK 10.0.201 targeting net8.0. No SDK
  pin changes or new package dependencies.

Native references reviewed: Doom actor files under
`wadsrc/static/zscript/actors/doom/`, `src/doomdata.h`,
`src/maploader/maploader.cpp`, `src/maploader/udmf.cpp`, and classic line
translations under `wadsrc/static/xlat/`. Synthetic regression fixtures establish
the documented managed behavior; there is no broad mod-corpus or performance
certification in this report.
