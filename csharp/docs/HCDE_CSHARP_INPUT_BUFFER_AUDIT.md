# Command buffering continuation audit

Date: 2026-09-26. The preceding 920-test gameplay/map/invasion work was committed
as `0a37c839` before this continuation. Earlier audit statements about uncommitted
changes describe their state when those reports were written.

## Closed item: command overwrite within a socket drain

The simulation previously stored one pending command per player. Multiple
commands accepted before the next tic overwrote each other, although network
sequence state advanced for each accepted command.

Player input now uses a FIFO bounded to 128 commands. The authoritative simulation
consumes one queued command per player per tic, including explicit empty commands.
A burst therefore preserves action/movement order without speeding up simulation.
The server sink returns queue admission success. A full queue returns false before
ClientInputApplySession advances the rejected command's CurrentSequence; retrying
that command after capacity opens accepts it once, and duplicates are skipped.

Death and pose restore explicitly clear buffered actions. Commands for an existing
dead/destroyed pawn are consumed without retention, preventing a later resurrection
from executing them. An unknown player number is rejected. The public Pending
property remains a compatibility replacement slot; network input uses the FIFO API.

## Audit corrections

- Audited ClientInputApplySession's ordering: the sink is called before advancing
  CurrentSequence. No transport/header redesign was needed for queue admission.
- Changed internal death/restore cleanup to ClearCommands. Assigning a default
  compatibility slot would insert an empty command and delay the next new action.
- Kept explicit zero commands in the FIFO: they represent elapsed input tics and
  must not be collapsed while draining a burst.
- Bounded the queue so a peer cannot create an unbounded input backlog. This is
  admission control, not a complete latency/overload or packet-loss policy.

## Verification

- Release solution: **923 tests passed, zero failed/skipped**; three new server
  integration cases cover burst execution, full-queue retry/duplicate sequencing,
  and restore/death clearing. These use the real command sink and input-apply
  session, rather than asserting queue implementation details alone.
- Release build with warnings as errors: **zero warnings/errors**.
- `git diff --check`: no whitespace errors.
- Existing UDP invasion, gameplay, map, save and protocol regressions remain passing.
  No dependency changes; no native source changes in this continuation.

## Remaining work

This closes the admitted-command overwrite item in the previous audits. It does
not close phases 1–3 or certify native multiplayer parity. Loss/reorder recovery,
existing gap resynchronization, sustained-overload behavior, participant mapping
and event/weapon-selection handling still need work. Input queues deliberately
are not serialized in pose archives; restore discards future input.

The snapshot actor-count/packet-size failure and accumulated tombstones remain
the most immediate release blocker. Native AI/actions, advanced geometry/map
specials, mod execution, full persistence and native runtime validation remain
as listed in the [movers audit](HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md) and the
[phase 2/3 audit](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md).

This is a focused source and regression self-audit, not an independent security
review or a real native host/client certification.
