# K8ti

> An OS-grade, cross-platform AI assistant that is **raised** — not just deployed —
> from a read-only observer into a trusted agent that can operate your computer.

**K8ti** is the model. This repo is the **environment that raises it**: a local,
privacy-first learning loop that watches you work, learns by imitation, earns
trust one capability at a time, and continuously fine-tunes itself on your own
hardware. No paid API, no data leaving your network. You own the model.

---

## The one-paragraph pitch

The accurate AI assistants are accurate because they are **trained and post-trained**
on enormous graded experience. You can't pretrain one of those at home — but you
**can** take a strong, openly-licensed base model and *keep specializing it on your
own work*, forever, locally. K8ti does that with a safety-first twist borrowed from
how you'd teach a child: **you don't give a baby a match.** K8ti starts only allowed
to *watch*. When it proves it knows what it's doing, it earns the right to *suggest
changes*. When it proves that's safe, it earns the right to *execute*. Competence is
demonstrated before capability is granted — and it can be revoked.

## The trust ladder (core idea)

| Rung | Allowed to | Learns by | Graduates when |
| --- | --- | --- | --- |
| **1. Observe** | Read only — sees screen/files/terminal, *predicts* actions, touches nothing | Imitation (watching you) | It predicts the right next action above a bar on held-out tasks |
| **2. Manipulate** | Propose & apply reversible changes, behind your approval — **cannot run code** | Preference learning (your accept/reject/correct) + compile/lint | High acceptance, low correction, changes compile, zero destructive proposals |
| **3. Execute** | Run code/commands — sandbox VM first, then real machine with guardrails | Reinforcement (tests pass, code runs) | Proven in sandbox, then granted monitored, allowlisted real access |

Promotion is **data-driven** (it must clear that rung's eval), and **demotion exists**
(regress or misbehave → drop a rung and re-earn trust).

## Read next

- [`DESIGN.md`](DESIGN.md) — the full blueprint: architecture, hardware mapping,
  the learning loop, the component stack (with licenses), data-lake schema, safety
  model, and the phased roadmap.

## Status

Pre-implementation. This repo currently holds the **design only**. First code
milestone is **Rung 1 (Observe)** — a zero-risk demonstration collector that begins
banking training data immediately.

## License intent

Permissive (MIT-style), aligned with the owner's StayVibin license. The recommended
component stack is deliberately all-permissive (MIT / Apache-2.0) so nothing forces
this project open and the fine-tuned K8ti weights remain fully owned. See
[`DESIGN.md` § Licensing](DESIGN.md#12-licensing-the-stack-is-deliberately-permissive).
