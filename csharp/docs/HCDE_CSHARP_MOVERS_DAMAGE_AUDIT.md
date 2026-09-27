# Phase 1–3 continuation: floors, lifts and damage audit

Date: 2026-09-26. Baseline: 908 managed tests. This is a focused continuation of
the gameplay foundation, combat and maps work. **Phases 1–3 remain incomplete.**
The new behavior below is implemented and regression-tested; it is not a full
native engine parity sign-off. Changes remain local, uncommitted and unpushed.

## Implemented

- Classic Doom floor raises to the next higher neighboring floor (18/69), lowers
  to the lowest neighboring floor (23/60/38/82), and 24-unit raises (58/92).
- Classic lowering lifts (10/21/62/88), with speed 4, a 105-tic bottom wait, return
  to the original height, and the corresponding use/cross/repeat rules.
- Classic raising/crushing floors (55/56/65/94) target ceiling minus eight and
  apply ten damage on every fourth simulation tic while obstructed. Shared damage
  handling supplies armor, invulnerability, pain and a single death transition.
- Supported ZDoom/Hexen actions 20/21/23/25/62 use argument tags and values,
  including the eight-unit platform lip. Speed arguments remain rounded to the
  existing integral sector-height model; fractional-speed parity is not claimed.
- Floor targets come from adjacent sectors, not a fixed eight-unit nudge. Missing
  valid targets do not consume one-shot actions. Active movers cannot be duplicated.
- Grounded riders follow platforms. Non-crushing ascending floors pause before
  an actor would overlap the ceiling; after the obstruction clears they continue.
  An obstructed returning lift reverses down without damaging its rider.
- Mover direction, targets, speed, delay, countdown and crushing mode now enter
  the simulation checksum, so identical positions with different future movement
  no longer produce the same checksum merely because those fields were omitted.
- The existing green-armor value 33 now follows Doom's one-third convention.
  Three points of damage consume one armor and two health, instead of the prior
  zero armor and three health. Remaining armor still limits absorption.

Native sources reviewed: `wadsrc/static/xlat/base.txt`, `defines.i`,
`src/playsim/actionspecials.h`, `src/playsim/p_lnspec.cpp`, the crush damage path
in `src/playsim/p_map.cpp`, and `wadsrc/static/zscript/actors/doom/doomarmor.zs`.

## Audit fixes and evidence

1. Loaded map floor/lift actions previously had no dispatch, while the internal
   legacy API only nudged a plane eight units. Added namespace-specific actions
   without changing the legacy Unknown-format action numbers.
2. Floor motion changed the plane without checking actor headroom. Added a
   prospective-height check before committing non-crushing upward movement.
3. Initial blocked-lift reversal set a wait whose expiry switched it upward again.
   The corrected path reverses immediately, descends, then uses its bottom wait.
   A regression observes the floor stop at 48 and then descend to 44.
4. A very large raise argument could overflow before clamping. Destination
   arithmetic uses long and is bounded before converting to a sector height.
5. Mover state was absent from checksums. Two initially identical rooms with
   different destinations now have different checksums after the first equal-sized
   step; a regression verifies this before their geometry diverges.
6. Green armor's integer percentage lost the intended third on small hits.
   Regressions cover damage 2/3/6 and armor depletion.

## Verification

- `dotnet test csharp/HCDE.sln -c Release --verbosity quiet`: **920 passed,
  zero failed/skipped**, including 167 playsim cases. Twelve new cases cover
  floor targets, rider movement, activation/consumption, lift timing/reversal,
  obstruction removal, crushing/death, argument overflow, checksums and armor.
- `dotnet build csharp/HCDE.sln -c Release --no-restore --verbosity quiet -warnaserror`:
  **zero warnings and errors**.
- `git diff --check`: no whitespace errors.
- Existing managed invasion, networking, physics, aim, mod-loader, archive and
  combat regressions remain passing. No package dependencies changed; the previous
  continuation's dependency audit remains the latest scan.

This was a source-based self-audit with synthetic fixtures. No native executable
or real-map playthrough was used; no independent review is claimed.

## Remaining risks and completion gates

- Crushing is a managed subset. Native speed changes/crush modes, actor stacking,
  gibbing, dropped-item handling and actors spanning sector boundaries need work.
  Obstruction tests currently use each actor's assigned sector.
- Sector heights/motion are integral. Slopes, portals, 3D floors, full drop-off and
  corner behavior remain absent. Floor and ceiling movers cannot run concurrently
  in the same sector under the current single-mover guard.
- Most line/sector actions, textures/switches, monster/projectile activation and
  complete map progression are still missing. These additions do not close phase 3.
- Native state/action tables, specialized AI, exact weapon/RNG behavior and full
  mod scripting remain incomplete. These additions do not close phase 2.
- Pose archives still do not save movers or full gameplay state. Checksumming
  mover state does not make it persist across save/restore.
- The high-priority snapshot-count/packet-budget failure, accumulated tombstones,
  input buffering, roster lifecycle and native interoperability gates remain.
  The originally reported native invasion desync still needs a real multiplayer
  reproduction/verification run.

The [previous combined audit](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md) retains the
broader unfinished-work inventory. No previously documented release gate is waived.
