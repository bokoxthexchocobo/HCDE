# Chunk 528 — AActor::CheckLOF

## Scope

AActor::CheckLOF

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::CheckLOF** 2358–2517 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmActionCheckLof.cs`
Managed LOC (tally): 87.
Conversion commit: `09d62a67d9a51b4c56ca2ae554e6f0bce26b0c47`.

## Tests

- `VmActionCheckLofTests.TargetNullWithoutAllow_FailsWhenStrict`
- `VmActionCheckLofTests.RangeExceeded_IgnoresPartialFlag`
- `VmActionCheckLofTests.DefaultRange_PlayerUsesPlayerMissileRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 528 --to 528` (0 hard / 0 soft).

## Seams

`Trace`, `CheckLOFTraceFunc`, `COPY_AAPTR`, and `ACTION_RETURN_BOOL` are not hosted.

## Credit rationale

Credits CheckLOF offset scaling, range gates, aim flags, trace success policy, and actor link flags from `p_actionfunctions.cpp` (2358–2517).
