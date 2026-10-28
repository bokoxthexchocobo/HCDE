# Chunk 839 — Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **125** (< **150** floor). Prior tally **155** included banned separator padding. Matrix credit **0**.

## Scope

Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **125** (< **150** floor). Prior tally **155** included banned separator padding. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f5519b429d99f20bdd6026f880aef0dfd5c5de66`.

## Padding review (2026-10-21)

### Separator uncredit → retire

| | `nativeLoc` |
|--|----------:|
| **Before** (incl. separators) | **155** |
| **After uncredit** (functions only) | **125** |
| **After retire** (matrix) | **0** |

Removed **30** separator lines across **4** `nativeRanges` entries.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 839 --to 839` (0 hard / 0 soft).

## Seams

Player display name, spawn, placement test, and failed-spawn destroy are host callbacks on `ParserContext` (`VmFScriptParserSfPlayer.cs` partial only).

## Credit rationale

Separator padding between `SF_*` bands must not count toward `nativeLoc`. After uncredit, band is below floor → retire (behavior may remain off-tally).
