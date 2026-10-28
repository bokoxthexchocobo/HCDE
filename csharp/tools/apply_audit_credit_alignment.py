#!/usr/bin/env python3
"""Deprecated wrapper: use shrink_chunk_credit_to_audit.py (shrink-only, no map backfill)."""
import subprocess
import sys

if __name__ == "__main__":
    args = sys.argv[1:]
    if "--backfill-zero" in args:
        print("error: --backfill-zero removed (never auto-widen audit maps)", file=sys.stderr)
        raise SystemExit(2)
    if "--align" in args:
        args = [a if a != "--align" else "--apply" for a in args]
    cmd = [sys.executable, str(__file__).replace("apply_audit_credit_alignment.py", "shrink_chunk_credit_to_audit.py")] + args
    raise SystemExit(subprocess.call(cmd))
