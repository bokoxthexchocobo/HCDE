# Playsim / Scripting build break (bisect)

**Breaking commit:** `e53a221b` — *fix: natural 597/601 credit, padding purge 450+, Net_DoCommand 602-603*

**Last good:** `e53a221b^` (`dotnet build csharp/HCDE.sln` succeeds)

**Cause:** `strip_all_padding.py` / `prune_padding_all_chunks.py` removed ~1.2k `Vm*` / ZCC helpers that remaining call sites still invoke (no matching caller updates).

**Remediation (this branch):** Restore `csharp/src/HCDE.Playsim` and `csharp/src/HCDE.Scripting` from `e53a221b^`, keep post-purge chunk/tally work. Re-apply `60a57c5e` playsim edits (`VmAcsDoSetActorProperty.cs`, `VmPMapTraceBleed.cs`) only.

**Do not re-run** wholesale padding strip without a green full-solution build gate.
