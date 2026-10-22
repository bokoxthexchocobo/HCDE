# Verify gate proof — 2026-10-22 (HEAD `c30a06e5`)

Hardened script: `csharp/tools/verify_full_solution_tests.sh` (`PIPESTATUS[0]`, build CS scan, 14 parsed `OK` rows).

| Step | Command | Expected | Result |
|------|---------|----------|--------|
| Fail | Scratch `VerifyGateScratchTests.GateProof_IntentionalFailure` → `Assert.Fail` (not committed) | exit **1** | **1** — log `/opt/cursor/artifacts/gate_proof_fail_2026-10-22.log` |
| Pass | Clean HEAD after removing scratch | exit **0**, 14/14 parsed | **0** — log `/opt/cursor/artifacts/gate_proof_pass_2026-10-22.log`, Playsim **18806** |

**Red commit (798 tally):** `b2c9e283` — `VmAFloorTickElevatorTests` used `TickContext` (CS0426); hardened verify fails build at that tree.

**Historical (hardened script in worktree):** `b5c017e6` PASS (18773); `ffde316c` PASS (18799); `b2c9e283` FAIL.
