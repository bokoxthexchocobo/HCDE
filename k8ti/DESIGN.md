# K8ti — Design Document

**Status:** Design / pre-implementation
**Last updated:** 2026-06-15
**Model name:** K8ti
**Owner:** bokoxthexchocobo

This document is the blueprint for **K8ti**: a locally-trained, OS-grade,
cross-platform AI assistant, and the **learning environment** that raises it from a
read-only observer into a trusted agent. It captures the full vision, the
architecture, the hardware it runs on, the safety model, the (license-vetted)
component stack, and a phased roadmap.

> This is intentionally a standalone project — separate from HCDE (where this doc is
> temporarily parked) and from StayVibin. It is meant to be moved into its own repo.

---

## 1. The goal

Build **K8ti**, an AI assistant that can operate a whole computer — see the screen,
read files, move the mouse, type, and run commands — **across Windows, macOS, and
Linux**, running **fully locally** with no paid API and no data leaving the home
network. Crucially, K8ti is not a static download: it **continuously learns and
specializes on the owner's own work**, getting more accurate over time.

The thesis, in the owner's words: *"if I want the most accurate non-paid AI model, I
have to train it myself."* That is correct — with one important refinement (§3).

## 2. Core principles

1. **Raise it, don't just run it.** Capability is *earned*, never assumed. K8ti
   climbs a trust ladder (Observe → Manipulate → Execute). "You don't give a baby a
   match" — you teach it first, then hand over the match.
2. **Learn side-by-side.** K8ti's first teacher is the owner. It learns by *watching*
   real work (imitation) before it ever acts.
3. **Own the data, own the model.** Every interaction becomes private, graded
   training data stored on the owner's NAS. The fine-tuned weights are owned outright.
4. **The loop is continuous; the weights change in batches.** Learning *feels*
   continuous because the loop never stops — not because any single chat rewrites the
   model live (that would be unstable; nobody ships true online weight updates).
5. **Server-authoritative learning.** Training, scoring, and promotion decisions live
   on the always-on guidance server. Clients only produce experience and consume the
   current best model.
6. **Safety is structural.** K8ti literally *cannot* do what it hasn't earned, because
   the environment enforces the permission tier.

## 3. Reality check (so we build the right thing)

There are two very different meanings of "train my own model." Only one is feasible
at home, and it's the powerful one:

- ❌ **Pretraining a base model from scratch** (the raw GPT/Qwen-class model). Costs
  millions of GPU-hours and trillions of tokens. Not feasible on home hardware.
  Karpathy-style "autoresearch / loopcraft" loops train *toy* models in 5-minute
  windows to compare architectures — great for learning, not for a usable assistant.
- ✅ **Continuously adapting a strong, openly-licensed base model to *you*.** Take an
  open base (e.g. UI-TARS-7B), and keep fine-tuning it on your repos, your accepted
  edits, your successful agent runs. A general paid model is broad; a model tuned on
  *your* stack and *your* taste can beat it **for your work** without being smarter in
  the abstract.

**Measure success by "did K8ti climb a rung / get more reliable on my tasks this
month," not by "is it GPT-class yet."** Each rung is independently useful and
shippable.

## 4. System topology

Two machines plus thin clients. Roles are split by what each is good at.

```
   Desktop (RTX 3090, 24 GB)            NAS VM "Guidance Server"
   ── HEAVY TRAINING WORKER ──          ── ALWAYS-ON BRAIN + DATA LAKE ──
   • big QLoRA / full-FT jobs           (2× Xeon ~28c/56t, 256 GB RAM,
   • idle-gated (preempts on input)      2× 8 GB GPU, 40 TB RAID10)
   • checkpoints stream to NAS  ──────►  • coordinator + job queue
                                         • data lake on RAID10 (source of truth)
   Client(s) running K8ti ──events────►  • auto-scorer / eval farm (56 threads)
   (the "environment" app)               • small-model QLoRA on the 2× 8 GB
                                         • eval gate + champion/challenger registry
                                         • model server → champion back to clients
```

