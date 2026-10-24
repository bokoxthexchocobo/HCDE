# Chunk 309 — LoadDehSupp lump gate

## Scope

LoadDehSupp lump gate

## Native ranges

- `src/gamedata/d_dehacked.cpp` **LoadDehSupp open gate** 3474–3491 (18 lines)

**Credited native LOC:** 18 (tally `nativeLoc` 10).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 40.
Conversion commit: `b11c0edaddf796acd4599fa988cd8f31a6fef7ea`.

## Tests

- `VmDehackedDehSuppLumpGateTests.TryOpenDehsupp_AcceptsCoreLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 309 --to 309` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
