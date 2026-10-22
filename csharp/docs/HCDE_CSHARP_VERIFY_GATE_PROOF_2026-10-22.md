# Verify gate proof — 2026-10-22

**Recorded in git:** commit **`cf124226`** (`Document hardened verify gate proof and b2c9e283 red commit`).  
**Audit cross-ref:** `csharp/docs/HCDE_CSHARP_SESSION_2026_10_22_AUDIT.md` (gate hardening + proof logs).

## Re-run at branch HEAD (2026-10-22, post chunk 802)

Scratch `VerifyGateScratchTests.GateProof_IntentionalFailure` → `Assert.Fail` was added **only in the worktree** (never committed).

| Step | Result | Log |
|------|--------|-----|
| Fail | exit **1** | `/opt/cursor/artifacts/gate_proof_head_fail_2026-10-22.log` |
| Pass | exit **0**, 14/14 parsed | `/opt/cursor/artifacts/gate_proof_head_pass_2026-10-22.log` |

Earlier same-day logs: `gate_proof_fail_2026-10-22.log`, `gate_proof_pass_2026-10-22.log`, `gate_proof_fail_rerun_2026-10-22.log`, `gate_proof_pass_rerun_2026-10-22.log`.

---

## Original proof (HEAD `c30a06e5`)

Hardened script: `csharp/tools/verify_full_solution_tests.sh` (`PIPESTATUS[0]`, build CS scan, 14 parsed `OK` rows).

| Step | Command | Expected | Result |
|------|---------|----------|--------|
| Fail | Scratch `VerifyGateScratchTests.GateProof_IntentionalFailure` → `Assert.Fail` (not committed) | exit **1** | **1** — log `/opt/cursor/artifacts/gate_proof_fail_2026-10-22.log` |
| Pass | Clean HEAD after removing scratch | exit **0**, 14/14 parsed | **0** — log `/opt/cursor/artifacts/gate_proof_pass_2026-10-22.log`, Playsim **18806** |

**Red commit (798 tally):** `b2c9e283` — `VmAFloorTickElevatorTests` used `TickContext` (CS0426); hardened verify fails build at that tree.

**Historical (hardened script in worktree):** `b5c017e6` PASS (18773); `ffde316c` PASS (18799); `b2c9e283` FAIL.
