# Cloud agent notes (HCDE migration)

## Chunk landing policy

- **One port commit per chunk**: all `csharp/src` VM work for a chunk lands in a single commit before tally/docs. Do not `git commit --amend` or `git reset` (soft/hard) to rewrite chunk port history, locally or after push.
- **Coverage before push**: `csharp/tools/check_port_coverage.sh <N> <port-sha>` must pass on the port commit (`conversionManagedCommit` = full 40-char port SHA).
- **Re-audit**: `python3 tools/reaudit_chunks.py audit --from N --to N` must be **0 hard / 0 soft** under the strict managed gate (`commit_ml` from `conversionManagedCommit` only).
- **Chunk size**: 150–400 native LOC, whole-function real ports (not padded to 150).
- **One TU per chunk**: Never merge native ranges from different translation units. Never credit enums, struct declarations, or comment-only lines. If a TU remainder is &lt;150 native LOC in whole functions, leave it uncredited.
- **Tests before push**: `bash csharp/tools/verify_full_solution_tests.sh` must pass (all **14** projects, Playsim ~18.7k+, MigrationMatrix 8/8, **0** failed). Do not trust `dotnet test` exit code alone. **No `git push` on failure.** Sub-agents must use this gate.