### 4.1 Hardware inventory (actual)

| Node | Specs | Role |
| --- | --- | --- |
| **NAS VM** | 2× Xeon Gold (~14c each ≈ 28c/56t), 256 GB RAM, 2× 8 GB GPU, **40 TB RAID10** | Always-on coordinator, data lake, scorer/eval farm, champion server, small-model trainer |
| **Desktop** | RTX 3090 (24 GB VRAM) | Heavy training worker; idle-gated; checkpoints to NAS |
| **Clients** | Any PC running the K8ti environment app | Produce graded experience; consume champion model |

### 4.2 Why this split works

- The NAS is **always on**, so the "train while idle" problem mostly disappears — the
  NAS *is* the idle machine and can grind 24/7 without competing with the desktop.
- The 40 TB RAID10 means **training data is never deleted** — and for a flywheel,
  retained history is exactly what compounds. It is the one irreplaceable asset.
- The desktop's 3090 (24 GB) is the real training muscle: comfortable **QLoRA on
  7B–13B**, even ~30B with offload. It only runs when the desktop is idle.

### 4.3 The GPU reality

- **Don't pool 8 GB + 8 GB into 16 GB for one model.** Possible (FSDP / DeepSpeed
  ZeRO) but slow and fiddly without NVLink. Not worth it at this scale.
- **Per single 8 GB card, 4-bit QLoRA + gradient checkpointing + short seq + batch 1
  + grad-accumulation:** 1.5B–4B comfortable, 7B tight (use CPU offload).
- **256 GB RAM is the cheat code.** Paged optimizers + ZeRO-Offload push optimizer
  state (and some layers) into system RAM, letting the 8 GB cards train beyond their
  VRAM — slow but fine for an always-on unattended box.
- **Use the two NAS cards as a champion/challenger pair**, not a pool:
  - **GPU 0 = challenger** trains the next candidate adapter.
  - **GPU 1 = champion** serves the current best model + runs evals.
  - When a challenger wins, swap roles. The loop never stops serving to train.

## 5. The trust ladder (the heart of K8ti)

Three rungs. Each has: what it may do, how it learns there, the **gate to graduate**,
and the safety mechanics. This maps to established ML methods — **imitation /
apprenticeship learning**, **curriculum learning**, and **graduated autonomy /
capability gating**.

### Rung 1 — Observe (Apprentice)

- **Permission:** Read-only. Sees the screen, open files, terminal output, and the
  task. *Predicts* the next action but **cannot apply anything**.
- **Learning:** Imitation. "What would I do next?" graded against what the owner
  *actually did*. Pure, safe, free data from normal work.
- **Gate to graduate:** Predicts the expert (owner / teacher model) action above a set
  accuracy on held-out tasks. It must *know* what to do before being allowed to do it.
- **Safety:** Zero risk — it touches nothing.

### Rung 2 — Manipulate (Supervised apprentice)

- **Permission:** Propose & apply changes (edit files, write code) that are
  **reversible** (git / snapshots) and **behind owner approval**. **Cannot run code.**
- **Learning:** Preference learning (DPO/KTO) from accept / reject / correct signals,
  plus does-it-compile / does-it-lint *without executing*.
- **Gate to graduate:** High acceptance rate, low correction rate, changes
  consistently compile + lint, **zero destructive proposals**.
- **Safety:** Every change reversible; nothing executes; human in the loop.

### Rung 3 — Execute (Trusted agent — "gets the match")

- **Permission:** Run code/commands — **first inside a disposable sandbox VM**
  (no network, snapshot rollback), then graduated to the real machine with guardrails
  (command allowlist, dry-run, resource limits, rollback).
- **Learning:** Reinforcement on the strongest reward signal — tests actually pass,
  code actually runs.
