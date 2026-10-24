# Chunk 841 — Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **119** (< **150** floor). Prior tally **160** included banned separator padding. Matrix credit **0**.

## Scope

Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **119** (< **150** floor). Prior tally **160** included banned separator padding. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ced02889d9116971d5abe2f44a7e5cacf146f09f`.

## Padding review (2026-10-21)

### Separator uncredit → retire

| | `nativeLoc` |
|--|----------:|
| **Before** (incl. separators) | **160** |
| **After uncredit** (functions only) | **119** |
| **After retire** (matrix) | **0** |

Removed **41** separator lines across **6** `nativeRanges` entries.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 841 --to 841` (0 hard / 0 soft).

## Seams

Teleport, damage, sector tag, and actor flags are host callbacks on `ParserContext` (`VmFScriptParserSfObjState.cs` only).

## Credit rationale

Separator padding between `SF_*` bands must not count toward `nativeLoc`. After uncredit, band is below floor → retire (behavior may remain off-tally).
