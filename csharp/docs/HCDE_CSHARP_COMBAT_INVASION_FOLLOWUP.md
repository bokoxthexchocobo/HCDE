# Combat and invasion follow-up

Date: 2026-09-26. The engine conversion remains incomplete. This follow-up closes
specific simulation and replication defects; it is not full gameplay parity.

The subsequent [gameplay-foundation audit](HCDE_CSHARP_GAMEPLAY_FOUNDATION_AUDIT.md)
supersedes the validation total and records further physics, lifecycle, save-pose
and co-op replication fixes. The historical results below describe this follow-up.

## Combat

- Bullets and melee now select the nearest intersected actor surface, with stable
  actor-ID ordering for ties. Melee no longer hits arbitrary actors off to one side.
- One-sided walls, explicit hitscan/block-everything flags, and closed two-sided
  sector openings stop attacks. Movement-only blocking flags allow shots through
  open two-sided lines, following the distinction in native `P_LineAttack`.
- Dead actors cease blocking or shielding other targets. Dead players cannot
  issue movement, rotate, fire, collect health or activate crossed specials.
  Corpses can retain physical momentum; dead bots stop walking.
- Existing fixed damage and simplified weapons remain. The foundation pass adds
  actor Z, shot-height checks and generic pain/death state timers. Vertical aiming,
  slopes, portals, 3D floors, projectiles, weapon timing, monster attacks and native
  animation definitions still need conversion. Circle tracing is the managed geometry
  model, not exact native collision parity.

## Invasion

The C# director previously started a new wave every 36 ticks regardless of kills.
It now tracks the actors spawned for that wave, stays active until all die or are
destroyed, then waits exactly 35 ticks before incrementing the wave. Disabled or
exited simulations do not advance its state. Counters distinguish spawned,
active and cleared actors; unrelated bots do not affect completion.

Spawn marker 3004 was incorrect: Doom maps use it for Zombieman. The managed
subset now uses MapSpot 9001 (see `wadsrc/static/mapinfo/common.txt`), and spawned
bots use the enemy type instead of the marker type. This also fixes their former
immunity to the combat target filter. Maps with no MapSpot retain the existing
(64, 64) demo fallback.

Native client corrections in `src/d_net.cpp`:

- Derive the pending countdown wave from the received active wave. Previously the
  unreplicated pending field could remain zero and announce the first wave again.
- Use the replicated timer on remote clients. A stale local cutscene countdown
  can no longer override it. Authority-controlled cutscenes retain their behavior.
- Use the accepted snapshot's spawned/cleared counters together with its active
  count, rather than mixing older maxima into new state. The enclosing native
  snapshot handler already rejects old and duplicate simulation tics.

The managed count policy follows the same rule. Its guest receiver now also
honors the command layer's stale/duplicate result before applying snapshot tails
or quitter resets. Previously old simulation ticks could still mutate invasion
state despite being ignored for commands. Guest invasion state now retains the
timer, active count, max waves and derived pending wave.

## Server integration follow-up

- `--gamemode 4` configures the managed director. `--invasion-waves`,
  `--invasion-countdown` and `--invasion-intermission` set a finite wave count and
  durations in seconds. Defaults are 8 waves, a 30-second initial countdown and
  a 1-second intermission. Victory remains visible until a map/restart transition;
  it does not silently wrap back to wave 1. Library users can retain unlimited
  waves by configuring zero; the CLI requires a positive limit.
- Invasion starts when the pregame handshake has produced a live session. The
  executable starts the handshake's game phase when the first client is ready.
- The executable schedules exactly 35 simulation tics per second from elapsed
  monotonic time. It previously called one simulation tick every 10ms. Catch-up
  work runs in bounded batches and retains fractional ticks. `Host.Pump` remains
  an explicit single-tic API for embedding and deterministic tests.
- A publisher maps director phases to native protocol state IDs. Its live header
  and launcher-query fields are derived from the same counters. Since this subset
  spawns the whole wave immediately, its active phase maps to wire `CLEANUP`.
- The shared query/pregame socket dispatches admitted clients' live input to the
  authority session instead of consuming it in the setup parser. Packet sequence
  tracking is independent per endpoint. The client sender advances command
  sequences, and the simulation command sink now forwards `BT_ATTACK`.
- Actor snapshots retain positions, angles, health and live flags. Sector snapshots
  use current moving heights rather than the original map heights.
- Invasion payloads are built once per simulation tic and reused for every client.
  This prevents event draining and rolling checksum changes between recipients.
  The payload budget now uses the existing transport limit instead of the old
  512-byte tail/1024-byte packet scaffolds. Packet paging is not implemented: more
  than 255 actor records or a payload beyond the transport budget produces an
  explicit error rather than silently substituting an incomplete co-op snapshot.

## Validation

- Baseline: 738 managed tests passed. First follow-up: 763 passed. After server
  integration: 776 passed, zero failures.
- Run from repository root: `dotnet test csharp/HCDE.sln --verbosity quiet`.
  SDK 10.0.201 builds net8.0 targets; the pinned `csharp/global.json` is unchanged.
- New simulation regressions cover wall occlusion, line flags, moving openings,
  melee direction/range, corpse handling, dead-player restrictions, wave counts,
  intermission timing, marker types and combat kills of invasion bots.
- A UDP loopback session exercises late-join countdown state, old/duplicate
  simulation tics arriving in fresh transport packets, and same-wave corrections.
- A full pregame-to-live C# server test sends three successive network attacks,
  kills a spawned enemy and verifies replicated health, position, cleared count
  and victory. A two-client test checks identical queued events and checksums
  with 40 moving-pose actor records; a separate test checks independent input
  sequences and duplicate suppression. Scheduler tests cover 60 seconds without
  drift and bounded catch-up. CLI and query/state mapping are also checked.
- Native policy assertions in `tests/invasion_stage9/invasion_policy_tests.cpp`
  compile with MSVC C++17, `/W4 /WX`. This validates the extracted countdown rules.
- Full native build and live multiplayer soak are not verified. Generated native
  project files reference `C:/Users/user/DoomConnector6/HCDE`, a different checkout.
  A selected-file MSBuild invocation did not compile this checkout's `d_net.cpp`.
  No native client/server executable or IWAD was found in this workspace for the
  Stage 9 runtime harness. Policy compilation is not a substitute for that run.

## Still required

The managed invasion subset still needs native classic spawn classes and arguments,
budgets and pacing, participant/wipe handling, boss selection,
spawn collision placement, and AI. The pose save format does not restore director
state, dynamic actor membership or player inventory. These remain prerequisites
for reliable full save/rewind support. Packet paging, reliable event replay after
loss and complete late-join world baselines remain required for large maps and
lossy networks. The existing (64, 64) fallback is only a demo fallback, not native
spawn selection. Passing managed loopback tests does not establish native client
compatibility, complete gameplay, or a graphical C# client.

Before claiming the user's native desync is resolved, rebuild the current native
checkout and run Stage 9 plus a multi-wave host/client session, including a late
join and same-wave wipe/retry. Check round, countdown, alive count and actual
monster presentation together under delay and packet loss.