- **Gate to graduate:** Passes sandboxed execution cleanly; then monitored,
  allowlisted real access.
- **Safety:** Sandbox-first; reversibility everywhere; high-risk ops require approval.

### Two rules that make the ladder real

1. **Promotion is data-driven, not vibes.** A model only climbs when it clears that
   rung's held-out eval. The guidance server is the examiner.
2. **Demotion exists.** Regress or do something unsafe → drop a rung and re-earn
   trust. The match can always be taken back.

## 6. The learning loop

Same pattern the big labs use, just smaller and local: **act → collect graded
experience → train offline in batches → eval-gate → redeploy → repeat.**

1. **Collect.** The environment app tees every session (task, thoughts, tool calls,
   diffs, outcomes) to the guidance server's ingest API.
2. **Score / label (auto).** Derive rewards with no manual annotation:
   `tests_passed`, `build_ok`, `lint_ok`, `edit_kept`, `steered` (negative),
   `stopped` (negative). Build preference pairs (chosen = accepted/passing,
   rejected = reverted/failed for the same task).
3. **Curate.** Assemble versioned datasets: SFT (imitate good trajectories) +
   preference (chosen vs rejected). Snapshot each so every model is reproducible.
4. **Train.** QLoRA/LoRA adapter passes on the open base. Cheap, interruptible,
   fits consumer VRAM. Heavy jobs → 3090; light jobs → NAS cards.
5. **Eval-gate (champion/challenger).** A frozen set of the owner's real tasks. A new
   adapter only becomes the served model **if it beats the current champion**.
6. **Promote & serve.** Winning adapter becomes champion; served back to clients.
7. **Repeat — forever.**

### 6.1 Two kinds of "learning" (use both)

- **Runtime memory / RAG (no training).** The app remembers past sessions,
  preferences, and codebase facts and feeds them back in context. *Feels* like instant
  learning; costs no training.
- **Weight-baking (offline training).** The guidance server bakes recent experience
  into a LoRA adapter the app hot-swaps in. *Real* learned weights delivered as a
  cheap file.

A real learning environment uses both: instant memory at runtime + slow weight-baking
in the background.

### 6.2 Guardrails against model collapse

Training a model on its own raw outputs degrades it. Mitigations are non-negotiable:

- Train SFT **only on verified-good** trajectories (tests/lint pass, edit kept).
- Keep failures only as the **rejected** half of preference pairs — never as positives.
- **Never skip the eval gate.** It is what stops the model from rotting.
- Expect specialization, not general genius: one person's usage is a *small* dataset.

## 7. Idle vs. active scheduling

The "train while idle AND while I use the PC" requirement is solved by putting the
work on **different machines**, not by time-slicing one:

- **NAS (24/7):** small fine-tunes, serving the champion, running the eval suite.
  Never competes with the desktop.
- **Desktop 3090 (idle-gated):** heavy jobs only when the desktop is idle.
  - Idle detector = last-input time + GPU utilization + power state.
  - Training runs in **short, checkpointed micro-batches** so it pauses within seconds
    on input and resumes exactly where it left off.
  - VRAM budget guard evicts the trainer if free VRAM drops below a threshold.

So *something* always makes progress: continuous light work on the NAS, heavy bursts
on the 3090 overnight. Everything lands on the 40 TB RAID10.

**Abstraction:** a **job queue on the NAS + pluggable, idle-aware GPU workers.** Start
with the 3090 worker; later add a second PC or a rented cloud GPU without design
changes.

## 8. Adopt the body, build the upbringing

The hard part of an OS agent (the "body") is largely solved and openly licensed.
**Adopt it.** The novel, valuable part (the "upbringing" — the trust ladder + learning
loop) is what we build.

### 8.1 The two ways to operate any OS (use both)

