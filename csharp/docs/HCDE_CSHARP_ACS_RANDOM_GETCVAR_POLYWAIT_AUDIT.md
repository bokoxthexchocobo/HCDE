# Principal audit — ACS Random, GetCvar, PolyWait

**Date:** 2026-10-19  
**Scope:** Native `p_acs.cpp` `PCD_RANDOM`, `PCD_RANDOMDIRECT`, `PCD_RANDOMDIRECTB`, `PCD_GETCVAR`, `PCD_POLYWAIT`, `PCD_POLYWAITDIRECT`.  
**Result:** Accepted for this slice. `PCD_RANDOMDIRECTB` is implemented but not bytecode-regression tested (unaligned tail operands).

## Native behavior

- `Random(min,max)` uses inclusive range with swapped ends when `max < min`, drawing from `pr_acs` (`FRandom` name `"ACS"`).
- `GetCvar` resolves the activator player (or console player) and applies `DoGetCVar` float fixed-point vs integer rules.
- `PolyWait` waits while `PO_Busy` is true; with no polyobject simulation, `PO_Busy` is always false and the script continues.

## Managed implementation

- `AuthoritySimulation.NextAcsRandom` seeds `NativeStateRandom` with CRC `0xa72f226d` (`"ACS"`).
- `AcsVm` executes opcodes 57/58/174, 255, 63/64.
- `ConsoleVariables.TryResolve` exposes float kind for ACS encoding.

## Divergences

- Client-side `pr_csacs` is not modeled (server ACS only).
- `PolyWait` is a no-op wait, not a true polyobject busy poll.
- `PCD_RANDOMDIRECTB` lacks a loaded-map regression (operand alignment differs from word-aligned tail in tests).

## Test evidence

`AcsRandomAndCvarOpcodeTests` (5 cases) plus existing suite:

```text
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release --filter FullyQualifiedName~AcsRandomAndCvarOpcodeTests
```

All five passed in this run.

## LOC tally

`LOC tally: converted this run = 38 native lines (98 C# lines added); native remaining = 508,048`
