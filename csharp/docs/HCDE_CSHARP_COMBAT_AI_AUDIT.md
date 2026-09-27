# Combat and AI implementation — combined audit

> Historical pass. See the [phase 2/3 continuation audit](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md)
> for newer actor defaults, pitch, archive changes and the current completion gates.

Date: 2026-09-26. This is phase 2 of the new eight-phase completion plan, not the
historical networking phase 2. Baseline: 802 managed tests after the gameplay
foundation pass. All changes remain local and uncommitted.

## Verdict

The managed combat/AI **implementation pass is complete for the behaviors listed
below**. Full native combat/AI parity is **not signed off**, and the overall
conversion is not ready for release. In particular, sustained projectile use can
exhaust the existing snapshot record/packet budget. Passing tests does not remove
that blocker or establish native-client compatibility.

This is a risk-based self-audit: focused source review of the changed gameplay,
server, replication, save, map and scripting paths; solution-wide build, test and
dependency checks across all 23 projects; and review of the existing phase audits.
It is not an independent security assessment or a line-by-line re-audit of every
native source file. No native code was removed.

## Implemented behavior

| Area | Behavior |
| --- | --- |
| Weapon cadence | Per-player cooldown prevents repeated firing every tic or bypass by changing the selected weapon. Empty/unowned/invalid weapon selections do not consume ammo or start cooldown. |
| Ammo | Pistol/chaingun: one bullet; shotgun: one shell; super shotgun: two shells; rocket: one rocket; plasma: one cell; BFG: forty cells; melee: no ammo. |
| Pellets | Shotgun fires seven pellets, super shotgun twenty, each with seeded horizontal spread and 5/10/15 damage. The explicit integer PRNG is initialized from the map seed. |
| Projectiles | Rockets, plasma, BFG balls and imp-style fireballs travel through the simulation, sweep against actor cylinders and wall capsules, check floor/ceiling openings, expire, and attribute damage to their owner. |
| Splash / BFG | Rockets apply distance falloff with sight occlusion, including owner splash. BFG impacts include an approximate forty-ray owner-origin spray. |
| AI | Known map monster IDs and invasion bots acquire a visible living player, retain a target, pursue its last visible position, use local obstacle steering, wind up attacks, recover, and stop attacking during pain/death. Damage sources can trigger infighting. |
| Attack styles | Zombieman/shotgun-guy/chaingun-guy IDs use the generic hitscan brain; imp/baron/hell-knight/cacodemon IDs use a generic fireball brain; demon/spectre IDs use melee. These are behavior families, not complete native class definitions. |
| Integration | Projectiles publish with the projectile category and XYZ pose; removals clear the live flag. Projectiles cannot activate crossing specials. The server no longer advances gameplay during the pregame lobby. |
| Maps | UDMF blockeverything, blocksight, blockhitscan and blockprojectiles survive parsing and level construction. Sight, hitscan and projectile flags are evaluated independently. |
| Diagnostics / pose saves | Simulation checksums include combat RNG, inventory, cooldown, AI control state and projectile owner/type/lifetime. Version 3 pose archives preserve RNG and weapon cooldown and retain v1/v2 reader support. |

Native references used: `src/doomdata.h`, `src/maploader/udmf.cpp`,
`wadsrc/static/zscript/actors/doom/weaponpistol.zs`, `weaponshotgun.zs`,
`weaponplasma.zs`, and `doomimp.zs`. Ammo/pellet conventions and flag distinctions
are based on these sources; timings and attacks remain a managed approximation.
Pistol, melee, direct projectile and generic monster damage are still simplified.

## Findings corrected in this pass

1. Weapons previously fired each received tick, and plasma/BFG were instant hitscan.
   Added cooldowns and actual projectiles; the invasion network test now respects
   pistol cadence while still verifying kill/count/victory synchronization.
2. Bots previously walked east without acquiring or attacking players. Added a
   shared brain used by known map monster types and invasion bots. No-target bots
   now remain idle; the old walking assertions were replaced accordingly.
3. Generic shot blocking cannot be reused for sight and missile movement: native
   flags distinguish all three. Added independent checks and map-import regressions.
4. New missiles would otherwise execute map crossing specials and be published as
   monsters. Excluded them from crossings and assigned the projectile category.
5. The existing host ticked map actors while clients were still joining. With AI
   enabled this could kill players before gameplay began. The lobby now remains
   paused; handshake-to-live integration still passes.
6. Pose restore could otherwise retain a newer cooldown/PRNG state. Version 3
   preserves these two fields; a regression verifies that restoring cannot grant
   a second immediate shot. This is still not a complete savegame.
7. Broader scripting review found negative/overflowing ACS jump offsets could
   reach an invalid span, and a truncated line-special instruction could execute
   with a fabricated zero argument. Invalid instruction pointers stop their fiber;
   truncated special operands cannot execute. Completed fibers are removed.