- **Vision-based** (screenshots + mouse/keyboard, e.g. UI-TARS): maximally portable
  across OSes/apps; local 7B is less reliable on long tasks. Best for *generalization*
  and for the **Manipulate/Execute** rungs.
- **Accessibility-based** (reads the UI tree, e.g. agent-cu): deterministic, reliable,
  reads element *state*, zero vision tokens; needs per-OS backends (UIA / AX /
  AT-SPI), which agent-cu already implements. Best for the **Observe** rung — the
  safest, richest way to capture "what's on screen" demonstration data.

## 9. Recommended component stack (all permissive licenses)

| Layer | Use | Project | License |
| --- | --- | --- | --- |
| **Starting policy (model)** | K8ti's base weights, to fine-tune | **UI-TARS-7B** (ByteDance-Seed) | **Apache-2.0** (code *and* weights) |
| **OS control — Observe** | Accessibility capture (deterministic) | **agent-cu** (kortix-ai) | **MIT** |
| **OS control — Manipulate/Execute** | Vision + mouse/keyboard | **UI-TARS** | **Apache-2.0** |
| **Local engine** | Serve/infer the policy | **Ollama / llama.cpp** (or **vLLM**) | MIT / Apache-2.0 |
| **Cross-platform shell** | One app, all OSes | **Avalonia** (reuse from StayVibin) | **MIT** |
| **Eval environments** | Grade OS-agent ability | **OSWorld** / **WindowsAgentArena** | Apache-2.0 / MIT |
| **Execute sandbox** | Hand over the "match" safely | a disposable **VM** (NAS already runs VMs) | n/a |
| **The learning loop** | Collect→score→train→gate→serve | **K8ti (this repo)** | own (MIT-style) |

**Standout:** UI-TARS is special because **both its code and its model weights are
Apache-2.0** — you can fine-tune the 7B weights through K8ti's trust ladder and
redistribute (or keep) the result however you like. Apache-2.0 also adds a patent
grant MIT lacks.

## 10. Data lake schema (on the 40 TB RAID10)

This is the irreplaceable asset. Suggested layout:

| Store | Contents | Notes |
| --- | --- | --- |
| **Raw events** | Every session, appended JSONL | Cheap, immutable, replayable |
| **Trajectories** | Normalized task → steps → final diff → outcome | Derived from raw events |
| **Auto-labels** | `tests_passed`, `build_ok`, `lint_ok`, `edit_kept`, `steered`, `stopped` | Free reward signals |
| **Datasets** | Versioned SFT + preference sets | Snapshotted for reproducibility |
| **Adapter/checkpoint registry** | LoRA adapter library + lineage | Which base, dataset version, eval score |
| **Eval sets + results** | Frozen "does it work for me" tasks + scores | The promotion gate |
| **Model registry** | Champion pointer + lineage | Which adapter is live, and why |

## 11. Safety & ethics

- **Sandbox-first execution.** Rung 3 always proves itself in a disposable VM (no
  network, snapshot rollback) before touching the real machine.
- **Reversibility everywhere.** Git, filesystem snapshots, copy-on-write.
- **Allowlists / denylists** for commands; **human-in-the-loop** for high-risk ops.
- **Demotion** on regression or unsafe behavior.
- **Ethical-use stance** consistent with StayVibin's license (no harm: disinformation,
  non-consensual deepfakes, phishing, autonomous weapons, malicious cyber-ops, illegal
  activity).

## 12. Licensing (the stack is deliberately permissive)

Everything recommended is **MIT or Apache-2.0**, mutually compatible and compatible
with keeping K8ti under the owner's own (MIT-style) terms. Nothing forces this project
open; the fine-tuned K8ti weights remain fully owned.

- ✅ Build a product on: UI-TARS (Apache-2.0, code+weights), agent-cu (MIT),
  Ollama/llama.cpp (MIT), vLLM (Apache-2.0), Avalonia (MIT), OSWorld (Apache-2.0),
  WindowsAgentArena (MIT).
