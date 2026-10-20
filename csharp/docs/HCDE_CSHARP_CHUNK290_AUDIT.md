# Chunk 290 — DoInclude notext, quote, and patch-directory combine

## Scope

DoInclude notext, quote, and patch-directory combine

## Native ranges

- `src/gamedata/d_dehacked.cpp` **DoInclude paths** 3437–3586 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 21).

## Managed

(see conversion commit file list)
Managed LOC (tally): 38.
Conversion commit: `24224a5ff436e2a5d7ae2df67a8264842a99455a`.

## Tests

- `VmDehackedIncludeDirectiveTests.TryCombinePatchDirectory_PrefixesFolder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 290 --to 290` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
