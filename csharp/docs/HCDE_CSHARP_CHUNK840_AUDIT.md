# Chunk 840 — Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **117** (< **150** floor). Prior tally **163** included banned separator padding. Matrix credit **0**.

## Scope

Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **117** (< **150** floor). Prior tally **163** included banned separator padding. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `beeda8d8b961e79b3ecc269b992b569e3d8382dd`.

## Padding review (2026-10-21)

### Separator uncredit → retire

| | `nativeLoc` |
|--|----------:|
| **Before** (incl. separators) | **163** |
| **After uncredit** (functions only) | **117** |
| **After retire** (matrix) | **0** |

Removed **46** separator lines across **6** `nativeRanges` entries.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 840 --to 840` (0 hard / 0 soft).

## Seams

Mobj destroy, massacre damage, coords, and `EV_Teleport` are host callbacks on `ParserContext` (`VmFScriptParserSfObj.cs`); `ScriptValue.FixedValue` holds `svt_fixed` returns.

## Credit rationale

Separator padding between `SF_*` bands must not count toward `nativeLoc`. After uncredit, band is below floor → retire (behavior may remain off-tally).