- ⚠️ **Avoid as a base:** **Open Interpreter** and **aiden** are **AGPL-3.0** — a
  *network copyleft* license. If you build on them and expose the result over a network
  (even a home server API), you must release your **entire derivative** under AGPL, and
  it is **incompatible with keeping K8ti MIT**. You may run them as separate standalone
  tools, but do not fork them into the core.

## 13. Relationship to StayVibin

StayVibin (the owner's local AI vibe-coder, Avalonia/.NET + bundled engine) already
solved the hard local-engine plumbing: engine lifecycle, VRAM-aware context fitting,
model store, tool-call translation. It was **not** designed with learning in mind.

Two viable client paths for the K8ti **environment**:

1. **Extend StayVibin** into the environment (add: feedback capture + memory + a NAS
   client). Reuses all the engine plumbing; coding tasks give the best *automatic*
   reward signal (tests/build/lint). Lower effort.
2. **A new/general OS-agent client** (e.g. fork UI-TARS-desktop) for non-coding,
   whole-OS tasks. Broader reach, but you lose automated-test rewards as the easy
   signal.

**Recommendation:** start the environment around **coding tasks** (best self-grading),
whether via StayVibin or a thin new client. K8ti the *project* (guidance server +
trust ladder) stays independent of whichever client(s) feed it.

## 14. Phased roadmap

| Phase | Deliverable | GPU? | Outcome |
| --- | --- | --- | --- |
| **v0 — Observe collector + data lake** | Ingest API + RAID10 schema + auto-scorer (tests/build/lint). Client tees events. | No | Every session becomes labeled training data immediately |
| **v1 — Job queue + 3090 idle worker** | Worker protocol, idle detection, checkpoint-to-NAS, preempt-on-input | Yes | Heavy training runs safely only when desktop is idle |
| **v2 — Training + eval gate** | First QLoRA pass, champion/challenger swap, serve champion to clients | Yes | The loop is closed; K8ti improves on its own |
| **v3 — Rung 2 (Manipulate)** | Approval-gated reversible edits + DPO from accept/reject | Yes | K8ti earns the right to propose changes |
| **v4 — Rung 3 (Execute)** | Sandbox-VM execution + RL on test-pass reward, then guarded real access | Yes | K8ti earns the match |

**Start point: v0.** It needs no GPU, is pure CPU/IO (the NAS's strongest suit), is
zero-risk, and begins accumulating the one thing you can't rush — your data.

## 15. Open questions

1. **Client choice for v0:** extend StayVibin, or a thin new collector client?
2. **Base model pin:** UI-TARS-1.5-7B vs 7B-DPO vs a 2B for the NAS cards?
3. **Hypervisor specifics** on the NAS (Proxmox / TrueNAS Scale / Unraid) for the
   guidance VM and the execute-sandbox VM, and whether either NAS GPU can be passed
   through.
4. **Networking:** LAN-only (simplest) vs Tailscale/VPN for off-site access.
5. **Eval-set seeding:** which ~30–50 real tasks form the first promotion gate?

## 16. Glossary

- **SFT** — Supervised fine-tuning: train the model to imitate good examples.
- **LoRA / QLoRA** — Cheap fine-tuning that trains small adapter weights (QLoRA = on a
  4-bit quantized base) instead of the whole model.
- **DPO / KTO** — Preference optimization: train from "this output is better than that
  one" pairs (e.g. accepted vs reverted edits).
- **Champion / challenger** — Keep the current best model (champion) serving; a newly
  trained candidate (challenger) only replaces it if it wins on a held-out eval.
- **Imitation / apprenticeship learning** — Learn by copying an expert's actions.
- **Curriculum learning** — Train on easy/safe tasks first, then harder ones.
- **Graduated autonomy / capability gating** — Grant privileges only after competence
  is demonstrated.
- **Model collapse** — Degradation from training on a model's own unfiltered outputs.
