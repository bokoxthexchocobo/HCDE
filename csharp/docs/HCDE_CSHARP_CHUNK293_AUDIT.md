# Chunk 293 — MBF scratch sound index offset

## Scope

MBF scratch sound index offset

## Native ranges

- `src/gamedata/d_dehacked.cpp` **MBF emitter helpers** 936–1085 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 3).

## Managed

`VmDehackedMbfParamEmitters.cs`, `VmDehackedMbfParamEmittersTests.cs`
Managed LOC (tally): 12.
Conversion commit: `d71fc5180bffd0a87b42358d14b2218680c3e033`.

## Tests

- `VmDehackedMbfParamEmittersTests.ScratchSoundIndex_OffsetsByOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 293 --to 293` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
