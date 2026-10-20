# Cloud agent notes (HCDE migration)

## Chunk landing policy

- **One port commit per chunk**: all `csharp/src` VM work for a chunk lands in a single commit before tally/docs. Do not `git commit --amend` or `git reset` (soft/hard) to rewrite chunk port history, locally or after push.
- **Coverage before push**: `csharp/tools/check_port_coverage.sh <N>` must pass on that port commit (`conversionManagedCommit` = port SHA).
- **Re-audit**: `python3 tools/reaudit_chunks.py audit --from N --to N` must be **0 hard / 0 soft** under the strict managed gate (`commit_ml` from `conversionManagedCommit` only).
- **Chunk size**: 150–400 native LOC, whole-function real ports (not padded to 150).
