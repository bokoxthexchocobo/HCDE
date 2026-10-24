# Chunk 310 — LoadDehSupp ActionList

## Scope

LoadDehSupp ActionList

## Native ranges

- `src/gamedata/d_dehacked.cpp` **ActionList** 3511–3546 (36 lines)

**Credited native LOC:** 36 (tally `nativeLoc` 36).

## Managed

`VmDehackedDehSuppActionList.cs`
Managed LOC (tally): 40.
Conversion commit: `7275271bf614da3f21540bebed526217515b32e2`.

## Tests

- `VmDehackedDehSuppActionListTests.ParseActionList_AcceptsNullAndPointer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 310 --to 310` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