8. Solution-wide NuGet audit reported high-severity advisory entries for the old
   test framework's transitive System.Net.Http 4.3.0 and
   System.Text.RegularExpressions 4.3.0 dependencies. Updated xUnit from 2.5.3 to
   2.9.3 in all eleven test projects. The post-update scan reports no known
   vulnerable packages using the configured NuGet source. This is a dependency
   advisory result, not proof those old packages were exploitable on net8.0.
9. Moved reusable stack buffers outside loops in DemEventStreamConverter and
   SnapshotChecksumPlaysimInputs, removing both existing CA2014 warnings.

## Remaining findings — no release sign-off

| Priority | Finding and consequence | Follow-up |
| --- | --- | --- |
| High | Full snapshot tails are limited to 255 actor records and the 8,000-byte transport budget. The publisher retains removed actors as tombstones. Repeated projectile shots therefore eventually make a snapshot too large, producing the existing explicit exception and ending the server run. | Multiplayer paging, retirement acknowledgements and bounded baselines. This is now especially important for sustained plasma/rocket use. |
| High | Projectile class IDs 65530–65533 are managed-only identities. Actor spawn/class mapping and animation presentation are not a native-client implementation. | Native class negotiation and client presentation; do not advertise native interoperability based on the loopback test. |
| High | Native enemy defaults, full attack/state tables, sound propagation, specialized bosses, resurrection, flying/skull behavior and native path choices are incomplete. Generic actors retain the existing default health/size unless DEHACKED overrides them. | Additional combat/AI conversion before full phase-2 parity sign-off. |
| High | Weapon startup/psprite states, pitch/autoaim, vertical pellet spread, exact RNG streams, damage distributions, BFG behavior and native refire timing are not reproduced. Weapon selection currently uses the managed inventory API; the server command sink still ignores selection/event records. | Combat parity plus input/client integration. |
| High | Pose archives do not restore inventory, dynamic actor membership, projectile lifetime/owner, AI target/windup/memory, running ACS, movers or invasion state. New cooldown/RNG fields do not make a complete combat save. | Persistence phase; no full save/rewind claim. |
| High | No complete active-player/participant lifecycle: brains see the simulation's living player-start pawns, not a fully maintained native roster. Disconnect, respawn and unused starts need integration. | Multiplayer and invasion participant handling. |
| High | Shared-socket input still has a single pending command per player; multiple accepted commands in one drain can overwrite one another. Loss/reorder recovery, event replay and late-join baselines remain incomplete. | Multiplayer phase. |
| High | Flat-sector physics still omits slopes, portals, 3D floors, stacking and full crushing/drop-off behavior. Sweeps use doubles before fixed-point storage. | Maps/physics and cross-platform parity work. |
| High | Native engine build/runtime not verified. Generated VS projects reference another checkout, and a native executable/IWAD test fixture was not available. | Build current native checkout; multi-wave host/client run with late join, wipe/retry and loss. Original native invasion desync remains unconfirmed. |
| Medium | Rendering remains a software demonstration/recorded-quad backend; audio is a PCM mixer; ZScript and ACS are limited interpreters, not the native runtimes. | Scripting and client phases. |
| Medium | Direct geometry scans, temporary allocations and broad snapshots have no representative large-map performance evidence. Unknown map classes and map-special translation remain incomplete. | Map compatibility, profiling and release validation. |

## Verification and coverage

- Release solution test run: **832 passed, 0 failed, 0 skipped** across eleven test
  projects. Thirty new cases extend the previous 802-test baseline.
- `dotnet test csharp/HCDE.sln -c Release --verbosity quiet`
- `dotnet build csharp/HCDE.sln -c Release --no-restore --verbosity quiet -warnaserror`:
  **0 warnings, 0 errors**.
- `dotnet list csharp/HCDE.sln package --vulnerable --include-transitive`:
  no known vulnerable packages from `https://api.nuget.org/v3/index.json` after the update.
- Native countdown/pending-wave policy assertions compile with MSVC C++17,
  `/W4 /WX /c`. This is not compilation of the complete native `d_net.cpp`.
- `git diff --check`: no whitespace errors.
- Commands run from repository root with SDK 10.0.201 / net8.0 runtime; the existing
  `csharp/global.json` pin was not changed.

New tests cover cadence, switching during cooldown, full ammo costs, invalid
selections, deterministic pellets, delayed/nearest/single projectile impacts,
owner exclusion and splash, wall occlusion, vertical separation, expiry, special
exclusion, independent line flags, sight acquisition, pursuit, retargeting,
windup/pain/death, infighting, deterministic AI runs, pose cooldown/RNG, UDMF flags
and malformed ACS. The existing UDP pregame-to-live invasion test additionally
fires a plasma projectile and verifies its category, movement and removal.

Solution-wide regression coverage also retains protocol golden vectors, UDP
transport/query, pregame, master/RCON, map parsing, foundation physics, invasion
timers/counters, stale snapshot handling, two-recipient co-op/invasion consistency,
and client/Gamedata subset tests. Those tests verify their implemented surfaces;
they are not evidence that the complete engine has been converted.
